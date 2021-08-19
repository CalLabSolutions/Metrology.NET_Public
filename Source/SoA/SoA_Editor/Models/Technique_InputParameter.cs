using Caliburn.Micro;
using SOA_DataAccessLib;
using SoA_Editor.ViewModels;

namespace SoA_Editor.Models
{
    public class Technique_InputParameter : PropertyChangedBase
    {
        public Technique_InputParameter(string inputParam, string quantity, bool optional, bool variable, string vType)
        {
            string qty = "";
            if (quantity != "")
            {
                var uom_qty = UomDataSource.getQuantity(quantity);
                if (uom_qty != null)
                {
                    qty = ViewModels.Quantity.FormatUomQuantity(uom_qty).FormatedName;
                }
            }
            InputParam = inputParam;
            Quantity = qty;
            Variable = variable;
            VariableType = vType;
            allowUpdate = true;
            Optional = optional ? "Yes" : "No";
        }

        private bool allowUpdate = false;
        public static TechniqueViewModel TechniqueVM = null;
        private Mtc_Symbol.SymbolType SymbolType;

        private string _InputParam;

        public string InputParam
        {
            get { return _InputParam; }
            set { _InputParam = value; NotifyOfPropertyChange(() => InputParam); }
        }

        private string _Quantity;

        public string Quantity
        {
            get { return _Quantity; }
            set { _Quantity = value; NotifyOfPropertyChange(() => Quantity); }
        }

        private string optional;

        public string Optional
        {
            get { return optional; }
            set
            {
                optional = value;
                NotifyOfPropertyChange(() => Optional);
            }
        }

        private string _VariableType;

        public string VariableType
        {
            get { return _VariableType; }
            set
            {
                _VariableType = value;
                if (VariableType.ToLower() == "variable")
                    SymbolType = Mtc_Symbol.SymbolType.Variable;
                else
                    SymbolType = Mtc_Symbol.SymbolType.Constant;
                UpdateVarType();
                NotifyOfPropertyChange(() => VariableType);
            }
        }

        private bool _Variable;

        public bool Variable
        {
            get { return _Variable; }
            set
            {
                _Variable = value;
                if (value)
                {
                    VariableType = "Variable";
                }
                else
                {
                    VariableType = "";
                }
                UpdateVarList();
                NotifyOfPropertyChange(() => Variable);
            }
        }

        // Add or remove a Variable from the list
        public void UpdateVarList()
        {
            TechniqueVM = TechniqueViewModel.Instance;

            if (TechniqueVM != null && allowUpdate)
            {
                Mtc_Technique technique = TechniqueVM.Technique.Technique;

                if (Variable)
                {
                    AddSymbol(technique);
                }
                else
                {
                    foreach (Technique_Variable variable in TechniqueVM.Variables)
                    {
                        if (variable.Value == InputParam)
                        {
                            TechniqueVM.Variables.Remove(variable);
                            RemoveSymbol(technique);
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateVarType()
        {
            TechniqueVM = TechniqueViewModel.Instance;

            if (TechniqueVM != null && allowUpdate)
            {
                Mtc_Technique technique = TechniqueVM.Technique.Technique;
                if (Variable)
                {
                    foreach (Technique_Variable variable in TechniqueVM.Variables)
                    {
                        // update the UI and the soa lists
                        if (variable.Value == InputParam)
                        {
                            technique.CMCUncertainties[0].SymbolDefinitions[InputParam].type = VariableType;
                            variable.Type = VariableType;

                            if (SymbolType == Mtc_Symbol.SymbolType.Variable && technique.CMCUncertainties[0].Constants.Contains(InputParam))
                            {
                                technique.CMCUncertainties[0].Constants.Remove(InputParam);
                                technique.CMCUncertainties[0].Variables.Add(InputParam);
                            }
                            else if (SymbolType == Mtc_Symbol.SymbolType.Constant && technique.CMCUncertainties[0].Variables.Contains(InputParam))
                            {
                                technique.CMCUncertainties[0].Variables.Remove(InputParam);
                                technique.CMCUncertainties[0].Constants.Add(InputParam);
                            }
                            else if (SymbolType == Mtc_Symbol.SymbolType.Variable)
                            {
                                technique.CMCUncertainties[0].Variables.Add(InputParam);
                            }
                            else
                            {
                                technique.CMCUncertainties[0].Constants.Add(InputParam);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void AddSymbol(Mtc_Technique technique)
        {
            Mtc_Symbol symbol;
            if (SymbolType == Mtc_Symbol.SymbolType.Variable)
            {
                symbol = new(technique.Parameters, InputParam, Mtc_Symbol.SymbolType.Variable);
                technique.CMCUncertainties[0].Variables.Add(InputParam);
            }
            else
            {
                symbol = new(technique.Parameters, InputParam, Mtc_Symbol.SymbolType.Constant);
                technique.CMCUncertainties[0].Constants.Add(InputParam);
            }

            technique.CMCUncertainties[0].SymbolDefinitions.Add(symbol);
            technique.CMCUncertainties[0].ExpressionSymbols.Add(symbol.parameter);
            TechniqueVM.Variables.Add(new Technique_Variable(symbol.parameter, VariableType));
        }

        private void RemoveSymbol(Mtc_Technique technique)
        {
            Mtc_Symbol symbol = TechniqueVM.Technique.Technique.CMCUncertainties[0].SymbolDefinitions[InputParam];
            technique.CMCUncertainties[0].SymbolDefinitions.Remove(symbol);

            // remove from expression symbol
            foreach (string expSymbol in technique.CMCUncertainties[0].ExpressionSymbols)
            {
                if (expSymbol == InputParam)
                {
                    technique.CMCUncertainties[0].ExpressionSymbols.Remove(expSymbol);
                    break;
                }
            }
            // Remove from Variables or Constants
            if (VariableType.ToLower() == "variable")
            {
                foreach (string vari in technique.CMCUncertainties[0].Variables)
                {
                    if (vari == InputParam)
                    {
                        technique.CMCUncertainties[0].Variables.Remove(vari);
                        break;
                    }
                }
            }
            else
            {
                foreach (string constant in technique.CMCUncertainties[0].Constants)
                {
                    if (constant == InputParam)
                    {
                        technique.CMCUncertainties[0].Constants.Remove(constant);
                        break;
                    }
                }
            }
        }
    }
}