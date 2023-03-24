using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using SOA_DataAccessLib;
using SoA_Editor.Models;
using SoA_Editor.Views;
using System;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Globalization;
using System.Windows;

namespace SoA_Editor.ViewModels
{
    public class TechniqueViewModel : Screen
    {
        public TechniqueViewModel()
        {
            InputParameterRanges = new ObservableCollection<Technique_InputParameterRange>();
            InputParameters = new ObservableCollection<Technique_InputParameter>();
            Outputs = new ObservableCollection<Technique_Output>();
            Variables = new ObservableCollection<Technique_Variable>();
            Assertions = new ObservableCollection<RangeAssertion>();
        }

        #region Properties

        private InputParameterDialogView inputParamView;
        private InputParameterRangeDialogView inputParamRangeView;
        private OutputDialogView outputDialogView;
        private AssertionDialogView assertionDialogView;
        private NumberStyles styles = NumberStyles.Number | NumberStyles.AllowExponent;
        private CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

        public static TechniqueViewModel Instance { get; set; }
        public Unc_Technique Technique = null;
        public Unc_Template template = null;
        public Unc_CMC cmc = null;
        public TechniqueNode node;

        private string _TaxonomyName;

        public string TaxonomyName
        {
            get { return _TaxonomyName; }
            set
            {
                _TaxonomyName = value;
                NotifyOfPropertyChange(() => TaxonomyName);
            }
        }

        private string _TechniqueName;

        public string TechniqueName
        {
            get { return _TechniqueName; }
            set
            {
                _TechniqueName = value;
                NotifyOfPropertyChange(() => TechniqueName);
            }
        }

        private string _Formula;

        public string Formula
        {
            get { return _Formula; }
            set
            {
                _Formula = value;
                if (Technique != null)
                {
                    Technique.Technique.CMCUncertainties[0].Expression = _Formula;
                    Technique.Technique.CMCUncertainties[0].ResetExpression();
                }
                NotifyOfPropertyChange(() => Formula);
            }
        }

        private string _Documentation;

        public string Documentation
        {
            get { return _Documentation; }
            set
            {
                _Documentation = value;
                NotifyOfPropertyChange(() => Documentation);
            }
        }

        private string functionName;

        public string FunctionName
        {
            get { return functionName; }
            set
            {
                functionName = value;
                NotifyOfPropertyChange(() => FunctionName);
            }
        }

        private string category;

        public string Category
        {
            get { return category; }
            set
            {
                category = value;
                NotifyOfPropertyChange(() => Category);
            }
        }

        private BindableCollection<string> sourceEquipment = new BindableCollection<string>();

        public BindableCollection<string> SourceEquipment
        {
            get { return sourceEquipment; }
            set
            {
                sourceEquipment = value;
                NotifyOfPropertyChange(() => SourceEquipment);
            }
        }

        private BindableCollection<string> measureEquipment = new BindableCollection<string>();

        public BindableCollection<string> MeasureEquipment
        {
            get { return measureEquipment; }
            set
            {
                measureEquipment = value;
                NotifyOfPropertyChange(() => MeasureEquipment);
            }
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
            set
            {
                variableTypes = value;
                NotifyOfPropertyChange(() => VariableTypes);
            }
        }

        private Technique_Variable variable;

        public Technique_Variable Variable
        {
            get { return variable; }
            set
            {
                variable = value;
                Formula = Formula += " " + variable.Value;
                NotifyOfPropertyChange(() => Variable);
            }
        }

        private ObservableCollection<RangeAssertion> assertions;

        public ObservableCollection<RangeAssertion> Assertions
        {
            get { return assertions; }
            set
            {
                assertions = value;
                NotifyOfPropertyChange(() => Assertions);
            }
        }

        #endregion Properties

        #region Methods

        // Technique
        public void EditTechnique()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 450;
            settings.Title = "Edit Technique";

            IWindowManager manager = new WindowManager();
            manager.ShowDialogAsync(new TechniqueInfoViewModel(Technique, template, cmc), null, settings);
            if (Helper.TreeViewTechnique != null)
            {
                // Update Technique Info
                TechniqueName = Helper.TreeViewTechnique.Name;
                Category = cmc.Category.Name;
                FunctionName = Helper.TreeViewTechnique.Technique.CMCUncertainties[0].function_name;
                SourceEquipment.Clear();
                MeasureEquipment.Clear();
                foreach (Mtc_Role r in Helper.TreeViewTechnique.Technique.RequiredEquipment.Roles)
                {
                    if (r.Name.ToLower() == "source")
                    {
                        foreach (string d in r.DeviceTypes)
                        {
                            SourceEquipment.Add(d);
                        }
                    }
                    else
                    {
                        foreach (string d in r.DeviceTypes)
                        {
                            MeasureEquipment.Add(d);
                        }
                    }
                }

                node.Name = Helper.TreeViewTechnique.Name;
                node.IsExpanded = true;
                Helper.TreeViewTechnique = null;
            }
        }

        // Input Param Methods
        private async void UpdateInputParameter(Technique_InputParameter inputParameter)
        {
            // dont bother if the dialog is already open
            if (DialogHost.IsDialogOpen("RootDialog"))
            {
                return;
            }

            // We can not edit Taxon added Technique Parameters
            if (Technique.Technique.Taxon.Parameters[inputParameter.InputParam] != null)
            {
                Helper.MessageDialog message = new();
                message.Warning("This can not be edited, it was added by the Technique's Taxon");
                return;
            }

            var param = Technique.Technique.Parameters[inputParameter.InputParam];
            inputParamView = new InputParameterDialogView()
            {
                DataContext = new InputParameterDialogViewModel(Technique.Technique.Parameters, param.name, param.Quantity.name, param.optional)
            };

            // Reset the error
            var viewModel = (InputParameterDialogViewModel)inputParamView.DataContext;
            viewModel.Error = "";

            // get our result
            object result = await DialogHost.Show(inputParamView, "RootDialog", ClosingEventHandlerInputParam);

            if ((bool)result)
            {
                UpdateParamRangeAndVars(param.name, viewModel.ParamName);
                param.name = viewModel.ParamName;
                param.optional = viewModel.Optional;
                param.Quantity = UomDataSource.getQuantity(viewModel.Quantity.QuantitiyName);
                inputParameter.InputParam = param.name;
                inputParameter.Optional = !param.optional ? "Yes" : "No";
                inputParameter.Quantity = Quantity.FormatUomQuantity(param.Quantity).FormatedName;
            }
        }

        public async void AddInputParameter()
        {
            // dont bother if the dialog is already open
            if (DialogHost.IsDialogOpen("RootDialog"))
            {
                return;
            }

            // set up our dialog view
            inputParamView = new InputParameterDialogView()
            {
                DataContext = new InputParameterDialogViewModel(Technique.Technique.Parameters)
            };

            // Reset the error
            var viewModel = (InputParameterDialogViewModel)inputParamView.DataContext;
            viewModel.Error = "";

            // get our result
            object result = await DialogHost.Show(inputParamView, "RootDialog", ClosingEventHandlerInputParam);

            // if passed validation in ClosingEventHandler, save the parameter
            if ((bool)result)
            {
                // Add the param to our soa object
                Technique.Technique.Parameters.Add(new Mtc_Parameter(viewModel.ParamName, viewModel.Quantity.QuantitiyName, viewModel.Optional));

                // Add the parameter to our view
                var param = Technique.Technique.Parameters[viewModel.ParamName];
                InputParameters.Add(new Technique_InputParameter(param.name, param.Quantity.name, param.optional, false, "Influence Quantity"));
            }
        }

        public void EditInputParam(Technique_InputParameter inputParam)
        {
            UpdateInputParameter(inputParam);
        }

        public void DeleteInputParam(Technique_InputParameter inputParameter)
        {
            if (Technique.Technique.Taxon.Parameters[inputParameter.InputParam] != null)
            {
                Helper.MessageDialog message = new();
                message.Warning("This can not be deleted, it was added by the Technique's Taxon");
                return;
            }
            InputParameters.Remove(inputParameter);
            Technique.Technique.Parameters.Remove(Technique.Technique.Parameters[inputParameter.InputParam]);
            Technique.Technique.Taxon.Parameters.Remove(Technique.Technique.Taxon.Parameters[inputParameter.InputParam]);

            // remove any ranges and variable matching the input param
            foreach (Technique_InputParameterRange range in InputParameterRanges)
            {
                if (range.InputParamRange == inputParameter.InputParam)
                {
                    InputParameterRanges.Remove(range);
                    Technique.Technique.ParameterRanges.Remove(Technique.Technique.ParameterRanges[range.InputParamRange]);
                    break;
                }
            }

            foreach (Technique_Variable _var in Variables)
            {
                if (_var.Value == inputParameter.InputParam)
                {
                    Technique.Technique.CMCUncertainties[0].RemvoeSymbol(_var.Value);
                    Variables.Remove(_var);
                    break;
                }
            }
        }

        private void ClosingEventHandlerInputParam(object sender, DialogClosingEventArgs eventArgs)
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
                   
                }
                else if (Technique.Technique.Parameters[viewModel.ParamName] != null)
                {
                    viewModel.Error = "That Parameter already exists";
                    error = true;
                }

                if (error)
                {
                    eventArgs.Cancel();
                }
            }
        }

        // Input Param Ranges
        public async void AddInputParameterRange()
        {
            // dont bother if the dialog is already open
            if (DialogHost.IsDialogOpen("RootDialog")) return;

            // set up our dialog view
            inputParamRangeView = new InputParameterRangeDialogView()
            {
                DataContext = new InputParameterRangeDialogViewModel(Technique.Technique.ParameterRanges, Technique.Technique.Parameters)
            };

            // Reset the error
            var viewModel = (InputParameterRangeDialogViewModel)inputParamRangeView.DataContext;
            viewModel.Error = "";

            // get our result
            object result = await DialogHost.Show(inputParamRangeView, "RootDialog", ClosingEventHandlerInputParamRange);

            // if passed validation in ClosingEventHandler, save the parameter range
            if ((bool)result)
            {
                var param = Technique.Technique.Parameters[viewModel.ParamRangeName];

                Mtc_Range_Start start = new();
                start.Quantity = param.Quantity.name;
                start.symbol = param.Quantity.UoM.symbol;
                start.Value = new decimal(double.Parse(viewModel.Min, styles));
                start.ValueString = viewModel.Min.ToString();
                start.format = start.ValueString;
                start.test = viewModel.TestMin;

                Mtc_Range_End end = new();
                end.Quantity = param.Quantity.name;
                end.symbol = param.Quantity.UoM.symbol;
                end.Value = new decimal(double.Parse(viewModel.Max, styles));
                end.ValueString = viewModel.Max.ToString();
                end.format = start.ValueString;
                end.test = viewModel.TestMax;

                Technique.Technique.ParameterRanges.Add(new Mtc_Range()
                {
                    name = viewModel.ParamRangeName,
                    Start = start,
                    End = end
                });

                // Add the parameter to our view
                var paramR = Technique.Technique.ParameterRanges[viewModel.ParamRangeName];
                InputParameterRanges.Add(new Technique_InputParameterRange(paramR.name, paramR.Start.ValueString, paramR.End.ValueString, paramR.Start.test, paramR.End.test));
            }
        }

        private async void UpdateInputParameterRange(Technique_InputParameterRange inputParamRange)
        {
            // dont bother if the dialog is already open
            if (DialogHost.IsDialogOpen("RootDialog"))
            {
                return;
            }

            var param = Technique.Technique.ParameterRanges[inputParamRange.InputParamRange];
            inputParamRangeView = new InputParameterRangeDialogView()
            {
                DataContext = new InputParameterRangeDialogViewModel(Technique.Technique.ParameterRanges, Technique.Technique.Parameters,
                        param.name, param.Start.ValueString, param.End.ValueString, param.Start.test, param.End.test, true)
            };

            // Reset the error
            var viewModel = (InputParameterRangeDialogViewModel)inputParamRangeView.DataContext;
            viewModel.Error = "";

            // get our result

            object result = await DialogHost.Show(inputParamRangeView, "RootDialog", ClosingEventHandlerInputParamRange);

            if ((bool)result)
            {
                // update soa object
                param.name = viewModel.ParamRangeName;
                param.Start.Value = new decimal(double.Parse(viewModel.Min, styles));
                param.Start.ValueString = viewModel.Min;
                param.Start.test = viewModel.TestMin;
                param.End.Value = new decimal(double.Parse(viewModel.Max, styles));
                param.End.ValueString = viewModel.Max;
                param.End.test = viewModel.TestMax;

                // update UI object
                inputParamRange.InputParamRange = viewModel.ParamRangeName;
                inputParamRange.Min = viewModel.Min;
                inputParamRange.Max = viewModel.Max;
                inputParamRange.TestMin = viewModel.TestMin;
                inputParamRange.TestMax = viewModel.TestMax;
            }
        }

        public void EditInputParamRange(Technique_InputParameterRange inputParamRange)
        {
            UpdateInputParameterRange(inputParamRange);
        }

        public void DeleteInputParamRange(Technique_InputParameterRange inputParamRange)
        {
            InputParameterRanges.Remove(inputParamRange);
            Technique.Technique.ParameterRanges.Remove(Technique.Technique.ParameterRanges[inputParamRange.InputParamRange]);
        }

        private void ClosingEventHandlerInputParamRange(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter)
            {
                var viewModel = (InputParameterRangeDialogViewModel)inputParamRangeView.DataContext;
                // validate
                bool error = false;
                if (viewModel.ParamRangeName == null || viewModel.ParamRangeName == "")
                {
                    viewModel.Error = "Please select a Parameter Name";
                    error = true;
                }
                else if (viewModel.TestMax == null || viewModel.TestMax == "")
                {
                    viewModel.Error = "Please select a Maximum (at, before, or after)";
                    error = true;
                }
                else if (viewModel.TestsMin == null || viewModel.TestMin == "")
                {
                    viewModel.Error = "Please select a Minimum (at, before, or after)";
                    error = true;
                }
                double test;
                if (!double.TryParse(viewModel.Min, styles, culture, out test))
                {
                    viewModel.Error = "Please enter a valid Minimum number";
                    error = true;
                }
                if (!double.TryParse(viewModel.Max, styles, culture, out test))
                {
                    viewModel.Error = "Please enter a valid Maximum number";
                    error = true;
                }
                if (double.Parse(viewModel.Max) < double.Parse(viewModel.Min, styles))
                {
                    viewModel.Error = "Please make sure your Maximum value is greater than the Minimum value.";
                    error = true;
                }

                if (error)
                {
                    eventArgs.Cancel();
                }
            }
        }

        // Outputs
        public async void AddOutput()
        {
            // dont bother if the dialog is already open
            if (DialogHost.IsDialogOpen("RootDialog")) return;

            // set up our dialog view
            outputDialogView = new OutputDialogView()
            {
                DataContext = new OutputDialogViewModel()
            };

            // Reset the error
            var viewModel = (OutputDialogViewModel)outputDialogView.DataContext;
            viewModel.Error = "";

            // get our result
            object result = await DialogHost.Show(outputDialogView, "RootDialog", ClosingEventHandlerOutput);

            // if passed validation in ClosingEventHandler, save the parameter range
            if ((bool)result)
            {
                Mtc_Range_Start start = new();
                start.Value = new decimal(double.Parse(viewModel.Min, styles));
                start.ValueString = viewModel.Min.ToString();
                start.format = start.ValueString;
                start.test = viewModel.TestMin;

                Mtc_Range_End end = new();
                end.Value = new decimal(double.Parse(viewModel.Max, styles));
                end.ValueString = viewModel.Max.ToString();
                end.format = start.ValueString;
                end.test = viewModel.TestMax;

                Technique.Technique.ResultRanges.Add(new Mtc_Range()
                {
                    name = viewModel.OutputName,
                    Start = start,
                    End = end
                });

                // Add the parameter to our view
                var outputR = Technique.Technique.ResultRanges[viewModel.OutputName];
                Outputs.Add(new Technique_Output(outputR.name, outputR.Start.ValueString, outputR.End.ValueString, outputR.Start.test, outputR.End.test));
            }
        }

        private async void UpdateOutput(Technique_Output output)
        {
            // dont bother if the dialog is already open
            if (DialogHost.IsDialogOpen("RootDialog"))
            {
                return;
            }

            var outputR = Technique.Technique.ResultRanges[output.Output];
            outputDialogView = new OutputDialogView()
            {
                DataContext = new OutputDialogViewModel(outputR.name, output.Min, output.Max, output.TestMin, output.TestMax)
            };

            // Reset the error
            var viewModel = (OutputDialogViewModel)outputDialogView.DataContext;
            viewModel.Error = "";

            // get our result

            object result = await DialogHost.Show(outputDialogView, "RootDialog", ClosingEventHandlerOutput);

            if ((bool)result)
            {
                // update soa object
                outputR.name = viewModel.OutputName;
                outputR.Start.Value = new decimal(double.Parse(viewModel.Min, styles));
                outputR.Start.ValueString = viewModel.Min;
                outputR.Start.test = viewModel.TestMin;
                outputR.End.Value = new decimal(double.Parse(viewModel.Max, styles));
                outputR.End.ValueString = viewModel.Max;
                outputR.End.test = viewModel.TestMax;

                // update UI object
                output.Output = viewModel.OutputName;
                output.Min = viewModel.Min;
                output.Max = viewModel.Max;
                output.TestMin = viewModel.TestMin;
                output.TestMax = viewModel.TestMax;
            }
        }

        public void EditOutput(Technique_Output output)
        {
            UpdateOutput(output);
        }

        public void DeleteOutput(Technique_Output output)
        {
            Outputs.Remove(output);
            if (output.Output == "result")
            {
                output.Output = "";
            }

            Technique.Technique.ResultRanges.Remove(Technique.Technique.ResultRanges[output.Output]);
        }

        private void ClosingEventHandlerOutput(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter)
            {
                var viewModel = (OutputDialogViewModel)outputDialogView.DataContext;
                // validate
                bool error = false;
                if (viewModel.OutputName == null || viewModel.OutputName == "")
                {
                    viewModel.Error = "Please select a Parameter Name";
                    error = true;
                }
                else if (viewModel.TestMax == null || viewModel.TestMax == "")
                {
                    viewModel.Error = "Please select a Maximum (at, before, or after)";
                    error = true;
                }
                else if (viewModel.TestsMin == null || viewModel.TestMin == "")
                {
                    viewModel.Error = "Please select a Minimum (at, before, or after)";
                    error = true;
                }
                double test;
                if (!double.TryParse(viewModel.Min, styles, culture, out test))
                {
                    viewModel.Error = "Please enter a valid Minimum number";
                    error = true;
                }
                if (!double.TryParse(viewModel.Max, styles, culture, out test))
                {
                    viewModel.Error = "Please enter a valid Maximum number";
                    error = true;
                }
                if (double.Parse(viewModel.Max) < double.Parse(viewModel.Min, styles))
                {
                    viewModel.Error = "Please make sure your Maximum value is greater than the Minimum value.";
                    error = true;
                }

                if (error)
                {
                    eventArgs.Cancel();
                }
            }
        }

        private void UpdateParamRangeAndVars(string oldName, string newName)
        {
            foreach (Technique_InputParameterRange range in InputParameterRanges)
            {
                if (range.InputParamRange == oldName)
                {
                    range.InputParamRange = newName;
                    Technique.Technique.ParameterRanges[oldName].name = newName;
                    break;
                }
            }

            foreach (Technique_Variable _var in Variables)
            {
                if (_var.Value == oldName)
                {
                    _var.Value = newName;
                    Technique.Technique.CMCUncertainties[0].SymbolDefinitions[oldName].parameter = newName;
                    Technique.Technique.CMCUncertainties[0].EditSymbol(oldName, newName);
                    break;
                }
            }
        }

        // Assetions
        public async void AddAssertion()
        {
            // dont bother if the dialog is already open
            if (DialogHost.IsDialogOpen("RootDialog")) return;

            // set up our dialog view
            assertionDialogView = new AssertionDialogView()
            {
                DataContext = new AssertionDialogViewModel()
            };

            // Reset the error
            var viewModel = (AssertionDialogViewModel)assertionDialogView.DataContext;
            viewModel.Error = "";

            // get our result
            object result = await DialogHost.Show(assertionDialogView, "RootDialog", ClosingEventHandlerAssertion);

            if ((bool)result)
            {
                Technique.Technique.RangeAssertions.Add(viewModel.AssertionName);
                var ra = new RangeAssertion();
                ra.Name = viewModel.AssertionName;
                ra.Values = string.Join(", ", template.CMCUncertaintyFunctions[0].getAssertionValuesByAssertionName(viewModel.AssertionName));
                Assertions.Add(ra);
            }
        }

        public void EditAssertion(RangeAssertion assertion)
        {
            UpdateAssertion(assertion);
        }

        private async void UpdateAssertion(RangeAssertion assertion)
        {
            // dont bother if the dialog is already open
            if (DialogHost.IsDialogOpen("RootDialog")) return;

            // set up our dialog view
            assertionDialogView = new AssertionDialogView()
            {
                DataContext = new AssertionDialogViewModel(assertion.Name)
            };

            // Reset the error
            var viewModel = (AssertionDialogViewModel)assertionDialogView.DataContext;
            viewModel.Error = "";

            // get our result
            object result = await DialogHost.Show(assertionDialogView, "RootDialog", ClosingEventHandlerAssertion);

            if ((bool)result)
            {
                string oldName = assertion.Name;
                string newName = viewModel.AssertionName;
                Technique.Technique.RangeAssertions.Replace(oldName, newName);
                assertion.Name = newName;
                assertion.Values = string.Join(", ", template.CMCUncertaintyFunctions[0].getAssertionValuesByAssertionName(newName));
                // UpdateUncRangeAssertions(newName, "update", oldName);
            }
        }

        public void DeleteAssertion(RangeAssertion assertion)
        {
            Assertions.Remove(assertion);
            Technique.Technique.RangeAssertions.Remove(assertion.Name);
            // UpdateUncRangeAssertions(assertion.Name, "delete");
        }

        private void ClosingEventHandlerAssertion(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter)
            {
                var viewModel = (AssertionDialogViewModel)assertionDialogView.DataContext;

                // validate
                bool error = false;
                if (viewModel.AssertionName == null || viewModel.AssertionName == "")
                {
                    viewModel.Error = "Please enter an Assertion Name";
                    error = true;
                }

                if (error)
                {
                    eventArgs.Cancel();
                }
            }
        }

        // This could potentally cause havoc on the ranges. Lets not use this right now
        public void UpdateUncRangeAssertions(string name, string action, string oldName = "")
        {
            var cases = template.CMCUncertaintyFunctions[0].Cases;
            if (cases == null) return;
            if (cases.Count() == 0) return;
 
            switch (action)
            {
                case "delete":
                    foreach (var _case in cases)
                    {
                        foreach (var assertion in _case.Assertions)
                        {
                            if (assertion.Name == name)
                            {
                                _case.Assertions.Remove(assertion);
                                break;
                            }
                        }
                    }
                    break;
                case "add":
                    // for now we will not add new assertions to existing cases.
                    break;
                case "update":
                    foreach (var _case in cases)
                    {
                        foreach (var assertion in _case.Assertions)
                        {
                            if (assertion.Name == oldName)
                            {
                                assertion.Name = name;
                                break;
                            }
                        }
                    }
                    break;
            }
        }

        #endregion Methods
    }
}