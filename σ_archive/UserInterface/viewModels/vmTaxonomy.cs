using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace soa_1_03
{
    public class vmTaxonomy : Inpc, ICommand
    {
        #region Private Properties
        private List<string> _actions;
        private string _action;
        private List<string> _quantities;
        private string _quantity;
        private List<string> _baseAltNames;
        private string _baseAltName;
        private List<string> _symbols;
        private string _symbol;
        private ObservableCollection<mSoa> _vmSoa;
        private mClient _vmClient;
        private bool _enableButton;
        #endregion

        #region Constructor
        public vmTaxonomy()
        {
            this.actions = new List<string>() { "measure", "source" };
            this.quantities = new List<string>();
            this.baseAltNames = new List<string>();
            this.symbols = new List<string>();
            this.enableButton = false;
            this.vmSoa = new ObservableCollection<mSoa>
            {
                new mSoa(){soaAction = "Measure"},
                new mSoa(){soaAction = "Source"}
            };
            this.vmClient = new mClient();
        }
        #endregion

        #region Public Properties
        public ObservableCollection<mSoa> vmSoa
        {
            get { return _vmSoa; }
            set
            {
                if(value != _vmSoa)
                {
                    _vmSoa = value;
                    OnPropertyChanged("vmSoa");
                }
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public List<string> actions
        {
            get { return _actions; }
            set
            {
                if (_actions != value)
                {
                    _actions = value;
                    OnPropertyChanged("actions");
                }
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public string action
        {
            get { return _action; }
            set
            {
                if (value != _action)
                {
                    _action = value;
                    EnableButtonCheck();
                    OnPropertyChanged("action");
                }
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public List<string> quantities
        {
            get
            {
                GetQuantitiesStrings();
                return _quantities;
            }
            set
            {
                if (value != _quantities)
                {
                    _quantities = value;
                    OnPropertyChanged("quantities");
                }
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public string quantity
        {
            get { return _quantity; }
            set
            {
                if (value != _quantity)
                {
                    _quantity = value;
                    NewQuantityValue();
                    EnableButtonCheck();
                    OnPropertyChanged("quantity");
                }
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public List<string> baseAltNames
        {
            get { return _baseAltNames; }
            set
            {
                if (value != _baseAltNames)
                {
                    _baseAltNames = value;
                    OnPropertyChanged("baseAltNames");
                }
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public string baseAltName
        {
            get { return _baseAltName; }
            set
            {
                if (value != _baseAltName)
                {
                    _baseAltName = value;
                    NewBaseAltNameValue();
                    EnableButtonCheck();
                    OnPropertyChanged("baseAltName");
                }
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public List<string> symbols
        {
            get { return _symbols; }
            set
            {
                if (value != _symbols)
                {
                    _symbols = value;
                    OnPropertyChanged("symbols");
                }
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public string symbol
        {
            get { return _symbol; }
            set
            {
                if (value != _symbol)
                {
                    _symbol = value;
                    EnableButtonCheck();
                    OnPropertyChanged("symbol");
                }
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public bool enableButton
        {
            get { return _enableButton; }
            set
            {
                if (value != _enableButton)
                {
                    _enableButton = value;
                    OnPropertyChanged("enableButton");
                }
            }
        }

        public mClient vmClient
        {
            get { return _vmClient; }
            set
            {
                if(value != _vmClient)
                {
                    _vmClient = value;
                    OnPropertyChanged("vmClient");
                }
            }
        }
        #endregion

        #region Methods
        private void GetQuantitiesStrings()
        {
            objTaxonomy t = objTaxonomy.GetInstance();
            foreach (taxonomyQuantity tq in t.quantities)
            {
                this._quantities.Add(tq.quantityName);
            }
        }

        private void NewQuantityValue()
        {
            objTaxonomy t = objTaxonomy.GetInstance();
            taxonomyQuantity tq = t.quantities.Find(x => x.quantityName == this._quantity);
            List<string> ls = new List<string>();
            foreach (taxonomyNames tn in tq.baseAltNames)
            {
                ls.Add(tn.baseAltName);
            }
            this.baseAltNames = ls;
        }

        private void NewBaseAltNameValue()
        {
            if (this.baseAltName == null)
            {
                this.symbol = null;
                return;
            }
            objTaxonomy t = objTaxonomy.GetInstance();
            taxonomyQuantity tq = t.quantities.Find(x => x.quantityName == this._quantity);
            taxonomyNames tn = tq.baseAltNames.Find(x => x.baseAltName == this._baseAltName);
            this.symbols = tn.taxonomyNamesSymbols;
        }

        public void AddSoaTaxonomy()
        {
            //TODO: Add check to make sure the defined taxonomy doesn't already exist
            //TODO: Add flag to mark a taxonomy as a new user-defined taxonomy (flag used to save new taxonomy to file)
            mSoaTaxonomy st = new mSoaTaxonomy() { soaQuantity = this.quantity, soaName = this.baseAltName, soaSymbol = this.symbol };
            ObservableCollection<mSoa> tempVmSoa = this.vmSoa;
            foreach (mSoa s in tempVmSoa)
            {
                if (s.soaAction.Equals(this.action, StringComparison.InvariantCultureIgnoreCase))
                {
                    s.soaTaxonomies.Add(st);
                    
                }
            }
            this.vmSoa = tempVmSoa;
        }

        public void EnableButtonCheck()
        {
            //if all string properties are set, then enable button to set taxonomy
            if (this.quantity != null && this.action != null && this.baseAltName != null && this.symbol != null)
            {
                this.enableButton = true;
            }
            else { this.enableButton = false; }
        }

        public void vmPopulateTestData()
        {
            mSoaTechniqueDescriptor descriptor = new mSoaTechniqueDescriptor()
            {
                descriptor = "Type J",
                soaTechniques = new ObservableCollection<mSoaTechnique>()
                {
                    new mSoaTechnique(){ rangeMin = "-210", rangeMax = "-100", uncertainty = "\u00B1" + "0.3"},
                    new mSoaTechnique(){rangeMin = "-100", rangeMax = "-50", uncertainty = "\u00B1" + "0.2"}
                }
            };

            mSoaTechniqueDescriptor descriptor02 = new mSoaTechniqueDescriptor()
            {
                descriptor = "Type K",
                soaTechniques = new ObservableCollection<mSoaTechnique>()
                {
                    new mSoaTechnique(){ rangeMin = "-250", rangeMax = "-100", uncertainty = "\u00B1" + "0.5"}
                }
            };


            this.vmSoa[0].soaTaxonomies.Add(new mSoaTaxonomy()
            {
                soaQuantity = "Temperature",
                soaName = "Kelvin",
                soaSymbol = "K",
            });

            this.vmSoa[0].soaTaxonomies[0].soaTechniqueDescriptors.Add(descriptor);
            this.vmSoa[0].soaTaxonomies[0].soaTechniqueDescriptors.Add(descriptor02);

            int y = 0;
            for (int i = 0; i < 2; i++)
            {
                this.vmSoa[i].soaTaxonomies.Add(new mSoaTaxonomy()
                {
                    soaQuantity = "Voltage",
                    soaName = "Millivolts",
                    soaSymbol = "mV",
                });

                mSoaTechniqueDescriptor firstDescriptor = new mSoaTechniqueDescriptor()
                {
                    descriptor = "3458A",
                    soaTechniques = new ObservableCollection<mSoaTechnique>()
                {
                    new mSoaTechnique(){ rangeMin = "-200", rangeMax = "-20", uncertainty = "\u00B1" + "0.3"},
                    new mSoaTechnique() {rangeMin = "-20", rangeMax = "-2", uncertainty = "\u00B1" + "0.1"}
                }
                };

                mSoaTechniqueDescriptor secondDescriptor = new mSoaTechniqueDescriptor()
                {
                    descriptor = "34401A",
                    soaTechniques = new ObservableCollection<mSoaTechnique>()
                {
                    new mSoaTechnique(){ rangeMin = "-200", rangeMax = "-20", uncertainty = "\u00B1" + "0.5"},
                    new mSoaTechnique(){ rangeMin = "-20", rangeMax = "-2", uncertainty = "\u00B1" + "0.3"}
                }
                };

                if (i == 0) { y = 1; }
                else { y = 0; }
                this.vmSoa[i].soaTaxonomies[y].soaTechniqueDescriptors.Add(firstDescriptor);
                this.vmSoa[i].soaTaxonomies[y].soaTechniqueDescriptors.Add(secondDescriptor);
            }

            this.vmClient.company = "TEGAM, Inc.";
            this.vmClient.facility = "Geneva";
        }
        #endregion

        #region ICommand Methods
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            AddSoaTaxonomy();
        }
        #endregion

    }

    public class vmTechnique : Inpc
    {
        private string _descriptor1;

        public string descriptor1
        {
            get { return _descriptor1; }
            set
            {
                if (value != _descriptor1)
                {
                    _descriptor1 = value;
                    OnPropertyChanged("descriptor1");
                }
            }
        }
        public string descriptor2;
        public string rangeMin;
        public string rangeMax;
        public string cmc;
    }
}
