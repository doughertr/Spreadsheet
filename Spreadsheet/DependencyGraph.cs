using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * ================================================
 * Name: Ryan Dougherty
 * UID: u0534947
 * 
 * CS 3500
 * Assignment PS2
 * Time Spent: ~4 Hours
 * ================================================
 */

namespace SpreadsheetUtilities
{
    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    /// </summary>
    public class DependencyGraph
    {
        /// <summary>
        /// This private class stores a HashSet of a single variable's 
        /// dependents and dependees for a dependency graph object.
        /// </summary>
        private class Dependencies
        {
            public HashSet<string> Dependents;
            public HashSet<string> Dependees;
            /// <summary>
            /// create a new empty dependency object
            /// </summary>
            public Dependencies()
            {
                Dependents = new HashSet<string>();
                Dependees = new HashSet<string>();
            }

        }


        /// <summary>
        /// This Dictionary stores all the variables and their corresponding dependencies and dependees
        /// </summary>
        private Dictionary<string, Dependencies> variableDependencies;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            variableDependencies = new Dictionary<string, Dependencies>();
            Size = 0;
        }

        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size{ get; private set; }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string var]
        {
            get {
                if (variableDependencies.ContainsKey(var))
                    return variableDependencies[var].Dependees.Count;
                return 0;
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string var)
        {
            if (variableDependencies.ContainsKey(var))
                return (variableDependencies[var].Dependents.Count > 0);
            return false;
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string var)
        {
            if (variableDependencies.ContainsKey(var))
                return (variableDependencies[var].Dependees.Count > 0);
            return false;
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string var)
        {
            if (variableDependencies.ContainsKey(var))
                return variableDependencies[var].Dependents;
            return new string[0]; //return a blank/empty string if 'var' is not a recognized variable
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string var)
        {
            if (variableDependencies.ContainsKey(var))
                return variableDependencies[var].Dependees;
            return new string[0]; //return a blank/empty string if 'var' is not a recognized variable

        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>        /// 
        public void AddDependency(string s, string t)
        {
            //if s or t dont exist, create a space in the dictionary for them
            if (!variableDependencies.ContainsKey(s))
                variableDependencies.Add(s, new Dependencies());
            if (!variableDependencies.ContainsKey(t))
                variableDependencies.Add(t, new Dependencies());

            //only add if both additions to the respective hashtables return true
            if(variableDependencies[s].Dependents.Add(t) && variableDependencies[t].Dependees.Add(s))
                Size++;
        }

        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s">s must be evaluated first. T depends on S</param>
        /// <param name="t">t cannot be evaluated until s is</param>
        public bool RemoveDependency(string s, string t)
        {
            //only try to remove if s and t exist
            if (variableDependencies.ContainsKey(s) && variableDependencies.ContainsKey(t))
            {
                variableDependencies[s].Dependents.Remove(t);
                variableDependencies[t].Dependees.Remove(s);
                Size--;
                return true;
            }
            return false;
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string var, IEnumerable<string> newDependents)
        {
            if (variableDependencies.ContainsKey(var))
            {
                //remove each old dependency
                foreach (string dependent in variableDependencies[var].Dependents.ToArray())
                    RemoveDependency(var, dependent);
                //add each new dependant
                foreach (string dependent in newDependents)
                    AddDependency(var, dependent);
            }
                
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string var, IEnumerable<string> newDependees)
        {
            if (variableDependencies.ContainsKey(var))
            {
                //remove each old dependee
                foreach (string dependee in variableDependencies[var].Dependees.ToArray())
                    RemoveDependency(dependee, var);
                //add each new dependee
                foreach (string dependee in newDependees)
                    AddDependency(dependee, var);
            }

        }

    }

}
