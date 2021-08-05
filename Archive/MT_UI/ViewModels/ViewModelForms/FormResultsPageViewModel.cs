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
    class FormResultsPageViewModel : Notifier
    {
        private ContentDialog dialog = new ContentDialog
        {
            Title = "Validation Error",
            CloseButtonText = "Ok"
        };

        public FormResultsPageViewModel()
        {
            // // Set up the object
            if (Form.TaxonToSave.Results == null)
            {
                Form.TaxonToSave.Results = new List<Result>();
            }

            Results = new ObservableCollection<Result>(Form.TaxonToSave.Results);
            quantitiesDic = UomDataSource.getQuantities();
        }

        #region commands

        private ICommand addResult;
        public ICommand AddResult
        {
            get
            {
                if (addResult == null)
                {
                    addResult = new RelayCommand(async () =>
                    {
                        if (Validate())
                        {
                            var q = new MT_DataAccessLib.Quantity()
                            {
                                Name = SelectedQuantity.QuantitiyName
                            };
                            Result param = new Result()
                            {
                                Name = Name,                                
                                Quantity = q

                            };
                            Results.Add(param);
                            Form.TaxonToSave.Results = new List<Result>(Results);
                        }
                        else
                        {
                            _ = await dialog.ShowAsync();
                        }
                    });
                }
                return addResult;
            }
            set
            {
                addResult = value;
                OnPropertyChanged("AddResult");
            }
        }

        private ICommand deleteResult;
        public ICommand DeleteResult
        {
            get
            {
                if (deleteResult == null)
                {
                    deleteResult = new RelayCommand<string>((name) => DeleteByName(name));
                }
                return deleteResult;
            }
            set
            {
                deleteResult = value;
                OnPropertyChanged("DeleteResult");
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

        private ObservableCollection<Result> results;

        public ObservableCollection<Result> Results
        {
            get { return results; }
            set
            {
                if (value == null) return;
                results = value;
                OnPropertyChanged("Results");

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
                dialog.Content = "A Result must have a name";
                return false;
            }

            if (SelectedQuantity == null)
            {
                dialog.Content = "A Result must have a Quantity";
                return false;
            }

            // make sure the name is not already in use
            if (Results.Where(r => r.Name.ToLower().Equals(Name)).ToList().Count > 0)
            {
                dialog.Content = "That Result Name already exists";
                return false;
            }

            return true;
        }

        private void DeleteByName(string name)
        {
            Results.RemoveAll(p => p.Name.ToLower().Equals(name.ToLower()));
            Form.TaxonToSave.Results = new List<Result>(Results);
        }
    }
}
