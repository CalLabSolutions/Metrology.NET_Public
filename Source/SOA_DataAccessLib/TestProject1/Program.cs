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
            XDocument doc = new XDocument();
            SampleSOA.writeTo(doc);
        }
    }
}
