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
            Optional = optional;
            quantityDictionary = UomDataSource.getQuantities();
            Quantities = new();
            foreach (KeyValuePair<string, UomDataSource.Quantity> entry in quantityDictionary)
            {
                // Format for Display
                if (entry.Value != null)
                    quantities.Add(Quantity.FormatUomQuantity(entry.Value));
            }
            if (qty != "")
            {
                Quantity = Quantities.SingleOrDefault(q => q.QuantitiyName == qty);
            }
        }
        private Dictionary<string, UomDataSource.Quantity> quantityDictionary;

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
    }
}
