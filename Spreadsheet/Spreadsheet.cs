using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

namespace SpreadsheetUtilities
{
/// <summary>
/// Internal spreadsheet class that keeps track of all
/// cell data
/// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        private Dictionary<string, Cell> _spreadsheetCells;
        private DependencyGraph _variableDependencies;
        private object editKey = new object();
        /// <summary>
        /// Determines whether or not the spreadsheet has been updated
        /// </summary>
        public override bool Changed { get; protected set; }

        /// <summary>
        /// Default constructor. Will accept any variable and will 
        /// not normalize any variables. Version is set to "default"
        /// </summary>
        public Spreadsheet() : base(v => true, n => n, "default")
        {
            _spreadsheetCells = new Dictionary<string, Cell>();
            _variableDependencies = new DependencyGraph();
            Changed = false;
        }
        /// <summary>
        /// Constructor for a new spreadsheet
        /// </summary>
        /// <param name="isValid">Function that determines which variables are considered valid</param>
        /// <param name="normalize">Function that normalizes all variables</param>
        /// <param name="version">Version name or number of the spreadsheet</param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            _spreadsheetCells = new Dictionary<string, Cell>();
            _variableDependencies = new DependencyGraph();
            Changed = false;
        }/// <summary>
         /// Constructor for an existing spreadsheet with a .sprd extension
         /// </summary>
         /// <param name="filename">Spreadsheet filename</param>
         /// <param name="isValid">Function that determines which variables are considered valid</param>
         /// <param name="normalize">Function that normalizes all variables</param>
         /// <param name="version">Version name or number of the spreadsheet</param>
        public Spreadsheet(string filename, Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            _spreadsheetCells = new Dictionary<string, Cell>();
            _variableDependencies = new DependencyGraph();

            if (GetSavedVersion(filename) != Version)
                throw new SpreadsheetReadWriteException("Filename version \'" + GetSavedVersion(filename) + "\' does not match spreadsheet version \'" + Version + "\'");
            try
            {
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement() && reader.Name == "cell")
                        {
                            reader.ReadToDescendant("name");
                            reader.Read();
                            string cellName = reader.Value;
                            reader.ReadToFollowing("contents");
                            reader.Read();
                            string cellContents = reader.Value;
                            this.SetContentsOfCell(cellName, cellContents);
                        }
                    }
                }
                Changed = false;
            }
            catch (Exception ex)
            {
                throw new SpreadsheetReadWriteException(ex.Message);
            }

        }

        /// <summary>
        /// Returns the contents of a passed cell
        /// </summary>
        /// <param name="name">Name of the cell</param>
        /// <returns>String, Double, or Formula</returns>
        public override object GetCellContents(string name)
        {
            lock (editKey)
            {
                name = Normalize(name);
                CheckValidity(name);

                if (_spreadsheetCells.ContainsKey(name))
                    return _spreadsheetCells[name].Contents;
                else
                    return string.Empty; 
            }
        }
        /// <summary>
        /// Returns all the names of nonempty cells
        /// </summary>
        /// <returns>HashSet</returns>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            lock (editKey)
            {
                return new HashSet<string>(_spreadsheetCells.Keys);   //return all used cells 
            }
        }
        /// <summary>
        /// Sets cell to a formula
        /// </summary>
        /// <param name="name">Cell name</param>
        /// <param name="formula">Cell's new value</param>
        /// <returns>SortedSet</returns>
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {
            lock (editKey)
            {
                CheckValidity(name, formula);

                IEnumerable<string> oldDependencies;                    //keep track of older dependencies just in case they need to be restored
                RemovePreviousDependencies(name, out oldDependencies);                                   //remove previous dependencies iff the old cell was a formula
                object oldCellContents = GetCellContents(name);
                //store old cell's contents just in case it needs to restore it
                Cell oldCell = null;

                if (oldCellContents.GetType() == typeof(string))
                {
                    oldCell = new Cell(this, name, (string)oldCellContents);
                }
                else if (oldCellContents.GetType() == typeof(double))
                {
                    oldCell = new Cell(this, name, (double)oldCellContents);
                }
                else if (oldCellContents.GetType() == typeof(Formula))
                {
                    oldCell = new Cell(this, name, (Formula)oldCellContents);
                }
                Cell newCell = new Cell(this, name, formula);

                _spreadsheetCells[name] = newCell;                                  //add or overwrite cell in the spreadsheet
                AddNewDependencies(name, formula);                                  //add new dependencies 

                try
                {
                    foreach (string var in formula.GetVariables())      //check to make sure that there are no circular dependencies within current formula(ex: new cell A1 = A1 + B5)
                    {
                        if (var == name)
                            throw new CircularException("Cell \'" + name + "\' contains a variable reference to itself");
                    }

                    IEnumerable<string> cellDependencies = _variableDependencies.GetDependents(name);   //all of the new cells dependencies
                    IEnumerable<string> uncalculatedCells = GetCellsToRecalculate(new SortedSet<string>(cellDependencies));   //all uncalculated cells that must be recalculated
                    ReevaluateCellDependencies(uncalculatedCells);

                    SortedSet<string> returnedSet = new SortedSet<string>(uncalculatedCells);    //get all of the direct and indirect dependents of cell
                    returnedSet.Add(name);                                                  //add new cell name to returned Set


                    return returnedSet;
                }
                catch (CircularException ex)
                {
                    //revert any changes
                    _spreadsheetCells[name] = oldCell;                                  //reset cell to its original cell
                    AddNewDependencies(name, oldDependencies);                          //reset cells dependencies
                    throw new CircularException(ex.Message);                            //rethrow exception
                }


            }
        }
        /// <summary>
        /// Sets cell to a string
        /// </summary>
        /// <param name="name">Cell name</param>
        /// <param name="text">Cell's new value</param>
        /// <returns>SortedSet</returns>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            lock (editKey)
            {
                CheckValidity(name, text);

                RemovePreviousDependencies(name);                                   //remove previous dependencies iff the old cell was a formula

                if (text == string.Empty)                                           //cell is considered empty if its string text is empty
                {
                    _spreadsheetCells.Remove(name);                                 //remove the cell from the internal dictionary because it no longer exists
                    return new SortedSet<string>();                                 //return an empty set
                }
                else
                {
                    Cell newCell = new Cell(this, name, text);
                    _spreadsheetCells[name] = newCell;                              //add or overwrite cell          

                }

                //need to re-evaluate all dependencies of new cell because it was just changed
                IEnumerable<string> cellDependencies = _variableDependencies.GetDependents(name);   //all of the new cells dependencies
                IEnumerable<string> uncalculatedCells = GetCellsToRecalculate(new SortedSet<string>(cellDependencies));   //all uncalculated cells that must be recalculated
                ReevaluateCellDependencies(uncalculatedCells);

                SortedSet<string> returnedSet = new SortedSet<string>(uncalculatedCells);    //get all of the dependees of cell 'name'
                returnedSet.Add(name);                                                  //add new cell name to returned Set
                return returnedSet; 
            }
        }
        /// <summary>
        /// Sets cell to a double
        /// </summary>
        /// <param name="name">Cell name</param>
        /// <param name="number">Cell's new value</param>
        /// <returns>SortedSet</returns>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            lock (editKey)
            {
                CheckValidity(name);

                RemovePreviousDependencies(name);                                   //remove previous dependencies iff the old cell was a formula

                Cell newCell = new Cell(this, name, number);
                _spreadsheetCells[name] = newCell;                                  //add or overwrite cell in the spreadsheet


                //need to re-evaluate all dependencies of new cell because it was just changed
                IEnumerable<string> cellDependencies = _variableDependencies.GetDependents(name);   //all of the new cells dependencies
                IEnumerable<string> uncalculatedCells = GetCellsToRecalculate(new SortedSet<string>(cellDependencies));   //all uncalculated cells that must be recalculated
                ReevaluateCellDependencies(uncalculatedCells);

                SortedSet<string> returnedSet = new SortedSet<string>(uncalculatedCells);    //get all of the dependees of cell 'name'
                returnedSet.Add(name);                                                  //add new cell name to returned Set
                return returnedSet; 
            }
        }
        /// <summary>
        /// Returns all cells that directly depend on the passed cell
        /// </summary>
        /// <param name="name">Cell name</param>
        /// <returns>SortedSet</returns>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            lock (editKey)
            {
                CheckValidity(name);
                return _variableDependencies.GetDependents(name); 
            }
        }
        /// <summary>
        /// Returns the spreadsheet version within the save file
        /// </summary>
        /// <param name="filename">Saved spreadsheet's filename</param>
        /// <returns></returns>
        public override string GetSavedVersion(string filename)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    if (reader.ReadToFollowing("spreadsheet"))
                    {
                        string saveFileVersion = reader.GetAttribute("version");
                        return saveFileVersion;
                    }
                    else
                        throw new SpreadsheetReadWriteException("Spreadsheet element could not be found");  //cant find spreadsheet element, therefore throw and error
                }
            }
            catch (Exception ex)
            {
                throw new SpreadsheetReadWriteException(ex.Message);    
            }
        }
        /// <summary>
        /// Saves the spreadsheet. Uses '.sprd' file extension
        /// </summary>
        /// <param name="filename">Spreadsheet's filename</param>
        public override void Save(string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");

            try
            {
                using (XmlWriter writer = XmlWriter.Create(filename, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("version", Version);
                    foreach (Cell c in _spreadsheetCells.Values)
                    {
                        if (c != null && IsValidSpreadsheetVariable(c.Name) && IsValid(c.Name))
                        {
                            c.WriteXml(writer);
                        }
                        else
                        {
                            throw new SpreadsheetReadWriteException("Invalid variable \'" + c.Name + "\' found within the spreadsheet");
                        }
                    }
                    Changed = false;
                    writer.WriteEndElement();
                    writer.Close();
                }
            }
            catch(Exception ex)
            {
                throw new SpreadsheetReadWriteException(ex.Message);
            }
        }
        /// <summary>
        /// Returns the value of the passed cell
        /// </summary>
        /// <param name="name">Cell name</param>
        /// <returns></returns>
        public override object GetCellValue(string name)
        {
            lock (editKey)
            {
                CheckValidity(name);

                if (_spreadsheetCells.ContainsKey(name))
                    return _spreadsheetCells[name].Value;
                else
                    return string.Empty; 
            }
        }
        /// <summary>
        /// Public method used to modify the value and contents of a
        /// cell
        /// </summary>
        /// <param name="name">Cell name</param>
        /// <param name="content">Cell's new contents</param>
        /// <returns></returns>
        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            lock (editKey)
            {
                name = Normalize(name);                                         //first, normalize the name before checking its validity

                double contentAsDouble;
                if (double.TryParse(content, out contentAsDouble))
                {
                    CheckValidity(name, content);
                    Changed = true;
                    return SetCellContents(name, contentAsDouble);                      //if context is a string version of a double, add it to the spreadsheet as a parsed double
                }
                else if (content.StartsWith("="))                             //check if it starts with equals
                {
                    Changed = true;
                    return SetCellContents(name, new Formula(content.Substring(1), Normalize, IsValidSpreadsheetVariable + IsValid));    //creates a formula with the content string minus the beginning equals, the spreadsheets normalize delegate, and a combination of the spreadsheets two validator delegates
                }
                else
                {
                    CheckValidity(name, content);
                    Changed = true;
                    return SetCellContents(name, content);                              //must be a plain string
                } 
            }
        }


        //========================================================
        //          Private Helper Functions and Classes
        //========================================================

        private void CheckValidity(string var)
        {
            if (var == null)
                throw new ArgumentNullException("variable name has a value of null");
            if (!IsValid(var) || !IsValidSpreadsheetVariable(var))
                throw new InvalidNameException("variable \'" + var + "\' is not considered a valid variable");
        }
        private void CheckValidity(string var, object contents)
        {
            var = Normalize(var);
            if (contents == null)
                throw new ArgumentNullException("the cell contents have a value of null");
            if(var == null)
                throw new InvalidNameException("variable contents \'" + var + "\' has a value of null");
            if (!IsValid(var) || !IsValidSpreadsheetVariable(var))
                throw new InvalidNameException("variable \'" + var + "\' is not considered a valid variable");
        }
        private double SpreadsheetLookup(string var)
        {
            if (_spreadsheetCells.ContainsKey(var) && _spreadsheetCells[var].Value.GetType() == typeof(double))
                return (double)_spreadsheetCells[var].Value;
            else
                throw new ArgumentException("variable \'" + var + "\' not found or not valid in spreadsheet");

        }
        private bool IsValidSpreadsheetVariable(string var)
        {
            return Regex.IsMatch(var, @"([a-zA-Z]+[0-9]+)$");
        }
        private void RemovePreviousDependencies(string cellName)
        {
            string[] oldCellDepedees = _variableDependencies.GetDependees(cellName).ToArray();
            if (_spreadsheetCells.ContainsKey(cellName) && _spreadsheetCells[cellName].Contents.GetType() == typeof(Formula)) //if the old cell was a formula, remove all its dependees
            {
                foreach (string dependee in oldCellDepedees)
                {
                    _variableDependencies.RemoveDependency(dependee, cellName); //TODO: check if this is properly implemented
                }
            }
        }
        private void RemovePreviousDependencies(string cellName, out IEnumerable<string> oldDependencies)
        {
            string[] oldCellDepedees = _variableDependencies.GetDependees(cellName).ToArray();
            RemovePreviousDependencies(cellName);
            oldDependencies = oldCellDepedees;
        }
        private void AddNewDependencies(string cellName, Formula newFormula)
        {
            foreach(string var in newFormula.GetVariables())
            {
                _variableDependencies.AddDependency(var, cellName);
            }
        }
        private void AddNewDependencies(string cellName, IEnumerable<string> newDependencies)
        {
            foreach (string var in newDependencies)
            {
                _variableDependencies.AddDependency(var, cellName);
            }
        }     
        private void ReevaluateCellDependencies(IEnumerable<string> dependencies)
        {

            foreach (string var in dependencies)
            {
                CheckValidity(var);

                if (!_spreadsheetCells.ContainsKey(var))        //if the cell doesn't exist, theres no reason to evaluate it
                    continue;

                Cell currentCell = _spreadsheetCells[var];
                if (currentCell.Contents.GetType() == typeof(Formula))      //only reevaluate the cell if its contents are a formula
                {
                    Formula CurrentCellFormula = currentCell.Contents as Formula;           //cast cell content as a formula
                    currentCell.Value = CurrentCellFormula.Evaluate(SpreadsheetLookup);     //set cell's value to the evaluated formula value
                }
            }
        }
        private class Cell
        {
            private Spreadsheet ParentSpreadsheet;
            public string Name { get; private set; }
            public object Value { get; set; }
            public object Contents { get; set; }

            public Cell(Spreadsheet parent, string name)
            {
                ParentSpreadsheet = parent;
                this.Contents = "";         //blank cell
                this.Value = "";
                this.Name = name;
            }
            public Cell(Spreadsheet parent, string name, string text) :
                this(parent, name)
            {
                Contents = text;
                Value = text;
            }
            public Cell(Spreadsheet parent, string name, double number) :
                this(parent, name)
            {
                Contents = number;
                Value = number;
            }
            public Cell(Spreadsheet parent, string name, Formula formula) :
                this(parent, name)
            {
                Contents = formula;
                Value = formula.Evaluate(parent.SpreadsheetLookup);

            }
            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("cell");   //Write cell element
                writer.WriteElementString("name", Name);
                if(Contents.GetType() == typeof(Formula))
                    writer.WriteElementString("contents", "=" + Contents.ToString());
                else
                    writer.WriteElementString("contents", Contents.ToString());
                writer.WriteEndElement();
            }
        }
    }

}
