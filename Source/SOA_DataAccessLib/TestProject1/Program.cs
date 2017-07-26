using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SOA_DataAccessLibrary;
using System.Xml.Linq;

namespace TestProject1
{
    class Program
    {
        static void Main(string[] args)
        {
            SOA_DataAccess dao = new SOA_DataAccess();
            dao.load("http://schema.metrology.net/SOASample_TwoParameter_SixCases_TwoAssertions_ComplexFormula.xml");
            Soa SampleSOA = dao.SOADataMaster;
            SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.Parameters.add("Resolution", new Mtc_Enumeration(@"6-1/2 digits", @"5-1/2 digits", @"4-1/2 digits"), false);
            SampleSOA.CapabilityScope.Activities[0].Techniques[0].Technique.Parameters.add("Connection", new Mtc_Enumeration("2 Wire", "4 Wire"), false);
            XDocument doc = new XDocument();
            SampleSOA.writeTo(doc);
        }
    }
}
