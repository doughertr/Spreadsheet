using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;

namespace SS
{
    /// <summary>
    /// Example of using a SpreadsheetPanel object
    /// </summary>
    public partial class SpreadsheetGUI : Form
    {
        private Spreadsheet _Spreadsheet;

        private MethodInvoker updateSpreadsheetContents;
        private struct GUICell
        {
            public int col;
            public int row;
            public string name;
            public string value;
            public string contents;
        }
        private GUICell currentCell;
        private GUICell previousCell;
        private string SpreadsheetName;
        /// <summary>
        /// Constructor for the demo
        /// </summary>
        public SpreadsheetGUI(Spreadsheet spread = null, string fileName = "Spreadsheet")
        {
            
            InitializeComponent();
            if(Equals(spread, null))
            {
                _Spreadsheet = new Spreadsheet(x => true, n => n.ToUpper(), "1.0");
            }
            else
            {
                // copies the passed in spreadsheet, then updates the GUI.
                _Spreadsheet = spread;
                foreach(string name in _Spreadsheet.GetNamesOfAllNonemptyCells())
                {
                    spreadsheetPanel.SetValue(name, _Spreadsheet.GetCellValue(name).ToString());
                }
            }
            SpreadsheetName = fileName;
            int LastIndex = SpreadsheetName.LastIndexOf((char)92) + 1;
            string tempstring = SpreadsheetName.Substring(LastIndex);
            this.Text = tempstring;

            // Load the OnExit event handler.
            FormClosing += OnExit;

            // Load the contents of the Help box.
            textBox1.Text = "Welcome to the Help Menu!\r\n\r\n";
            textBox1.Text += "To edit cells, simply click them and start typing!\r\n";
            textBox1.Text += "You can also navigate using the arrow keys.\r\n\r\n";
            textBox1.Text += "To add formulas using multiple cells, simply type '=', followed by the formula.\r\n";
            textBox1.Text += "For example: To make the formula 'A1 + A3', you would type '=A1 + A3'.\r\n\r\n";
            textBox1.Text += "To save your work, go to File -> Save.\r\n\r\n";
            textBox1.Text += "To save your work under a different name, go to File -> Save As\r\n\r\n";
            textBox1.Text += "To Load an existing spreadsheet, go to File -> Load and select the file you want to load.\r\n\r\n";
            textBox1.Text += "To close this menu, click 'Help' again.\r\n\r\n";
            textBox1.Text += "\r\n\r\n";

            // This an example of registering a method so that it is notified when
            // an event happens.  The SelectionChanged event is declared with a
            // delegate that specifies that all methods that register with it must
            // take a SpreadsheetPanel as its parameter and return nothing.  So we
            // register the displaySelection method below.

            // This could also be done graphically in the designer, as has been
            // demonstrated in class.

            this.ActiveControl = cellContentsTextbox;
            this.AcceptButton = submitButton;
            spreadsheetPanel.SelectionChanged += updateCells;

            spreadsheetPanel.SetSelection(2, 3);
            SetCurrentCell();
            cellLabel.Text = currentCell.name;
        }
        /// <summary>
        /// This method is handled by the selection changed event. Sets the current
        /// cell to its new value, reevaluate all cell dependencies, and update 
        /// GUI respectively
        /// </summary>
        /// <param name="ss">current spreadsheetPannel to change</param>
        private void updateCells(SpreadsheetPanel ss)
        {
            cellContentsTextbox.Focus();
            previousCell = currentCell;
            //this method invoker will reevaluate all cell's values and update the GUI accordingly
            updateSpreadsheetContents = new MethodInvoker(
                () =>
                {
                    ISet<string> cellsToUpdate = _Spreadsheet.SetContentsOfCell(previousCell.name, previousCell.contents);
                    foreach (string cell in cellsToUpdate)
                    {
                        spreadsheetPanel.SetValue(cell, _Spreadsheet.GetCellValue(cell).ToString());
                    }
                }
            );

            UpdateSpreadsheetWorker.RunWorkerAsync();
            SetCurrentCell();
            cellContentsTextbox.Text = currentCell.contents;
            spreadsheetPanel.SetValue(currentCell.name, cellContentsTextbox.Text);
            cellLabel.Text = currentCell.name;
            //highlight contents in textbox when changing to another cell
            if (!String.IsNullOrEmpty(cellContentsTextbox.Text))
            {
                cellContentsTextbox.SelectionStart = 0;
                cellContentsTextbox.SelectionLength = cellContentsTextbox.Text.Length;
            }
        }

        /// <summary>
        /// Updates underlying data of spreadsheet class. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        private void updateSpreadsheetData(SpreadsheetPanel ss, string name, string content)
        {

            UpdateSpreadsheetWorker.RunWorkerAsync();
        }

        // Deals with the New menu
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Tell the application context to run the form on the same
            // thread as the other forms.
            SpreadsheetApplicationContext.getAppContext().RunForm(new SpreadsheetGUI());
        }

        // Deals with the Close menu
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// This background worker will reevaluate all dependencies and updates 
        /// spreadsheet GUI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateSpreadsheetWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            updateSpreadsheetContents.Invoke();
        }

        /// <summary>
        /// This function saves the spreadsheet. It is called when the user clicks File -> Save.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // checks if the spreadsheet's name has changed.
            // If it hasn't it means that the file has not been saved
            // If it hasn't been saved before, it saves as
            // otherwise, it overwrites the current spreadsheet.
            if(SpreadsheetName == "Spreadsheet")
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                _Spreadsheet.Save(SpreadsheetName);
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Only looks for .sprd files by default
            openFileDialog1.Filter = ".sprd Spreadsheet File| *.sprd";
            openFileDialog1.Title = "Save a Spreadsheet";
            int LastIndex = openFileDialog1.FileName.LastIndexOf((char)92) + 1;
            openFileDialog1.FileName = openFileDialog1.FileName.Remove(LastIndex);
            openFileDialog1.ShowDialog();

            // If there is no valid filename, doesn't attempt to load.
            if(openFileDialog1.FileName == "")
            {
                return;
            }
            if (!openFileDialog1.CheckFileExists)
            {
                return;
            }
            Spreadsheet newSpreadsheet = new Spreadsheet(openFileDialog1.FileName, x => true, n => n.ToUpper(), "1.0");

            SpreadsheetApplicationContext.getAppContext().RunForm(new SpreadsheetGUI(newSpreadsheet, openFileDialog1.FileName));
        }
        /// <summary>
        /// Shifts the cell selection down by one and calls update
        /// function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void submitButton_Click(object sender, EventArgs e)
        {
            cellContentsTextbox.Clear();
            if (currentCell.row < 99)
            {
                spreadsheetPanel.SetSelection(currentCell.col, currentCell.row + 1);
                currentCell.row = currentCell.row + 1;
            }
            updateCells(spreadsheetPanel);
        }


        private bool SetCurrentCell()
        {
            spreadsheetPanel.GetSelection(out currentCell.name);
            spreadsheetPanel.GetSelection(out currentCell.col, out currentCell.row);
            currentCell.value = _Spreadsheet.GetCellValue(currentCell.name).ToString();
            object cellContents = _Spreadsheet.GetCellContents(currentCell.name);
            if (cellContents.GetType() == typeof(Formula))
                currentCell.contents = "=" + _Spreadsheet.GetCellContents(currentCell.name).ToString();
            else
                currentCell.contents = _Spreadsheet.GetCellContents(currentCell.name).ToString();
            return true;
        }

        private void cellContentsTextbox_KeyUp(object sender, KeyEventArgs e)
        {
            int row, col;
            String value;
            spreadsheetPanel.GetSelection(out col, out row);
            spreadsheetPanel.GetValue(col, row, out value);
            spreadsheetPanel.SetValue(col, row, cellContentsTextbox.Text);
            currentCell.contents = cellContentsTextbox.Text;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Only looks for .sprd files by default
            saveFileDialog1.Filter = ".sprd Spreadsheet File| *.sprd";

            saveFileDialog1.Title = "Save a Spreadsheet";
            saveFileDialog1.ShowDialog();

            // Makes sure that a valid filename has been input, and then saves to that file.
            // Filename includes save path in it. Automatically saved to that path.
            if (saveFileDialog1.FileName != "")
            {
                _Spreadsheet.Save(saveFileDialog1.FileName);
                int LastIndex = saveFileDialog1.FileName.LastIndexOf((char)92) + 1;
                SpreadsheetName = saveFileDialog1.FileName;
                string tempstring = SpreadsheetName.Substring(LastIndex);
                this.Text = tempstring;
            }
        }

        /// <summary>
        /// This function is called on application exit
        /// If the user has unsaved work, it prompts them to save their work.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExit(object sender, CancelEventArgs e)
        {
            
            if (_Spreadsheet.Changed)
            {
                var option = MessageBox.Show("There are unsaved changes. \nWould you line to save your work?", "Unsaved Work.", MessageBoxButtons.YesNoCancel);
                if(option == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(sender, e);
                    return;
                }
                if(option == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            return;
        }
        /// <summary>
        /// Event handler that deals with all button presses. Has been overwritten to
        /// handle spreadsheet arrow key movements
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                    if (currentCell.col > 0)
                    {
                        currentCell.col--;
                        spreadsheetPanel.SetSelection(currentCell.col, currentCell.row);
                        updateCells(spreadsheetPanel);
                        return true;
                    }
                    break;
                case Keys.Right:
                    if (currentCell.col < 26)
                    {
                        currentCell.col++;
                        spreadsheetPanel.SetSelection(currentCell.col, currentCell.row);
                        updateCells(spreadsheetPanel);
                        return true;
                    }
                    break;
                case Keys.Up:
                    if (currentCell.row > 0)
                    {
                        currentCell.row--;
                        spreadsheetPanel.SetSelection(currentCell.col, currentCell.row);
                        updateCells(spreadsheetPanel);
                        return true;
                    }
                    break;
                case Keys.Down:
                    if (currentCell.row < 99)
                    {
                        currentCell.row++;
                        spreadsheetPanel.SetSelection(currentCell.col, currentCell.row);
                        updateCells(spreadsheetPanel);
                        return true;
                    }
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }


        
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Visible = !textBox1.Visible;
            if(textBox1.Visible == true)
            {
                helpToolStripMenuItem.BackColor = Color.LightGray;
                
            }
            else
            {
                helpToolStripMenuItem.BackColor = Color.White;
            }
        }
    }
}
