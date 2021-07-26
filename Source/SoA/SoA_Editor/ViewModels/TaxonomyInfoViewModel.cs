using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using SoA_Editor.Models;
using MT_DataAccessLib;
using Parameter = MT_DataAccessLib.Parameter;

namespace SoA_Editor.ViewModels
{
    public class TaxonomyInfoViewModel : Screen
    {
        private string _selectedOptionForTaxonomy;
        private Taxon _selectedTaxon;
        private bool _canSelectATaxonomy = false;
        private TaxonomyFactory taxonFactory = null;
        //Soa SampleSOA;

        private BindableCollection<string> _taxonomyOptions = new BindableCollection<string>();
        private BindableCollection<string> _taxonomyContent = new BindableCollection<string>();
        private BindableCollection<string> _selectedTaxonomy = new BindableCollection<string>();
        private BindableCollection<Taxon> _taxons = new BindableCollection<Taxon>(); // ProcessType is defined in the Models

        private Taxon _currentTaxon; // to be saved for a company

        private BindableCollection<MeasurementParameter> _optionalParameters = new BindableCollection<MeasurementParameter>();
        private BindableCollection<MeasurementParameter> _requiredParameters = new BindableCollection<MeasurementParameter>();
        private BindableCollection<Result> _results = new BindableCollection<Result>(); 

        public TaxonomyInfoViewModel(List<string> names)
        {
            //SampleSOA = new Soa();

            TaxonomyOptions.Add("Source");
            TaxonomyOptions.Add("Measure");

            _currentTaxon = new Taxon();

            taxonFactory = new();
            // only add to the list if it has not already been added
            foreach (Taxon taxon in taxonFactory.GetAllTaxons())
            {
                if (!names.Contains(taxon.Name))
                    Taxons.Add(taxon);
            }

            //select a default value
            SelectedOptionForTaxonomy = "Source";
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
                    foreach (Taxon taxon in Taxons)
                    {
                        if (taxon.Name.ToLower().Contains("source"))
                        {
                            SelectedTaxonomy.Add(taxon.Name);
                        }
                    }
                    CanSelectATaxonomy = IsSelectedTaxonomyEmpty();
                }
                else if (string.Equals(value, "Measure"))
                {
                    SelectedTaxonomy.Clear();
                    foreach (Taxon taxon in Taxons)
                    {
                        if (taxon.Name.ToLower().Contains("measure"))
                        {
                            SelectedTaxonomy.Add(taxon.Name);
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
            else if (!SelectedTaxonomy.Any())
            {
                return false;
            }

            return false;
        }

        public Taxon SelectedTaxon
        {
            get { return _selectedTaxon; }
            set
            {
                _selectedTaxon = value;

                // SelectedOptionForTaxonomy + SelectedProcessType; // Source + Volts.AC

                foreach (Taxon taxon in Taxons)
                {
                    if (taxon.Name.ToLower().Contains(SelectedOptionForTaxonomy.ToLower()) && taxon.Name.Equals(SelectedTaxon.Name))
                    {
                        CurrentTaxon = taxon;
                        break;
                    }
                }

                //Console.WriteLine("--- " + CurrentProcessType.Action + "." + CurrentProcessType.Taxonomy + " ---");

                RequiredParameters.Clear();
                foreach (Parameter param in CurrentTaxon.Parameters)
                {
                    if (!param.Optional)
                    {
                        MeasurementParameter mp = new MeasurementParameter(param.Name);
                        RequiredParameters.Add(mp);
                    }
                }

                OptionalParameters.Clear();
                foreach (Parameter param in CurrentTaxon.Parameters)
                {
                    if (param.Optional)
                    {
                        MeasurementParameter mp = new MeasurementParameter(param.Name);
                        OptionalParameters.Add(mp);
                    }
                }

                Results.Clear();
                if (CurrentTaxon.Results != null)
                {
                    foreach (Result result in CurrentTaxon.Results)
                    {
                        Results.Add(result);
                    }
                }
                

                NotifyOfPropertyChange(() => RequiredParameters);
                NotifyOfPropertyChange(() => OptionalParameters);
                NotifyOfPropertyChange(() => Results);
                NotifyOfPropertyChange(() => SelectedTaxon);
            }
        }

        public void okButton(Object obj)
        {
            Helper.TreeViewSelectedTaxon = SelectedTaxon;
            this.TryCloseAsync(null);
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

        public BindableCollection<Taxon> Taxons
        {
            get { return _taxons; }
            set { _taxons = value; }
        }

        public Taxon CurrentTaxon
        {
            get { return _currentTaxon; }
            set { _currentTaxon = value; }
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

        public BindableCollection<Result> Results
        {
            get { return _results; }
            set
            {
                _results = value;
                NotifyOfPropertyChange(() => Results);
            }
        }
    }
}