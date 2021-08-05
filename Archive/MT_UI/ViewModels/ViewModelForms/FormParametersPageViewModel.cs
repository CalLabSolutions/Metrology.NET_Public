using GalaSoft.MvvmLight.Command;
using MT_DataAccessLib;
using MT_UI.Extensions;
using MT_UI.Pages;
using MT_UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace MT_UI.ViewModels.ViewModelForms
{
    class FormParametersPageViewModel : Notifier
    {

        private ContentDialog dialog = new ContentDialog
        {
            Title = "Validation Error",
            CloseButtonText = "Ok"
        };

        public FormParametersPageViewModel()
        {
            // // Set up the object
            if (Form.TaxonToSave.Parameters == null)
            {
                Form.TaxonToSave.Parameters = new List<Parameter>();
            }

            Parameters = new ObservableCollection<Parameter>(Form.TaxonToSave.Parameters);
            quantitiesDic = UomDataSource.getQuantities();
        }

        #region commands

        private ICommand addParam;
        public ICommand AddParam
        {
            get
            {
                if (addParam == null)
                {
                    addParam = new RelayCommand(async () =>
                    {
                        if (Validate())
                        {
                            var q = new MT_DataAccessLib.Quantity()
                            {
                                Name = SelectedQuantity.QuantitiyName
                            };
                            Parameter param = new Parameter()
                            {
                                Name = Name,
                                Definition = Definition,
                                Optional = Optional,
                                Quantity = q
                                
                            };
                            Parameters.Add(param);
                            Form.TaxonToSave.Parameters = new List<Parameter>(Parameters);
                        }
                        else
                        {
                            _ = await dialog.ShowAsync();
                        }
                    });
                }
                return addParam;
            }
            set
            {
                addParam = value;
                OnPropertyChanged("AddParam");
            }
        }

        private ICommand deleteParam;
        public ICommand DeleteParam
        {
            get
            {
                if (deleteParam == null)
                {
                    deleteParam = new RelayCommand<string>((name) => DeleteByName(name));
                }
                return deleteParam;
            }
            set
            {
                deleteParam = value;
                OnPropertyChanged("DeleteParam");
            }
        }

        #endregion

        #region properties

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        private string definition;
        public string Definition
        {
            get { return definition; }
            set
            {
                definition = value;
                OnPropertyChanged("Definition");
            }
        }

        private bool optional = false;
        public bool Optional
        {
            get { return optional; }
            set
            {
                optional = value;
                OnPropertyChanged("Optional");
            }
        }

        private Quantity selectedQuantity;
        public Quantity SelectedQuantity
        {
            get { return selectedQuantity; }
            set
            {
                if (value == null) return;
                selectedQuantity = value;
                OnPropertyChanged("SelectedQuantity");
            }
        }

        private ObservableCollection<Parameter> parameters;

        public ObservableCollection<Parameter> Parameters
        {
            get { return parameters; }
            set
            {
                if (value == null) return;
                parameters = value;
                OnPropertyChanged("Parameters");

            }
        }

        private Dictionary<string, UomDataSource.Quantity> quantitiesDic;

        private List<Quantity> quantities = new List<Quantity>();
        public List<Quantity> Quantities
        {
            get
            {
                if (quantities.Count == 0)
                {
                    foreach (KeyValuePair<string, UomDataSource.Quantity> entry in quantitiesDic)
                    {
                        // Format for Display      
                        if (entry.Value != null)
                            quantities.Add(Quantity.FormatUomQuantity(entry.Value));
                    }
                }
                return quantities;
            }
        }

        #endregion

        private bool Validate()
        {
            // verify required inputs
            if (Name == null || Name == "")
            {
                dialog.Content = "A Parameter must have a name";
                return false;
            }

            if (SelectedQuantity == null)
            {
                dialog.Content = "A Parameter must have a Quantity";
                return false;
            }

            // make sure the name is not already in use
            if (Parameters.Where(p => p.Name.ToLower().Equals(Name)).ToList().Count > 0)
            {
                dialog.Content = "That Parameter name already exists";
                return false;
            }

            return true;
        }

        private void DeleteByName(string name)
        {
            Parameters.RemoveAll(p => p.Name.ToLower().Equals(name.ToLower()));
            Form.TaxonToSave.Parameters = new List<Parameter>(Parameters);
        }
    }
}
