using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace MT_DataAccessLib
{
    static class Namespaces
    {
        public const string BASE_UIR = "https://cls-schemas.s3.us-west-1.amazonaws.com/";
        public const string MTC = BASE_UIR + "MetrologyTaxonomyCatalog";
        public const string UOM = BASE_UIR + "UOM_Database";
        public const string MathML_NS = "http://www.w3.org/1998/Math/MathML";
        public const string MathML_SL = "http://www.w3.org/Math/XMLSchema/mathml3/mathml3.xsd";
    }
    
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    [XmlRoot(Namespace = Namespaces.MTC, IsNullable = false)]
    public class Taxonomy
    {
        private List<Taxon> taxons;

        [XmlElement("Taxon")]
        public List<Taxon> Taxons
        {
            get { return taxons; }
            set { taxons = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class Taxon : ICloneable
    {
        public Taxon() { }
        protected Taxon(Taxon taxon)
        {
            Name = taxon.Name;
            Deprecated = taxon.Deprecated;
            Replacement = taxon.Replacement;
            ExternalReference = taxon.ExternalReference;
            ExternalReferences = taxon.ExternalReferences;
            Parameters = taxon.Parameters;
            Results = taxon.Results;
            Discipline = taxon.Discipline;
            Definition = taxon.Definition;
        }
        
        private string name = "";

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private bool deprecated;

        [XmlAttribute("deprecated")]
        public bool Deprecated
        {
            get { return deprecated; }
            set { deprecated = value; }
        }

        private string replacement = "";

        [XmlAttribute("replacement")]
        public string Replacement
        {
            get { return replacement; }
            set { replacement = value; }
        }

        private ExternalReference externalReference;

        [XmlElement("ExternalReference", IsNullable = false)]
        public ExternalReference ExternalReference
        {
            get { return externalReference; }
            set { externalReference = value; }
        }

        private ExternalReferences externalReferences;

        [XmlElement("ExternalReferences", IsNullable = false)]
        public ExternalReferences ExternalReferences
        {
            get { return externalReferences; }
            set { externalReferences = value; }
        }

        private List<Result> results;

        [XmlElement("Result")]
        public List<Result> Results
        {
            get { return results; }
            set { results = value; }
        }

        private List<Parameter> parameters;

        [XmlElement("Parameter")]
        public List<Parameter> Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        [XmlElement("Discipline", IsNullable = false)]
        public Discipline Discipline
        {
            get { return discipline; }
            set { discipline = value; }
        }

        private string definition = "";

        [XmlElement("Definition")]
        public string Definition
        {
            get { return definition; }
            set { definition = TaxonomyFactory.Tools.Format(value); }
        }

        private Discipline discipline;

        public object Clone()
        {
            return new Taxon(this);
        }

        //private string documentation = "";

        //[XmlElement("Documentation")]
        //public string Documentation
        //{
        //    get { return documentation;  }
        //    set { documentation = value;  }
        //}
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class Result
    {
        private string name = "";

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private Quantity quantity;

        [XmlElement("Quantity", Namespace = Namespaces.UOM)]
        public Quantity Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class Parameter
    {
        private string name = "";

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private bool optional = false;

        [XmlAttribute("optional")]
        public bool Optional
        {
            get { return optional; }
            set { optional = value; }
        }

        private Quantity quantity;

        [XmlElement("Quantity", Namespace = Namespaces.UOM)]
        public Quantity Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        private string definition = "";

        public string Definition
        {
            get { return definition; }
            set { definition = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.UOM)]
    public class Quantity
    {
        private string name;

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class Discipline
    {
        private string name = "";

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private List<string> subDisciplines;

        [XmlElement("SubDiscipline", IsNullable = false)]
        public List<string> SubDisciplines
        {
            get { return subDisciplines; }
            set { subDisciplines = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class ExternalReferences
    {
        private List<Reference> references;

        [XmlElement("Reference", IsNullable=false)]
        public List<Reference> References
        {
            get { return references; }
            set { references = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class Reference
    {
        private ReferenceUrl referenceUrl;

        [XmlElement("ReferenceUrl", IsNullable=false)]
        public ReferenceUrl ReferenceUrl
        {
            get { return referenceUrl; }
            set { referenceUrl = value; }
        }

        private List<CategoryTag> categoryTagList;

        [XmlElement("CategoryTag", IsNullable = false)]
        public List<CategoryTag> CategoryTagList
        {
            get { return categoryTagList; }
            set { categoryTagList = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class ReferenceUrl
    {
        private string name = string.Empty;

        [XmlElement("name")]
        public string UrlName
        {
            get { return name; }
            set { name = value; }
        }

        private string value = string.Empty;

        [XmlElement("url")]
        public string UrlValue
        {
            get { return value; }
            set { this.value = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class ExternalReference
    {
        private string name = "";

        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private List<CategoryTag> categoryTags;

        [XmlElement("CategoryTag", IsNullable = false)]
        public List<CategoryTag> CategoryTags
        {
            get { return categoryTags; }
            set { categoryTags = value; }
        }

        private string url = "";

        [XmlElement("url")]
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = Namespaces.MTC)]
    public class CategoryTag
    {
        private string name = "";

        [XmlElement("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string value = "";

        [XmlElement("value")]
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
}