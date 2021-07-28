using Caliburn.Micro;
using SOA_DataAccessLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoA_Editor.ViewModels
{
    class InputParameterDialogViewModel : Screen
    {
        public InputParameterDialogViewModel(Mtc_Parameters parameters, string name = "", string qty = "", bool optional = false)
        {
            
            ParamName = name;
            if (qty != "")
            {
                var tempQty = UomDataSource.getQuantity(qty);
                Quantity = Quantity.FormatUomQuantity(tempQty);
            }            
            Optional = optional;
            quantityDictionary = UomDataSource.getQuantities();
            Quantities = new();
            dialog = new();
            dialog.Title = "Validation Error";
            dialog.Button = System.Windows.MessageBoxButton.OK;
            dialog.Image = System.Windows.MessageBoxImage.Error;
            Parameters = parameters;
        }
        private Dictionary<string, UomDataSource.Quantity> quantityDictionary;
        private Mtc_Parameters Parameters;
        private Helper.MessageDialog dialog;

        #region Properties

        private string paramName;
        public string ParamName
        {
            get { return paramName; }
            set
            {
                paramName = value;
                NotifyOfPropertyChange(() => ParamName);
            }
        }

        private string error = "";
        public string Error
        {
            get { return error; }
            set
            {
                error = value;
                NotifyOfPropertyChange(() => Error);
            }
        }

        private bool optional = false;
        public bool Optional
        {
            get { return optional; }
            set
            {
                optional = value;
                NotifyOfPropertyChange(() => Optional);
            }
        }

        private ObservableCollection<Quantity> quantities;
        public ObservableCollection<Quantity> Quantities
        {
            get
            {
                if (quantities.Count == 0)
                {
                    foreach (KeyValuePair<string, UomDataSource.Quantity> entry in quantityDictionary)
                    {
                        // Helperat for Display
                        if (entry.Value != null)
                            quantities.Add(Quantity.FormatUomQuantity(entry.Value));
                    }
                }
                return quantities;
            }
            set
            {
                quantities = value;
                NotifyOfPropertyChange(() => Quantities);
            }
        }

        private Quantity quantity;
        public Quantity Quantity
        {
            get { return quantity; }
            set
            {
                quantity = value;
                NotifyOfPropertyChange(() => Quantity);
            }
        }

        #endregion

        #region Methods

        public void Save(bool result)
        {
            // If we made it here the fields have already been validated
            // durning the close dialog event
            if (result)
            {
                Parameters.Add(new Mtc_Parameter(ParamName, Quantity.QuantitiyName, Optional));
            }
        }

        #endregion
    }
}
