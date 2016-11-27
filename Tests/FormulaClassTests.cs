using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpreadsheetUtilities.Tests
{
    // These test cases are the property of the University of Utah School of Computing.
    // Redistributing this file is strictly against SoC policy.

    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using SpreadsheetUtilities;

    namespace GradingTests
    {
        struct LookupFunctions
        {
            public static double NullLookup(string var)
            {
                throw new ArgumentException("Null Lookup was called");
            }
            public static double ZeroLookup(string var)
            {
                switch (var)
                {
                    case "A0":
                        return 0;
                    default:
                        throw new ArgumentException("Unknown variable \'" + var + "\'");
                }
            }
            public static double SimpleLookup1(string var)
            {
                switch (var)
                {
                    case "A6":
                        return 7;
                    case "B3":
                        return 3.0;
                    default:
                        throw new ArgumentException("Unknown variable \'" + var + "\'");
                }
            }
        }
        [TestClass]
        public class PS3GradingTests
        {
            [TestMethod()]
            public void Test16()
            {
                Formula f = new Formula("2+X1");
                Assert.IsInstanceOfType(f.Evaluate(s => { throw new ArgumentException("Unknown variable"); }), typeof(FormulaError));
            }

            [TestMethod()]
            public void Test17()
            {
                Formula f = new Formula("5/0");
                Assert.IsInstanceOfType(f.Evaluate(s => 0), typeof(FormulaError));
            }

            [TestMethod()]
            public void Test18()
            {
                Formula f = new Formula("(5 + X1) / (X1 - 3)");
                Assert.IsInstanceOfType(f.Evaluate(s => 3), typeof(FormulaError));
            }


            // Tests of syntax errors detected by the constructor
            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void Test19()
            {
                Formula f = new Formula("+");
            }

            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void Test20()
            {
                Formula f = new Formula("2+5+");
            }

            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void Test21()
            {
                Formula f = new Formula("2+5*7)");
            }

            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void Test22()
            {
                Formula f = new Formula("((3+5*7)");
            }

            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void Test23()
            {
                Formula f = new Formula("5x");
            }

            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void Test24()
            {
                Formula f = new Formula("5+5x");
            }

            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void Test25()
            {
                Formula f = new Formula("5+7+(5)8");
            }

            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void Test26()
            {
                Formula f = new Formula("5 5");
            }

            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void Test27()
            {
                Formula f = new Formula("5 + + 3");
            }

            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void Test28()
            {
                Formula f = new Formula("");
            }

            // Some more complicated formula evaluations
            [TestMethod()]
            public void Test29()
            {
                //continue to pop and solve while previous operators precede current operator in ranking
                Formula f = new Formula("y1*3-8/2+4*(8-9*2)/14*x7");
                Assert.AreEqual(5.14285714285714, (double)f.Evaluate(s => (s == "x7") ? 1 : 4), 1e-9);
            }

            [TestMethod()]
            public void Test30()
            {
                Formula f = new Formula("x1+(x2+(x3+(x4+(x5+x6))))");
                Assert.AreEqual(6, (double)f.Evaluate(s => 1), 1e-9);
            }

            [TestMethod()]
            public void Test31()
            {
                Formula f = new Formula("((((x1+x2)+x3)+x4)+x5)+x6");
                Assert.AreEqual(12, (double)f.Evaluate(s => 2), 1e-9);
            }

            [TestMethod()]
            public void Test32()
            {
                Formula f = new Formula("a4-a4*a4/a4");
                Assert.AreEqual(0, (double)f.Evaluate(s => 3), 1e-9);
            }

            // Test of the Equals method
            [TestMethod()]
            public void Test33()
            {
                Formula f1 = new Formula("X1+X2");
                Formula f2 = new Formula("X1+X2");
                Assert.IsTrue(f1.Equals(f2));
            }

            [TestMethod()]
            public void Test34()
            {
                Formula f1 = new Formula("X1+X2");
                Formula f2 = new Formula(" X1  +  X2   ");
                Assert.IsTrue(f1.Equals(f2));
            }

            [TestMethod()]
            public void Test35()
            {
                Formula f1 = new Formula("2+X1*3.00");
                Formula f2 = new Formula("2.00+X1*3.0");
                Assert.IsTrue(f1.Equals(f2));
            }

            [TestMethod()]
            public void Test36()
            {
                Formula f1 = new Formula("1e-2 + X5 + 17.00 * 19 ");
                Formula f2 = new Formula("   0.0100  +     X5+ 17 * 19.00000 ");
                Assert.IsTrue(f1.Equals(f2));
            }

            [TestMethod()]
            public void Test37()
            {
                Formula f = new Formula("2");
                Assert.IsFalse(f.Equals(null));
                Assert.IsFalse(f.Equals(""));
            }


            // Tests of == operator
            [TestMethod()]
            public void Test38()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("2");
                Assert.IsTrue(f1 == f2);
            }

            [TestMethod()]
            public void Test39()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("5");
                Assert.IsFalse(f1 == f2);
            }

            [TestMethod()]
            public void Test40()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("2");
                Assert.IsFalse(null == f1);
                Assert.IsFalse(f1 == null);
                Assert.IsTrue(f1 == f2);
            }


            // Tests of != operator
            [TestMethod()]
            public void Test41()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("2");
                Assert.IsFalse(f1 != f2);
            }

            [TestMethod()]
            public void Test42()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("5");
                Assert.IsTrue(f1 != f2);
            }


            // Test of ToString method
            [TestMethod()]
            public void Test43()
            {
                Formula f = new Formula("2*5");
                Assert.IsTrue(f.Equals(new Formula(f.ToString())));
            }


            // Tests of GetHashCode method
            [TestMethod()]
            public void Test44()
            {
                Formula f1 = new Formula("2*5");
                Formula f2 = new Formula("2*5");
                Assert.IsTrue(f1.GetHashCode() == f2.GetHashCode());
            }

            [TestMethod()]
            public void Test45()
            {
                Formula f1 = new Formula("2*5");
                Formula f2 = new Formula("3/8*2+(7)");
                Assert.IsTrue(f1.GetHashCode() != f2.GetHashCode());
            }


            // Tests of GetVariables method
            [TestMethod()]
            public void Test46()
            {
                Formula f = new Formula("2*5");
                Assert.IsFalse(f.GetVariables().GetEnumerator().MoveNext());
            }

            [TestMethod()]
            public void Test47()
            {
                Formula f = new Formula("2*X2");
                List<string> actual = new List<string>(f.GetVariables());
                HashSet<string> expected = new HashSet<string>() { "X2" };
                Assert.AreEqual(actual.Count, 1);
                Assert.IsTrue(expected.SetEquals(actual));
            }

            [TestMethod()]
            public void Test48()
            {
                Formula f = new Formula("2*X2+Y3");
                List<string> actual = new List<string>(f.GetVariables());
                HashSet<string> expected = new HashSet<string>() { "Y3", "X2" };
                Assert.AreEqual(actual.Count, 2);
                Assert.IsTrue(expected.SetEquals(actual));
            }

            [TestMethod()]
            public void Test49()
            {
                Formula f = new Formula("2*X2+X2");
                List<string> actual = new List<string>(f.GetVariables());
                HashSet<string> expected = new HashSet<string>() { "X2" };
                Assert.AreEqual(actual.Count, 1);
                Assert.IsTrue(expected.SetEquals(actual));
            }

            [TestMethod()]
            public void Test50()
            {
                Formula f = new Formula("X1+Y2*X3*Y2+Z7+X1/Z8");
                List<string> actual = new List<string>(f.GetVariables());
                HashSet<string> expected = new HashSet<string>() { "X1", "Y2", "X3", "Z7", "Z8" };
                Assert.AreEqual(actual.Count, 5);
                Assert.IsTrue(expected.SetEquals(actual));
            }

            // Tests to make sure there can be more than one formula at a time
            [TestMethod()]
            public void Test51a()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("3");
                Assert.IsTrue(f1.ToString().IndexOf("2") >= 0);
                Assert.IsTrue(f2.ToString().IndexOf("3") >= 0);
            }

            [TestMethod()]
            public void Test51b()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("3");
                Assert.IsTrue(f1.ToString().IndexOf("2") >= 0);
                Assert.IsTrue(f2.ToString().IndexOf("3") >= 0);
            }

            [TestMethod()]
            public void Test51c()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("3");
                Assert.IsTrue(f1.ToString().IndexOf("2") >= 0);
                Assert.IsTrue(f2.ToString().IndexOf("3") >= 0);
            }

            [TestMethod()]
            public void Test51d()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("3");
                Assert.IsTrue(f1.ToString().IndexOf("2") >= 0);
                Assert.IsTrue(f2.ToString().IndexOf("3") >= 0);
            }

            [TestMethod()]
            public void Test51e()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("3");
                Assert.IsTrue(f1.ToString().IndexOf("2") >= 0);
                Assert.IsTrue(f2.ToString().IndexOf("3") >= 0);
            }

            // Stress test for constructor
            [TestMethod()]
            public void Test52a()
            {
                Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
            }

            // Stress test for constructor
            [TestMethod()]
            public void Test52b()
            {
                Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
            }

            // Stress test for constructor
            [TestMethod()]
            public void Test52c()
            {
                Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
            }

            // Stress test for constructor
            [TestMethod()]
            public void Test52d()
            {
                Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
            }

            // Stress test for constructor
            [TestMethod()]
            public void Test52e()
            {
                Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
            }
        }






        [TestClass()]
        public class FormulaClassConstructorTests
        {
            [TestMethod]
            [ExpectedException(typeof(FormulaFormatException))]
            public void BlankConstructorTest1()
            {
                new Formula("").Evaluate(LookupFunctions.NullLookup);
            }

            [TestMethod]
            [ExpectedException(typeof(FormulaFormatException))]
            public void BlankConstructorTest2()
            {
                new Formula("", n => n.ToString(), s => true);
            }
            [TestMethod()]
            public void TestSucessfulBasicConstruction1()
            {
                try
                {
                    Formula TestFormula = new Formula("9 + 8");
                }
                catch (FormulaFormatException ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }

            }
            [TestMethod()]
            public void TestSucessfulBasicConstruction2()
            {
                try
                {
                    Formula TestFormula = new Formula("7 * 8 + 3 - 2 / 7 + 9.0002 * 5.999");
                }
                catch (FormulaFormatException ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }

            }
            [TestMethod()]
            public void TestSucessfulBasicConstruction3()
            {
                try
                {
                    Formula TestFormula = new Formula("4 + 4 - a5 * 6.01 - (t33 / bb1) + _r");
                }
                catch (FormulaFormatException ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }

            }
            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestSucessfulBasicConstruction4()
            {
                Formula TestFormula = new Formula("3 + 9.9.5", s => s, s => Regex.IsMatch(s, "^[a-zA-Z]+[0-9]+$"));
            }
            [TestMethod()]
            public void TestFailureBasicConstruction1()
            {
                try
                {
                    Formula TestFormula = new Formula("10 * i5rt510oo + 3.0");
                }
                catch (FormulaFormatException ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }
            }
            [TestMethod()]
            public void TestFailureBasicConstruction2()
            {
                try
                {
                    Formula TestFormula = new Formula("2.0 + (7 - y5yyt) * 5 - 10.9");
                }
                catch (FormulaFormatException ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }
            }
            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestFailureConstruction1()
            {
                Formula TestFormula = new Formula("4 * 2.5 + i3r", s => s, s => Regex.IsMatch(s, "^[a-zA-Z]+[0-9]+$"));
            }

            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestFailureConstruction2()
            {
                Formula TestFormula = new Formula("9.0 + 15 - 17.4 * q4.0qq + 4", s => s, s => Regex.IsMatch(s, "^[a-zA-Z]+[0-9]+$"));
            }
            [TestMethod()]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestFailureConstruction3()
            {
                Formula TestFormula = new Formula("9.0 + 4 * (i - 57) / (a.) + 5", s => s, s => Regex.IsMatch(s, "^[a-zA-Z]+[0-9]+$"));
            }
        }
        [TestClass()]
        public class FormulaClassGetVariableTests
        {
            [TestMethod()]
            public void TestEmptyGetVariables()
            {
                Formula TestFormula = new Formula("9 + 8.2 / 3 (5 + 9.003) * 5 * 1.67");
                foreach (string var in TestFormula.GetVariables())
                {
                    Assert.AreEqual(var, "");
                }
            }
            [TestMethod()]
            public void TestBasicGetVariables1()
            {
                Formula TestFormula = new Formula("9 + a3 - 9 * Y4");
                IEnumerator<string> testVarEnum = TestFormula.GetVariables().GetEnumerator();
                testVarEnum.MoveNext();
                Assert.AreEqual(testVarEnum.Current, "a3");
                testVarEnum.MoveNext();
                Assert.AreEqual(testVarEnum.Current, "Y4");
            }
            [TestMethod()]
            public void TestBasicGetVariables2()
            {
                Formula TestFormula = new Formula("9 + rr2 - 8 * (4.8 + i9kddddq) / yu1 + 4");
                IEnumerator<string> testVarEnum = TestFormula.GetVariables().GetEnumerator();
                testVarEnum.MoveNext();
                Assert.AreEqual(testVarEnum.Current, "rr2");
                testVarEnum.MoveNext();
                Assert.AreEqual(testVarEnum.Current, "i9kddddq");
                testVarEnum.MoveNext();
                Assert.AreEqual(testVarEnum.Current, "yu1");
            }
            [TestMethod()]
            public void TestBasicGetVariables3()
            {
                Formula TestFormula = new Formula("a1 + a2 + A1 + A2");
                IEnumerator<string> testVarEnum = TestFormula.GetVariables().GetEnumerator();
                testVarEnum.MoveNext();
                Assert.AreEqual(testVarEnum.Current, "a1");
                testVarEnum.MoveNext();
                Assert.AreEqual(testVarEnum.Current, "a2");
                testVarEnum.MoveNext();
                Assert.AreEqual(testVarEnum.Current, "A1");
                testVarEnum.MoveNext();
                Assert.AreEqual(testVarEnum.Current, "A2");
            }
            [TestMethod]
            public void TestGetVariablesWithMultipleOfSameVariable()
            {
                Formula TestFormula = new Formula("a1 + b5 + a1 + c6 + b5 + d10");
                IEnumerator<string> testVarEnum = TestFormula.GetVariables().GetEnumerator();
                testVarEnum.MoveNext();
                Assert.AreEqual(testVarEnum.Current, "a1");
                testVarEnum.MoveNext();
                Assert.AreEqual(testVarEnum.Current, "b5");
                testVarEnum.MoveNext();
                Assert.AreEqual(testVarEnum.Current, "c6");
                testVarEnum.MoveNext();
                Assert.AreEqual(testVarEnum.Current, "d10");
            }

            [TestMethod()]
            public void TestNormalizedGetVariables()
            {
                try
                {
                    Formula TestFormula = new Formula("9 + A1 - (R8 / eEeE5) * 9.3", s => s.ToLower(), s => Regex.IsMatch(s, "^[a-zA-Z]+[0-9]+$"));
                    IEnumerator<string> testVarEnum = TestFormula.GetVariables().GetEnumerator();
                    testVarEnum.MoveNext();
                    Assert.AreEqual(testVarEnum.Current, "a1");
                    testVarEnum.MoveNext();
                    Assert.AreEqual(testVarEnum.Current, "r8");
                    testVarEnum.MoveNext();
                    Assert.AreEqual(testVarEnum.Current, "eeee5");

                }
                catch (FormulaFormatException ex)
                {
                    Assert.Fail("Expected no exception, but got: " + ex.Message);
                }

            }
        }
        [TestClass()]
        public class FormulaClassEqualsTests
        {
            [TestMethod()]
            public void TestNullEqualsOperator1()
            {
                Formula TestFormula1 = null;
                Formula TestFormula2 = null;
                Assert.IsTrue(TestFormula1 == TestFormula2);
            }
            [TestMethod()]
            public void TestNullEqualsOperator2()
            {
                Formula TestFormula1 = new Formula("8 + 6/3 - (10 + A5)");
                Formula TestFormula2 = null;
                Assert.IsFalse(TestFormula1 == TestFormula2);
                Assert.IsFalse(TestFormula2 == TestFormula1);
            }

            [TestMethod()]
            public void TestNullNotEqualsOperator1()
            {
                Formula TestFormula1 = null;
                Formula TestFormula2 = null;
                Assert.IsFalse(TestFormula1 != TestFormula2);
                Assert.IsFalse(TestFormula2 != TestFormula1);
            }
            [TestMethod()]
            public void TestNullNotEqualsOperator2()
            {
                Formula TestFormula1 = new Formula("8 + 6/3 - (10 + A5)");
                Formula TestFormula2 = null;
                Assert.IsTrue(TestFormula1 != TestFormula2);
                Assert.IsTrue(TestFormula2 != TestFormula1);
            }


            [TestMethod]
            public void TestFunctionEquals1()
            {
                Assert.IsTrue(new Formula("x1+y2", n => n.ToUpper(), s => true).Equals(new Formula("X1  +  Y2")));
            }
            [TestMethod]
            public void TestFunctionEquals2()
            {
                Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("X1+Y2")));
            }
            [TestMethod]
            public void TestFunctionEquals3()
            {
                Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("y2+x1")));
            }
            [TestMethod]
            public void TestFunctionEquals4()
            {
                Assert.IsTrue(new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")));
            }
            [TestMethod]
            public void TestFunctionEquals5()
            {
                Assert.IsFalse(new Formula("2.0").Equals(new FormulaError()));
            }
            [TestMethod]
            public void TestFunctionEquals6()
            {
                Assert.IsFalse(new Formula("2.0 + x7 * 9 - 3").Equals(new Formula("2.000 + x7")));
            }
            [TestMethod]
            public void TestEqualsDoublePrecision()
            {
                Assert.IsTrue(new Formula("2.0 + x7").Equals(new Formula("2.0000000000000000000001 + x7")));
            }


            [TestMethod]
            public void TestOperatorEquals1()
            {
                Assert.IsTrue(new Formula("x1+y2", n => n.ToUpper(), s => true) == (new Formula("X1  +  Y2")));
            }
            [TestMethod]
            public void TestOperatorEquals2()
            {
                Assert.IsFalse(new Formula("x1+y2") == (new Formula("X1+Y2")));
            }
            [TestMethod]
            public void TestOperatorEquals3()
            {
                Assert.IsFalse(new Formula("x1+y2") == (new Formula("y2+x1")));
            }
            [TestMethod]
            public void TestOperatorEquals4()
            {
                Assert.IsTrue(new Formula("2.0 + x7") == (new Formula("2.000 + x7")));
            }
            [TestMethod]
            public void TestOperatorEqualsDoublePrecision()
            {
                Assert.IsTrue(new Formula("2.0 + x7") == (new Formula("2.0000000000000000000001 + x7")));
            }

            [TestMethod]
            public void TestOperatorNotEquals1()
            {
                Assert.IsFalse(new Formula("x1+y2", n => n.ToUpper(), s => true) != (new Formula("X1  +  Y2")));
            }
            [TestMethod]
            public void TestOperatorNotEquals2()
            {
                Assert.IsTrue(new Formula("x1+y2") != (new Formula("X1+Y2")));
            }
            [TestMethod]
            public void TestOperatorNotEquals3()
            {
                Assert.IsTrue(new Formula("x1+y2") != (new Formula("y2+x1")));
            }
            [TestMethod]
            public void TestOperatorNotEquals4()
            {
                Assert.IsFalse(new Formula("2.0 + x7") != (new Formula("2.000 + x7")));
            }
            [TestMethod]
            public void TestOperatorNotEqualsDoublePrecision()
            {
                Assert.IsFalse(new Formula("2.0 + x7") != (new Formula("2.0000000000000000000001 + x7")));
            }
        }
        [TestClass]
        public class FormulaClassToStringTests
        {
            [TestMethod]
            public void BasicToStringTest1()
            {
                Assert.AreEqual(new Formula("x + y", n => n.ToUpper(), s => true).ToString(), "X+Y");
            }
            [TestMethod]
            public void BasicToStringTest2()
            {
                Assert.AreEqual(new Formula("x + Y").ToString(), "x+Y");
            }
            [TestMethod]
            public void BasicToStringTest3()
            {
                Assert.AreEqual(new Formula("9.000 + a3 + (t5 - aAaa445 + 9.9991) * uo1", n => n.ToUpper(), s => true).ToString(), "9+A3+(T5-AAAA445+9.9991)*UO1");
            }
            [TestMethod]
            public void ToStringEqualsTest1()
            {
                Assert.IsTrue(new Formula("x+y").ToString().Equals(new Formula("x + y").ToString()));
            }
            [TestMethod]
            public void ToStringEqualsTest3()
            {
                Assert.IsTrue(new Formula("2.0e3").ToString().Equals(new Formula("2000").ToString()));
            }
        }
        [TestClass]
        public class FormulaClassGetHashCodeTests
        {
            [TestMethod]
            public void TestGetHashCodeEquivalence1()
            {
                Formula TestFormula1 = new Formula("x + Y");
                Formula TestFormula2 = TestFormula1;
                Assert.AreEqual(TestFormula1.GetHashCode(), TestFormula2.GetHashCode());
            }

            [TestMethod]
            public void TestGetHashCodeEquivalence2()
            {
                Assert.AreEqual(new Formula("x + Y").GetHashCode(), new Formula("x+Y").GetHashCode());
            }
            [TestMethod]
            public void TestGetHashCodeEquivalence3()
            {
                Assert.AreEqual(new Formula("x + y", n => n.ToUpper(), s => true).GetHashCode(), new Formula("X+Y").GetHashCode());
            }
            [TestMethod]
            public void TestGetHashCodeEquivalence4()
            {
                Assert.AreEqual(new Formula("3", n => n.ToUpper(), s => true).GetHashCode(), new Formula("3.000000").GetHashCode());
            }
            [TestMethod]
            public void TestGetHashCodeEquivalence5()
            {
                Assert.AreEqual(new Formula("2e2 + a4", n => n.ToUpper(), s => true).GetHashCode(), new Formula("200+A4").GetHashCode());
            }
            [TestMethod]
            public void TestGetHashCodeNonEquivalence1()
            {
                Assert.AreNotEqual(new Formula("x + y").GetHashCode(), new Formula("X + Y").GetHashCode());
            }
            [TestMethod]
            public void TestGetHashCodeNonEquivalence2()
            {
                Assert.AreNotEqual(new Formula("x+y").GetHashCode(), "x+y".GetHashCode());
            }
        }
        [TestClass]
        public class FormulaClassEvaluateTests
        {

            [TestMethod]
            public void BasicEvaluateTest1()
            {
                Assert.AreEqual(new Formula("10").Evaluate(LookupFunctions.NullLookup), 10.0);
            }
            [TestMethod]
            public void BasicEvaluateTest2()
            {
                Assert.AreEqual(new Formula("5+5").Evaluate(LookupFunctions.NullLookup), 10.0);
            }
            [TestMethod]
            public void BasicEvaluateTest3()
            {
                Assert.AreEqual(new Formula("(5+5)/2").Evaluate(LookupFunctions.NullLookup), 5.0);
            }
            [TestMethod]
            public void BasicEvaluateTest4()
            {
                Assert.AreEqual(new Formula("A6 - 90 / B3 + 7 ").Evaluate(LookupFunctions.SimpleLookup1), -16.0);
            }
            [TestMethod]
            public void BasicEvaluateTest5()
            {
                Assert.IsInstanceOfType(new Formula("7 * 4 -  (4 / 0) ").Evaluate(LookupFunctions.NullLookup), typeof(FormulaError));
            }
            [TestMethod]
            public void BasicEvaluateTest6()
            {
                Assert.IsInstanceOfType(new Formula("(6 + 9) - Z5 + 8 / 3").Evaluate(LookupFunctions.SimpleLookup1), typeof(FormulaError));
            }

            [TestMethod()]
            public void AdvancedEvaluateTest2()
            {
                Assert.AreEqual(12.0, new Formula("((((x1+x2)+x3)+x4)+x5)+x6").Evaluate(s => 2));
            }
            //[TestMethod()]
            //public void AdvancedEvaluateTest3()
            //{
            //    Assert.AreEqual(5.14, new Formula("4*3-8/2+4*(8-9*2)/14*1").Evaluate(s => (s == "x7") ? 1 : 4));
            //}


            [TestMethod]
            public void DivideByZeroTest1()
            {
                Assert.IsInstanceOfType(new Formula("10 / 0").Evaluate(LookupFunctions.NullLookup), typeof(FormulaError));
            }

            [TestMethod]
            public void DivideByZeroTest2()
            {
                Assert.IsInstanceOfType(new Formula("10 / (9-9)").Evaluate(LookupFunctions.NullLookup), typeof(FormulaError));
            }
            [TestMethod]
            public void DivideByZeroTest3()
            {
                Assert.IsInstanceOfType(new Formula("10 / A0").Evaluate(LookupFunctions.ZeroLookup), typeof(FormulaError));
            }
            [TestMethod]
            public void DivideByZeroTest4()
            {
                Assert.IsInstanceOfType(new Formula("10 + 8 / 0 * 5 / 3").Evaluate(LookupFunctions.NullLookup), typeof(FormulaError));
            }
        }
    }
}