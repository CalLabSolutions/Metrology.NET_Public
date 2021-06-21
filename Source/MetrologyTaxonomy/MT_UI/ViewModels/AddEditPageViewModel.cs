using System;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using MT_DataAccessLib;
using MT_UI.Pages;
using MT_UI.Pages.Forms;
using MT_UI.Services;
using MT_UI.ViewModels.ViewModelForms;
using Windows.UI.Xaml.Controls;

namespace MT_UI.ViewModels
{
    class AddEditPageViewModel : Notifier
    {
        public AddEditPageViewModel()
        {
            factory = new TaxonomyFactory();
        }

        private readonly bool edit = MT_Data.SelectedTaxon != null;
        private TaxonomyFactory factory;
        private ContentDialog dialog = new ContentDialog
        {
            Title = "Validation Error",
            CloseButtonText = "Ok"
        };

        #region commands

        private ICommand saveTaxon;

        public ICommand SaveTaxon
        {
            get
            {
                if (saveTaxon == null)
                {
                    saveTaxon = new RelayCommand(async () =>
                    {
                        var currentFrame = Form.Frame.Content;
                        Form.Frame.Navigate(typeof(FormDetailsPage));
                        if (Validate())
                        {
                            if (edit)
                            {
                                var taxonomy = factory.Edit(Form.TaxonToSave, MT_Data.SelectedTaxon.Name);
                                factory.Save(taxonomy, MT_Data.SaveLocal);
                                MT_Data.ContentFrame.Navigate(typeof(ViewAllPage));
                                MT_Data.ViewAll.IsSelected = true;
                            }
                            else
                            {
                                var taxonomy = factory.Add(Form.TaxonToSave);
                                factory.Save(taxonomy, MT_Data.SaveLocal);
                                MT_Data.ContentFrame.Navigate(typeof(ViewAllPage));
                                MT_Data.ViewAll.IsSelected = true;
                            }
                        }
                        else
                        {
                            _ = await dialog.ShowAsync();
                            Form.Frame.Navigate(currentFrame.GetType());
                        }                        
                    });
                }
                return saveTaxon;
            }      
            set
            {
                saveTaxon = value;
                OnPropertyChanged("SaveTaxon");
            }
        }

        private ICommand details;

        public ICommand Details
        {
            get
            {
                if (details == null)
                {
                    details = new RelayCommand(() =>
                    {
                        if (Form.Frame.Content is FormDetailsPage) return;
                        Form.Frame.Navigate(typeof(FormDetailsPage));
                    });
                }
                return details;
            }
            set
            {
                details = value;
                OnPropertyChanged("Details");
            }
        }

        private ICommand parameters;

        public ICommand Parameters
        {
            get
            {
                if (parameters == null){
                    parameters = new RelayCommand(() =>
                    {
                        if (Form.Frame.Content is FormParametersPage) return;
                        Form.Frame.Navigate(typeof(FormParametersPage));
                    });
                }
                return parameters;
            }
            set
            {
                parameters = value;
                OnPropertyChanged("Parameters");
            }
        }

        private ICommand results;

        public ICommand Results
        {
            get
            {
                if (results == null)
                {
                    results = new RelayCommand(() =>
                    {
                        if (Form.Frame.Content is FormResultsPage) return;
                        Form.Frame.Navigate(typeof(FormResultsPage));
                    });
                }
                return results;
            }
            set
            {
                results = value;
                OnPropertyChanged("Results");
            }
        }

        private ICommand discipline;

        public ICommand Discipline
        {
            get
            {
                if (discipline == null)
                {
                    discipline = new RelayCommand(() =>
                    {
                        if (Form.Frame.Content is FormDisciplinePage) return;
                        Form.Frame.Navigate(typeof(FormDisciplinePage));
                    });
                }
                return discipline;
            }
            set
            {
                discipline = value;
                OnPropertyChanged("Discipline");
            }
        }

        private ICommand extRef;

        public ICommand ExtRef
        {
            get
            {
                if (extRef == null)
                {
                    extRef = new RelayCommand(() =>
                    {
                        if (Form.Frame.Content is FormExtRefPage) return;
                        Form.Frame.Navigate(typeof(FormExtRefPage));
                    });
                }
                return extRef;
            }
            set
            {
                extRef = value;
                OnPropertyChanged("ExtRef");
            }
        }

        #endregion

        private bool Validate()
        {

            // Make sure we have a well formated Taxon name and that is does not already exist
            // We need to check all the taxon for its elements
            var frame = (FormDetailsPage)Form.Frame.Content;
            var details = (FormDetailsPageViewModel)frame.DataContext;
            if (details.TypeStr == "" || details.TypeStr == null)
            {
                dialog.Content = "The Taxon must have a Type";
                return false;
            }
            if (details.QuantityStr == "" || details.QuantityStr == null)
            {
                dialog.Content = "Please select a Quantity";
                return false;
            }
            if (details.Process == "" || details.Process == null)
            {
                dialog.Content = "Please enter a Process";
                return false;
            }            

            // make sure it does not already exist
            if (factory.GetAllTaxons().Where(w => w.Name.ToLower().Equals(Form.TaxonToSave.Name.ToLower())).ToList().Count > 0)
            {
                dialog.Content = string.Format("Taxon \"{0}\" already exists.", Form.TaxonToSave.Name);
                return false;
            }

            // Make sure we have at least 1 Parameter and 1 Result
            if (Form.TaxonToSave.Parameters == null)
            {
                dialog.Content = "You must have at least 1 Parameter";
                return false;
            }
            if (Form.TaxonToSave.Parameters.Count == 0)
            {
                dialog.Content = "You must have at least 1 Parameter";
                return false;
            }
            if (Form.TaxonToSave.Results == null)
            {
                dialog.Content = "You must have at least 1 Result";
                return false;
            }
            if (Form.TaxonToSave.Results.Count == 0)
            {
                dialog.Content = "You must have at least 1 Result";
                return false;
            }

            return true;
           
        }
    }
}
