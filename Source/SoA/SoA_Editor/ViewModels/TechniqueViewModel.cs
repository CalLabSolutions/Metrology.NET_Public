using Caliburn.Micro;
using SoA_Editor.Models;
using System.Collections.ObjectModel;
using SOA_DataAccessLib;

namespace SoA_Editor.ViewModels
{
    public class TechniqueViewModel : Screen
    {
        public TechniqueViewModel() {
            InputParameterRanges = new ObservableCollection<Technique_InputParameterRange>();
            InputParameters = new ObservableCollection<Technique_InputParameter>();
            Outputs = new ObservableCollection<Technique_Output>();
            Variables = new ObservableCollection<Technique_Variable>();
            VariableTypes = new ObservableCollection<string>();
            VariableTypes.Add("Variable");
            VariableTypes.Add("Constant");
        }

        public static TechniqueViewModel Instance { get; set; }
        public Unc_Technique Technique = null;

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

        private string functionName;
        
        public string FunctionName
        {
            get { return functionName; }
            set { functionName = value; NotifyOfPropertyChange(() => FunctionName); }
        }

        private string category;

        public string Category
        {
            get { return category; }
            set { category = value; NotifyOfPropertyChange(() => Category); }
        }

        private BindableCollection<string> sourceEquipment = new BindableCollection<string>();

        public BindableCollection<string> SourceEquipment
        {
            get { return sourceEquipment; }
            set { sourceEquipment = value; NotifyOfPropertyChange(() => SourceEquipment); }
        }


        private BindableCollection<string> measureEquipment = new BindableCollection<string>();

        public BindableCollection<string> MeasureEquipment
        {
            get { return measureEquipment; }
            set { measureEquipment = value; NotifyOfPropertyChange(() => MeasureEquipment); }
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


        private ObservableCollection<string> variableTypes;

        public ObservableCollection<string> VariableTypes
        {
            get { return variableTypes; }
            set { variableTypes = value; NotifyOfPropertyChange(() => VariableTypes); }
        }

    }
}
