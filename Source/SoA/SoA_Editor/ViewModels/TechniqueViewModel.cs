using Caliburn.Micro;
using SoA_Editor.Models;
using System.Collections.ObjectModel;
using SOA_DataAccessLib;
using SoA_Editor.Views;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Windows.Controls;

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

        #region Properties

        private InputParameterDialogView inputParamView;

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


        private Technique_InputParameter _InputParameter;
        public Technique_InputParameter InputParameter
        {
            get { return _InputParameter; }
            set
            {
                _InputParameter = value;
                NotifyOfPropertyChange(() => InputParameter);
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

        #endregion

        #region Methods        

        public async void OpenInputParameterDialog()
        {
            // set up our dialog view
            inputParamView = new InputParameterDialogView()
            {
                DataContext = new InputParameterDialogViewModel(Technique.Technique.Parameters)
            };

            // Reset the error
            var viewModel = (InputParameterDialogViewModel)inputParamView.DataContext;
            viewModel.Error = "";

            // get our result
            object result = await DialogHost.Show(inputParamView, "TechniqueDialog", ClosingEventHandler);
            
            // if passed validation in ClosingEventHandler, save the parameter
            viewModel.Save((bool)result);

            // Add the parameter to our view
            var param = Technique.Technique.Parameters[Technique.Technique.Parameters.Count() - 1];
            InputParameters.Add(new Technique_InputParameter(param.name, param.Quantity.name, param.optional, false, ""));
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter)
            {
                var viewModel = (InputParameterDialogViewModel)inputParamView.DataContext;
                // validate
                bool error = false;
                if (viewModel.ParamName == "" || viewModel.ParamName == null)
                {
                    viewModel.Error = "Please enter a Parameter Name";
                    error = true;
                }

                else if (viewModel.Quantity == null)
                {
                    viewModel.Error = "Please select a Quantity";
                    error = true;
                }

                else if (viewModel.Quantity != null && UomDataSource.getQuantity(viewModel.Quantity.QuantitiyName) == null)
                {
                    viewModel.Error = "Please select a valid Quantity from the list.";
                    error = true;
                }

                if (error) eventArgs.Cancel();
            }
        }

        #endregion
    }
}
