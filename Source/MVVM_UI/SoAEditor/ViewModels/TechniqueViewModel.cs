using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoAEditor.Models;
using System.Collections.ObjectModel;

namespace SoAEditor.ViewModels
{
    public class TechniqueViewModel : Screen
    {
        public TechniqueViewModel() {
            InputParameterRanges = new ObservableCollection<Technique_InputParameterRange>();
            InputParameters = new ObservableCollection<Technique_InputParameter>();
            Outputs = new ObservableCollection<Technique_Output>();
            Variables = new ObservableCollection<Technique_Variable>();
        }

        private string _TaxonomyName;

        public string TaxonomyName
        {
            get { return _TaxonomyName; }
            set { _TaxonomyName = value; NotifyOfPropertyChange(() => TaxonomyName); }
        }

        private string _TechniqueName;

        public string TechniqueName
        {
            get { return _TechniqueName; }
            set { _TechniqueName = value; NotifyOfPropertyChange(() => TechniqueName); }
        }

        private string _Formula;

        public string Formula
        {
            get { return _Formula; }
            set { _Formula = value; NotifyOfPropertyChange(() => Formula); }
        }

        private string _Documentation;

        public string Documentation
        {
            get { return _Documentation; }
            set { _Documentation = value; NotifyOfPropertyChange(() => Documentation); }
        }

        private ObservableCollection<Technique_InputParameterRange> _InputParameterRanges;

        public ObservableCollection<Technique_InputParameterRange> InputParameterRanges
        {
            get
            {
                return _InputParameterRanges;
            }
            set
            {
                Set(ref _InputParameterRanges, value);
            }
        }


        private ObservableCollection<Technique_InputParameter> _InputParameters;

        public ObservableCollection<Technique_InputParameter> InputParameters
        {
            get
            {
                return _InputParameters;
            }
            set
            {
                Set(ref _InputParameters, value);
            }
        }


        private ObservableCollection<Technique_Output> _Outputs;

        public ObservableCollection<Technique_Output> Outputs
        {
            get
            {
                return _Outputs;
            }
            set
            {
                Set(ref _Outputs, value);
            }
        }


        private ObservableCollection<Technique_Variable> _Variables;

        public ObservableCollection<Technique_Variable> Variables
        {
            get
            {
                return _Variables;
            }
            set
            {
                Set(ref _Variables, value);
            }
        }
    }
}
