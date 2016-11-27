using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyGraphTests
{
    [TestClass()]
    public class MyDependencyGraphTests
    {
        [TestMethod()]
        public void TestSizeAfterAddition()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            Assert.AreEqual(TestDpGraph.Size, 0);
            TestDpGraph.AddDependency("A2", "A3");
            Assert.AreEqual(TestDpGraph.Size, 1);
        }
        [TestMethod()]
        public void TestSucessfulAddition1()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            TestDpGraph.AddDependency("Z3", "B2");
            Assert.AreEqual(TestDpGraph.GetDependents("Z3").ToArray()[0], "B2");
            Assert.AreEqual(TestDpGraph.GetDependees("B2").ToArray()[0], "Z3");
        }
        [TestMethod()]
        public void TestSucessfulAddition2()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            TestDpGraph.AddDependency("K3", "K3");
            TestDpGraph.AddDependency("K3", "E4");
            TestDpGraph.AddDependency("K3", "I9");
            TestDpGraph.AddDependency("O1", "K3");
            TestDpGraph.AddDependency("L2", "K3");


            Assert.IsTrue(TestDpGraph.GetDependents("K3").ToArray().Contains("K3"));
            Assert.IsTrue(TestDpGraph.GetDependents("K3").ToArray().Contains("E4"));
            Assert.IsTrue(TestDpGraph.GetDependents("K3").ToArray().Contains("I9"));
            Assert.IsTrue(TestDpGraph.GetDependees("K3").ToArray().Contains("O1"));
            Assert.IsTrue(TestDpGraph.GetDependees("K3").ToArray().Contains("L2"));

        }
        [TestMethod()]
        public void TestSizeAfterRemoval()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            Assert.AreEqual(TestDpGraph.Size, 0);
            TestDpGraph.AddDependency("A2", "A3");
            Assert.AreEqual(TestDpGraph.Size, 1);
            TestDpGraph.RemoveDependency("A2", "A3");
            Assert.AreEqual(TestDpGraph.Size, 0);

        }
        [TestMethod()]
        public void TestSucessfulRemoval1()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            TestDpGraph.AddDependency("K8", "C2");
            TestDpGraph.AddDependency("C2", "P6");
            TestDpGraph.RemoveDependency("C2", "K8"); //nothing should happen
            Assert.IsTrue(TestDpGraph.GetDependents("K8").ToArray().Contains("C2"));
            TestDpGraph.RemoveDependency("K8", "C2"); //should sucessfully remove
            Assert.IsFalse(TestDpGraph.GetDependents("K8").ToArray().Contains("C2"));

        }
        [TestMethod()]
        public void TestSucessfulRemoval2()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            TestDpGraph.AddDependency("K8", "C2");
            TestDpGraph.AddDependency("C2", "P6");
            TestDpGraph.RemoveDependency("C2", "K8"); 
            Assert.IsTrue(TestDpGraph.GetDependents("K8").ToArray().Contains("C2"));

        }
        [TestMethod()]
        public void TestFalseRemoval()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            Assert.IsFalse(TestDpGraph.GetDependents("K8").ToArray().Contains("C2"));
            Assert.IsFalse(TestDpGraph.GetDependees("C2").ToArray().Contains("K8"));
            TestDpGraph.RemoveDependency("K8", "C2");
            Assert.IsFalse(TestDpGraph.GetDependents("K8").ToArray().Contains("C2"));
            Assert.IsFalse(TestDpGraph.GetDependees("C2").ToArray().Contains("K8"));

        }
        [TestMethod()]
        public void TestCorrectDependeeSize()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            TestDpGraph.AddDependency("K8", "C2");
            TestDpGraph.AddDependency("R5", "I9");
            TestDpGraph.AddDependency("K8", "P0");
            TestDpGraph.AddDependency("E4", "I9");
            TestDpGraph.AddDependency("Q2", "P7");
            Assert.IsTrue(TestDpGraph["C2"] == 1);
            Assert.IsTrue(TestDpGraph["I9"] == 2);
            Assert.IsTrue(TestDpGraph["P0"] == 1);
            Assert.IsTrue(TestDpGraph["P7"] == 1);
            Assert.IsTrue(TestDpGraph["T8"] == 0);
        }
        [TestMethod()]
        public void TestHasDependants1()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            TestDpGraph.AddDependency("K8", "C2");
            Assert.IsTrue(TestDpGraph.HasDependents("K8"));
            Assert.IsFalse(TestDpGraph.HasDependents("C2"));
            Assert.IsFalse(TestDpGraph.HasDependents("I8"));
        }
        [TestMethod()]
        public void TestHasDependants2()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            TestDpGraph.AddDependency("P9", "F5");
            Assert.IsTrue(TestDpGraph.HasDependents("P9"));
            TestDpGraph.RemoveDependency("P9", "F5");
            Assert.IsFalse(TestDpGraph.HasDependents("P9"));
        }

        [TestMethod()]
        public void TestHasDependees1()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            TestDpGraph.AddDependency("C2", "K8");
            Assert.IsTrue(TestDpGraph.HasDependees("K8"));
            Assert.IsFalse(TestDpGraph.HasDependees("C2"));
            Assert.IsFalse(TestDpGraph.HasDependees("I8"));
        }
        [TestMethod()]
        public void TestHasDependees2()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            TestDpGraph.AddDependency("P9", "F5");
            Assert.IsTrue(TestDpGraph.HasDependees("F5"));
            TestDpGraph.RemoveDependency("P9", "F5");
            Assert.IsFalse(TestDpGraph.HasDependees("F5"));
        }
        [TestMethod()]
        public void TestReplaceDependents()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            TestDpGraph.AddDependency("K8", "C2");
            TestDpGraph.AddDependency("K8", "I9");
            TestDpGraph.AddDependency("K8", "P0");
            TestDpGraph.AddDependency("E4", "C2");
            TestDpGraph.AddDependency("Q2", "C2");
            TestDpGraph.AddDependency("L7", "C2");
            string[] newDependents = {"W3", "M9", "B0", "X2", "I9" };
            TestDpGraph.ReplaceDependents("K8", newDependents);
            CollectionAssert.AreEqual(TestDpGraph.GetDependents("K8").ToArray(), newDependents);
            Assert.IsTrue(TestDpGraph.GetDependents("P0").ToArray().Length == 0);
            Assert.IsTrue(TestDpGraph.GetDependees("P0").ToArray().Length == 0);

        }
        [TestMethod()]
        public void TestReplaceDependendees()
        {
            DependencyGraph TestDpGraph = new DependencyGraph();
            TestDpGraph.AddDependency("K8", "C2");
            TestDpGraph.AddDependency("K8", "I9");
            TestDpGraph.AddDependency("K8", "P0");
            TestDpGraph.AddDependency("E4", "C2");
            TestDpGraph.AddDependency("Q2", "C2");
            TestDpGraph.AddDependency("L7", "C2");
            string[] newDependendees = { "L3", "Q2", "U6", "Z3", "A7" };
            TestDpGraph.ReplaceDependees("C2", newDependendees);
            CollectionAssert.AreEqual(TestDpGraph.GetDependees("C2").ToArray(), newDependendees);
            Assert.IsTrue(TestDpGraph.GetDependents("E4").ToArray().Length == 0);
            Assert.IsTrue(TestDpGraph.GetDependees("E4").ToArray().Length == 0);

        }
    }
    [TestClass()]
    public class DepenencyGraphGradingTests
    {
        // ************************** TESTS ON EMPTY DGs ************************* //

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyTest1()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.AreEqual(0, t.Size);
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyTest2()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.IsFalse(t.HasDependees("a"));
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyTest3()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.IsFalse(t.HasDependents("a"));
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyTest4()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.IsFalse(t.GetDependees("a").GetEnumerator().MoveNext());
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyTest5()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.IsFalse(t.GetDependents("a").GetEnumerator().MoveNext());
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyTest6()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.AreEqual(0, t["a"]);
        }

        /// <summary>
        ///Removing from an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void EmptyTest7()
        {
            DependencyGraph t = new DependencyGraph();
            t.RemoveDependency("a", "b");
            Assert.AreEqual(0, t.Size);
        }

        /// <summary>
        ///Adding an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void EmptyTest8()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
        }

        /// <summary>
        ///Replace on an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void EmptyTest9()
        {
            DependencyGraph t = new DependencyGraph();
            t.ReplaceDependents("a", new HashSet<string>());
            Assert.AreEqual(0, t.Size);
        }

        /// <summary>
        ///Replace on an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void EmptyTest10()
        {
            DependencyGraph t = new DependencyGraph();
            t.ReplaceDependees("a", new HashSet<string>());
            Assert.AreEqual(0, t.Size);
        }


        /**************************** SIMPLE NON-EMPTY TESTS ****************************/

        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest1()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            Assert.AreEqual(2, t.Size);
        }

        /// <summary>
        ///Slight variant
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest2()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "b");
            Assert.AreEqual(1, t.Size);
        }

        /// <summary>
        ///Nonempty graph should contain something
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest3()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            Assert.IsFalse(t.HasDependees("a"));
            Assert.IsTrue(t.HasDependees("b"));
            Assert.IsTrue(t.HasDependents("a"));
            Assert.IsTrue(t.HasDependees("c"));
        }

        /// <summary>
        ///Nonempty graph should contain something
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest4()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            HashSet<String> aDents = new HashSet<String>(t.GetDependents("a"));
            HashSet<String> bDents = new HashSet<String>(t.GetDependents("b"));
            HashSet<String> cDents = new HashSet<String>(t.GetDependents("c"));
            HashSet<String> dDents = new HashSet<String>(t.GetDependents("d"));
            HashSet<String> eDents = new HashSet<String>(t.GetDependents("e"));
            HashSet<String> aDees = new HashSet<String>(t.GetDependees("a"));
            HashSet<String> bDees = new HashSet<String>(t.GetDependees("b"));
            HashSet<String> cDees = new HashSet<String>(t.GetDependees("c"));
            HashSet<String> dDees = new HashSet<String>(t.GetDependees("d"));
            HashSet<String> eDees = new HashSet<String>(t.GetDependees("e"));
            Assert.IsTrue(aDents.Count == 2 && aDents.Contains("b") && aDents.Contains("c"));
            Assert.IsTrue(bDents.Count == 0);
            Assert.IsTrue(cDents.Count == 0);
            Assert.IsTrue(dDents.Count == 1 && dDents.Contains("c"));
            Assert.IsTrue(eDents.Count == 0);
            Assert.IsTrue(aDees.Count == 0);
            Assert.IsTrue(bDees.Count == 1 && bDees.Contains("a"));
            Assert.IsTrue(cDees.Count == 2 && cDees.Contains("a") && cDees.Contains("d"));
            Assert.IsTrue(dDees.Count == 0);
            Assert.IsTrue(dDees.Count == 0);
        }

        /// <summary>
        ///Nonempty graph should contain something
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest5()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            Assert.AreEqual(0, t["a"]);
            Assert.AreEqual(1, t["b"]);
            Assert.AreEqual(2, t["c"]);
            Assert.AreEqual(0, t["d"]);
            Assert.AreEqual(0, t["e"]);
        }

        /// <summary>
        ///Removing from a DG 
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest6()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            t.RemoveDependency("a", "b");
            Assert.AreEqual(2, t.Size);
        }

        /// <summary>
        ///Replace on a DG
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest7()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            t.ReplaceDependents("a", new HashSet<string>() { "x", "y", "z" });
            HashSet<String> aPends = new HashSet<string>(t.GetDependents("a"));
            Assert.IsTrue(aPends.SetEquals(new HashSet<string>() { "x", "y", "z" }));
        }

        /// <summary>
        ///Replace on a DG
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest8()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            t.ReplaceDependees("c", new HashSet<string>() { "x", "y", "z" });
            HashSet<String> cDees = new HashSet<string>(t.GetDependees("c"));
            Assert.IsTrue(cDees.SetEquals(new HashSet<string>() { "x", "y", "z" }));
        }

        // ************************** STRESS TESTS ******************************** //
        /// <summary>
        ///Using lots of data
        ///</summary>
        [TestMethod()]
        public void StressTest1()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 100;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 2; j < SIZE; j += 2)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }



        // ********************************** ANOTHER STESS TEST ******************** //
        /// <summary>
        ///Using lots of data with replacement
        ///</summary>
        [TestMethod()]
        public void StressTest8()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 100;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 2; j < SIZE; j += 2)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Replace a bunch of dependents
            for (int i = 0; i < SIZE; i += 4)
            {
                HashSet<string> newDents = new HashSet<String>();
                for (int j = 0; j < SIZE; j += 7)
                {
                    newDents.Add(letters[j]);
                }
                t.ReplaceDependents(letters[i], newDents);

                foreach (string s in dents[i])
                {
                    dees[s[0] - 'a'].Remove(letters[i]);
                }

                foreach (string s in newDents)
                {
                    dees[s[0] - 'a'].Add(letters[i]);
                }

                dents[i] = newDents;
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }

        // ********************************** A THIRD STESS TEST ******************** //
        /// <summary>
        ///Using lots of data with replacement
        ///</summary>
        [TestMethod()]
        public void StressTest15()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 100;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 2; j < SIZE; j += 2)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Replace a bunch of dependees
            for (int i = 0; i < SIZE; i += 4)
            {
                HashSet<string> newDees = new HashSet<String>();
                for (int j = 0; j < SIZE; j += 7)
                {
                    newDees.Add(letters[j]);
                }
                t.ReplaceDependees(letters[i], newDees);

                foreach (string s in dees[i])
                {
                    dents[s[0] - 'a'].Remove(letters[i]);
                }

                foreach (string s in newDees)
                {
                    dents[s[0] - 'a'].Add(letters[i]);
                }

                dees[i] = newDees;
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }
    }

}