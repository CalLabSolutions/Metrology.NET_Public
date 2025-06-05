using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CalLabSolutions.TaxonManager;
using System.Collections.Generic;
using System.IO;

namespace CalLabSolutions.TaxonManagerTest
{

    [TestClass]
    public class TaxonManagerTests
    {
        [TestMethod]
        public void TestFileToTaxonomy()
        {
            string[] args = { "test" };
            TaxonManager.TaxonManager tm = new TaxonManager.TaxonManager(args);
            List<string> fileNames = new List<string>();
            string dir = Directory.GetCurrentDirectory();
            fileNames.Add(dir + "..\\..\\..\\..\\Mocks\\MeasurandTaxonomyCatalog.xml");
            List<Taxon> taxons = TaxonManager.TaxonManager.FileToTaxonomyConverter(fileNames, true);
            Assert.IsTrue(taxons.Count > 1, "Multiple taxons not created");
            fileNames.RemoveAt(0);
            fileNames.Add(dir + "..\\..\\..\\..\\Mocks\\Measure_Capacitance.xml");
            taxons = TaxonManager.TaxonManager.FileToTaxonomyConverter(fileNames);
            Assert.IsTrue(taxons.Count == 1, "Single taxon not created");
        }

        [TestMethod]
        public void TestToolsHtml()
        {
            List<Taxon> taxons = getTaxons();
            string taxonHtml = Tools.CreateTaxonHtml(taxons[0]);
            Assert.IsTrue(taxonHtml.Contains("html"), "Not formatted HTML");
            Assert.IsTrue(taxonHtml.Contains(taxons[0].Name), "Name not applied to the html");
            taxons = getTaxons(3);
            Taxonomy taxonomy = new Taxonomy();
            taxonomy.Taxons = taxons;
            string taxonomyHtml = Tools.CreateTaxonomyHtml(taxonomy);
            Assert.IsTrue(taxonomyHtml.Contains("html"), "Not formatted HTML");
            Assert.IsTrue(taxonomyHtml.Contains(taxons[0].Name), "Does not have first taxon");
            Assert.IsTrue(taxonomyHtml.Contains(taxons[2].Name), "Does not have last taxon");
        }

        private List<Taxon> getTaxons(int index = 1)
        {
            List<Taxon> taxons = new List<Taxon>();
            Taxon taxon = new Taxon();
            for (int i = 0; i < index; i++)
            {
                taxon.Name = "TestName_" + index.ToString();
                taxon.Definition = "This is a test taxon";
                taxon.Discipline = new Discipline();
                taxon.Discipline.Name = "Testing";
                taxon.Deprecated = false;
                taxon.Parameters = new List<Parameter>();
                taxon.Parameters.Add(new Parameter());
                taxon.Parameters[0].Name = "Test";
                taxon.ExternalReferences = new ExternalReferences();
                taxon.ExternalReferences.References = new List<Reference>();
                Reference refer = new Reference();
                refer.ReferenceUrl = new ReferenceUrl();
                refer.ReferenceUrl.UrlValue = "http://someURl";
                refer.ReferenceUrl.UrlName ="Some name";
                CategoryTag catTag = new CategoryTag();
                catTag.Name = "Some Cat name";
                catTag.Value = "Some Category";
                refer.CategoryTagList = new List<CategoryTag>();
                refer.CategoryTagList.Add(catTag);
                taxon.ExternalReferences.References.Add(refer);
                taxons.Add(taxon);
            }
            return taxons;
        }
    }
}
