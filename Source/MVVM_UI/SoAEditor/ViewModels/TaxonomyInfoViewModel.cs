using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SoAEditor.Models;

namespace SoAEditor.ViewModels
{
   public class TaxonomyInfoViewModel : Screen
    {
        private string _selectedOptionForTaxonomy;
        private string _selectedProcessType;
        private bool _canSelectATaxonomy = false;
        //Soa SampleSOA;

        private BindableCollection<string> _taxonomyOptions = new BindableCollection<string>();
        private BindableCollection<string> _taxonomyContent = new BindableCollection<string>();
        private BindableCollection<string> _selectedTaxonomy = new BindableCollection<string>();
        private BindableCollection<ProcessType> _processTypes = new BindableCollection<ProcessType>(); // ProcessType is defined in the Models

        private ProcessType _currentProcessType; // to be saved for a company

        private BindableCollection<MeasurementParameter> _optionalParameters = new BindableCollection<MeasurementParameter>();
        private BindableCollection<MeasurementParameter> _requiredParameters = new BindableCollection<MeasurementParameter>();

        public TaxonomyInfoViewModel()
        {
            //SampleSOA = new Soa();

            TaxonomyOptions.Add("Source");
            TaxonomyOptions.Add("Measure");

            _currentProcessType = new ProcessType();

            LoadTaxonomyDatabase();
        }

        public void LoadTaxonomyDatabase()
        {
            XmlDocument db = new XmlDocument();
            db.Load(@"c:\temp\MetrologyNET_Taxonomy_v2.xml"); //the path should be updated in the final version

            //////////////////////////////

            XmlNodeList ptNodesList = db.GetElementsByTagName("mtc:ProcessType");

            foreach(XmlNode xmlNode in ptNodesList)
            {
                ProcessType tempPt = new ProcessType(); // Model object to be filled by XML node
                String tempName = xmlNode.Attributes["name"].Value;
                //Console.WriteLine(tempName);
                if(tempName.StartsWith("Source"))     // taxonomy contains: <mtc:ProcessType name="D0AD73A4-E43E-4B9A-9C41-9A54281C18BC">, change or delete it
                {
                    tempPt.Action = "Source";
                    tempPt.Taxonomy = tempName.Substring(7);
                }
                else if (tempName.StartsWith("Measure"))
                {
                    tempPt.Action = "Measure";
                    tempPt.Taxonomy = tempName.Substring(8);
                }

                XmlNodeList childNodeList = xmlNode.ChildNodes;
                foreach(XmlNode childNode in childNodeList)
                {
                    if(childNode.Name.Equals("mtc:Parameter"))
                    {
                        bool isOptional = false;
                        XmlAttributeCollection attributes = childNode.Attributes;
                        foreach (XmlAttribute xmlAttribute in attributes)
                        {
                            if (xmlAttribute.Name.Equals("optional") && xmlAttribute.Value.Equals("true")) // if there exist an optional attribute and its value is true...
                            {
                                isOptional = true;
                            }
                        }

                        if (isOptional == true) // optional parameter
                        {
                            tempPt.OptionalParameters.Add(new MeasurementParameter(childNode.Attributes["name"].Value));
                        }
                        else if (isOptional == false)
                        {
                            tempPt.RequiredParameters.Add(new MeasurementParameter(childNode.Attributes["name"].Value));
                        }
                    }
                }

                ProcessTypes.Add(tempPt);
            }
        }

        public string SelectedOptionForTaxonomy
        {
            get { return _selectedOptionForTaxonomy; }
            set
            {
                _selectedOptionForTaxonomy = value;
                if (string.Equals(value, "Source"))
                {
                    SelectedTaxonomy.Clear();
                    foreach (ProcessType processType in ProcessTypes)
                    {
                        if (processType.Action.Equals("Source"))
                        {
                            SelectedTaxonomy.Add(processType.Taxonomy);
                        }
                    }
                    CanSelectATaxonomy = IsSelectedTaxonomyEmpty();
                }
                else if (string.Equals(value, "Measure"))
                {
                    SelectedTaxonomy.Clear();
                    foreach (ProcessType processType in ProcessTypes)
                    {
                        if (processType.Action.Equals("Measure"))
                        {
                            SelectedTaxonomy.Add(processType.Taxonomy);
                        }
                    }
                    CanSelectATaxonomy = IsSelectedTaxonomyEmpty();
                }
                NotifyOfPropertyChange(() => SelectedOptionForTaxonomy);
            }
        }

        public bool IsSelectedTaxonomyEmpty()
        {
            if (SelectedTaxonomy.Any())
            {
                return true;
            }
            else if(!SelectedTaxonomy.Any())
            {
                return false;
            }

            return false;

        }

        public string SelectedProcessType
        {
            get { return _selectedProcessType; }
            set
            {
                _selectedProcessType = value;

                // SelectedOptionForTaxonomy + SelectedProcessType; // Source + Volts.AC
                
                foreach(ProcessType processType in ProcessTypes)
                {
                    if(processType.Action.Equals(SelectedOptionForTaxonomy) && processType.Taxonomy.Equals(SelectedProcessType))
                    {
                        CurrentProcessType = processType;
                        break;
                    }
                }

                //Console.WriteLine("--- " + CurrentProcessType.Action + "." + CurrentProcessType.Taxonomy + " ---");

                RequiredParameters.Clear();
                foreach (MeasurementParameter mp in CurrentProcessType.RequiredParameters)
                {
                    RequiredParameters.Add(mp);
                }

                OptionalParameters.Clear();
                foreach (MeasurementParameter mp in CurrentProcessType.OptionalParameters)
                {
                    OptionalParameters.Add(mp);
                }

                NotifyOfPropertyChange(() => RequiredParameters);
                NotifyOfPropertyChange(() => OptionalParameters);
                NotifyOfPropertyChange(() => SelectedProcessType);
            }
        }

        public bool CanSelectATaxonomy
        {
            get { return _canSelectATaxonomy; }
            set
            {
                _canSelectATaxonomy = value;
                NotifyOfPropertyChange(() => CanSelectATaxonomy);
            }
        }

        public BindableCollection<string> TaxonomyOptions
        {
            get { return _taxonomyOptions; }
            set { _taxonomyOptions = value; }
        }

        public BindableCollection<string> SelectedTaxonomy
        {
            get { return _selectedTaxonomy; }
            set { _selectedTaxonomy = value; }
        }

     
        public BindableCollection<string> TaxonomyContent
        {
            get { return _taxonomyContent; }
            set { _taxonomyContent = value; }
        }

        public BindableCollection<ProcessType> ProcessTypes
        {
            get { return _processTypes; }
            set { _processTypes = value; }
        }

        public ProcessType CurrentProcessType
        {
            get { return _currentProcessType; }
            set { _currentProcessType = value; }
        }

        public BindableCollection<MeasurementParameter> OptionalParameters
        {
            get { return _optionalParameters; }
            set
            {
                _optionalParameters = value;
                NotifyOfPropertyChange(() => OptionalParameters);
            }
        }

        public BindableCollection<MeasurementParameter> RequiredParameters
        {
            get { return _requiredParameters; }
            set
            {
                _requiredParameters = value;
                NotifyOfPropertyChange(() => RequiredParameters);
            }
        }

    }


}
