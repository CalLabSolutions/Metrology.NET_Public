using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SOA_DataAccessLibrary;
using System.IO;

namespace SOA_DataAccessLibrary_UnitTest1
{
    [TestClass]
    public class SOA_DataAccessLibrary_UnitTest1
    {
        [TestMethod]
        public void TestLoad()
        {
            SOA_DataAccess dao = new SOA_DataAccess();
            OpResult op;

            // An SOA_DataAccess object can be loaded from a string, stream, local file, or remote file

            // uncomment next 5 lines if using local files 
            //string binDebugPath = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath;      
            //string DataFilesPath = binDebugPath.Replace(@"\Source\SOA_DataAccessLib\UnitTestLoad\bin\Debug", @"\Data Files\");
            //UomDataSource.DatabasePath = DataFilesPath + "UOM_Database.xml";
            //op = dao.load( DataFilesPath + "SOASample_TwoParameter_SixCases_TwoAssertions_ComplexFormula.xml"); 
            //Assert.IsTrue(op.Success, "local files test " + op.Error);

            //uncomment next 2 lines if using remote files
            op = dao.load("http://schema.metrology.net/SOASample_TwoParameter_SixCases_TwoAssertions_ComplexFormula.xml");
            Assert.IsTrue(op.Success, "remote file test " + op.Error);

            // Once the SOA_DataAccess object is loaded, the Object Model is accessed via the SOA_DataAccess object's SOADataMaster property
            Soa SampleSOA = dao.SOADataMaster;
             
            // This will often be the first query performed on an SOA (Does it deal at all with a measurement quantity that is of interest?)
            bool supportsResultType = SampleSOA.ResultTypes.Contains("voltage");

            // Looping through a SOA's ProcessTypes looking for known ProcessType Names will also be very common search refinement
            // A ProcessType specifies the measurement quantity sourced or measured as a result of performing the ProcessType, 
            //   as well as a minimal list of input parameters required. 
            var process_name1 = SampleSOA.CapabilityScope.Activities[0].ProcessTypes[0].ProcessType.Name;
            Assert.AreEqual<string>("Measure.Voltage.AC", process_name1, "Failed MtcTechnique MtcProcessType");

            // Looping through a SOA's Technique Names will also be very common search refinement
            // A Technique is a specific implementation of a ProcessType.
            var technique_name1 = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.Name;
            Assert.AreEqual<string>("Measure.Voltage.AC.LowVoltage", technique_name1, "Failed TemplateTechnique Name");

            // Once a Technique is found, the numeric bounds of its results can be found
            // In this case, the Technique is verified to have one result, the result is voltage, 
            //    and it is within a range starting at 0 volts and ending at 110 volts.
            var Technique = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique;
            Assert.IsTrue(Technique.ProcessType.ResultTypes.Contains("voltage"), "Failed Result Types");
            Assert.IsTrue(Technique.ProcessType.ResultTypes.Count == 1, "Failed Result Types");
            var result_range = SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.ResultRanges[0];
            var result_start = result_range.Start;
            var result_end = result_range.End;
            Assert.AreEqual<string>("at", result_start.Test, "Failed Result Start Type");
            Assert.AreEqual<double>(0.0, (double)result_start.BaseValue, "Failed Result Start Value");
            Assert.AreEqual<string>("at", result_end.Test, "Failed Result End Type");
            Assert.AreEqual<double>(110, (double)result_end.BaseValue, "Failed Result End Value");

            // A template is the primary structure that holds the atomic CMC informational elements in an SOA.
            // Once a template is selected a great deal of information is made readily available.
            var template = SampleSOA.CapabilityScope.Activities[0].CMCs[0].Templates[0];

            // a CMC Function Name provides a linkage between the template a CMC Uncertainty Function contained in a Technique
            string functionName = template.CMCUncertaintyFunctions[0].Name;
            Assert.AreEqual<string>("Measure.Voltage.AC.LowVoltage.Uncertainty.Certified", functionName, "Failed CMCFunction Name");

            // A CMC Uncertainty Function is used to calculate a CMC uncertainty value at the value of a specific test point.
            // Units of Measurement for all values in the function are unambiguously specified by specifying the measurement quantity
            //   of each value, in a CMC Parameter definition for each value.  These definitions are contained within the Technique.
            // All values utilize the base Unit of Measurement for the value's specified measurement quantity
            string functionExpression = template.getCMCUncertaintyFunctionExpression(functionName);
            Assert.AreEqual<string>("k_nominal * nominal + k_range * range", functionExpression, "Failed CMCFunction By Name");

            // "k_nominal", "nominal", "k_range", and "range" are the function Symbols.
            //  These symbols come directly from parsing a CMC Uncertainty Function's text.
            //  Once the values for all of these symbols have been set, the function may be evaluated.
            var functionSymbols = template.getCMCUncertaintyFunctionSymbols(functionName);
            Assert.AreEqual<int>(4, functionSymbols.Count, "Failed Function Symbols");


            // "frequency" and "nominal" are the Range Variables.
            //  The values of these variables will be used in the selection a Range subordinate to this template. 
            //  The values from these variables must come from a source external to the SOA.        
            var rangeVariables = template.getCMCFunctionRangeVariables(functionName);
            Assert.AreEqual<int>(2, rangeVariables.Count, "Failed Distinct Range Variables");

            // "nominal" and "range" are the function variables.  
            //  The values from these variables must come from a source external to the SOA.
            //  These variables should also appear in function symbols.
            //  The function symbols with same name must have their values set before the function may be evaluated.
            var functionVariables = template.getCMCFunctionVariables(functionName);
            Assert.AreEqual<int>(2, functionVariables.Count, "Failed Distinct Function Variables");

            // "k_nominal" and "k_range" are the function constants.  
            //  The values for these constants are contained in the SOA in a Range Element that is subordinate to this template.
            //  These constants should also appear in function symbols.
            //  The function symbols with same name must have their values set before the function may be evaluated.
            var functionConstants= template.getCMCFunctionConstants(functionName);
            Assert.AreEqual<int>(2, functionConstants.Count, "Failed Distinct Function Constants");


            // "Resolution" and "Connection" are the Assertion Names
            // The values of these assertions will be used in the selection a Range subordinate to this template
            var assertionNames = template.getCMCFunctionAssertionNames(functionName); 
            Assert.AreEqual<int>(2, assertionNames.Count, "Failed Distinct AssertionNames");
            Assert.IsTrue(assertionNames.Contains("Resolution") && assertionNames.Contains("Connection"), "Failed Get AssertionNames");


            // "2 Wire" and "4 Wire" are the set of expected values for the Assertion named "Connection".
            var assertionValues2 = template.getCMCFunctionAssertionValues(functionName, "Connection");
            Assert.AreEqual<int>(2, assertionValues2.Count, "Failed Distinct Assertion Values");
            Assert.IsTrue(assertionValues2.Contains("2 Wire") && assertionValues2.Contains("4 Wire"), "Failed Get AssertionValues By Assertion Name");

            // "6-1/2 digit", "5-1/2 digit", and "4-1/2 digit" are the set expected values for the Assertion named "Resolution".
            var assertionValues1 = template.getCMCFunctionAssertionValues(functionName, "Resolution");
            Assert.AreEqual<int>(3, assertionValues1.Count, "Failed Distinct Assertion Values");
           
            // Once a range that is subordinate to this template is selected, the values of Constants may be retrieved from the range.
            var range1 = template.CMCUncertaintyFunctions[0].Cases[0].Ranges[0].Ranges[0];     
            var constantValue1 = range1.ConstantValues[0];

            Assert.AreEqual<string>("k_nominal", constantValue1.const_parameter_name, "Failed ConstantValue Const_parameter_name");
            Assert.AreEqual<string>("0.0001", constantValue1.Value, "Failed Get ConstantValue Value");
            Assert.AreEqual<string>("ratio", constantValue1.Quantity, "Failed Get ConstantValue Quantity");
            Assert.AreEqual<string>("percent", constantValue1.Uom_alternative, "Failed Get ConstantValue UOM Alternative");

            // All numeric uses of values are performed in the base Unit Of Measurement for the value's measurement quantity
            // A values's BaseValue property contains this numeric value of a Value in its base Unit Of Measurement. 
            // It is derived with knowledge of the Value's uom_alternative property.
            // In this case the Value's quantity is "ratio" and its uom_alternative is "percent".
            // Therefor the BaseValue = the specified value (0.0001) divided by 100, which is 0.000001
            double baseValue = (double)constantValue1.BaseValue;
            Assert.AreEqual<double>(0.000001, baseValue, "Failed Converting a ConstantValue's Value To its Base UOM Value");

            // Once a Range has been selected, a loop such as the one that follows can set the values of the CMCFunction's symbols that are constants.
            // Whereas, setting the values of the CMCFunction's symbols that are variables requires custom logic, 
            //   because the values must come from an external source.
            foreach (var constant in range1.ConstantValues)
            {
                template.setCMCFunctionSymbol(functionName, constant.const_parameter_name, (double)constant.BaseValue);
            }
            template.setCMCFunctionSymbol(functionName, "nominal", 2.5);
            template.setCMCFunctionSymbol(functionName, "range", 10);

            // With all the functions values now set, the function may be evaluated.
            double uncertainty = (double)template.evaluateCMCFunction(functionName);
            Assert.AreEqual<double>(2.25E-05, uncertainty, "Failed Uncertainty Calculation");

        }
    }
}
