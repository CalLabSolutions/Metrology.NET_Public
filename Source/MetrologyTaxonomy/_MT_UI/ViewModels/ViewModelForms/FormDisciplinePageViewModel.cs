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
    class FormDisciplinePageViewModel : Notifier
    {
        private ContentDialog dialog = new ContentDialog
        {
            Title = "Validation Error",
            CloseButtonText = "Ok"
        };

        public FormDisciplinePageViewModel()
        {
            // // Set up the object
            if (Form.TaxonToSave.Discipline == null)
            {
                Form.TaxonToSave.Discipline = new Discipline
                {
                    SubDisciplines = new List<string>()
                };
            }
               
            else if (Form.TaxonToSave.Discipline != null && Form.TaxonToSave.Discipline.SubDisciplines == null)
            {
                Form.TaxonToSave.Discipline.SubDisciplines = new List<string>();
            }

            Name = Form.TaxonToSave.Discipline.Name;
            SubDisciplines = new ObservableCollection<string>(Form.TaxonToSave.Discipline.SubDisciplines);
        }

        private ICommand addSubDiscipline;
        public ICommand AddSubDiscipline
        {
            get
            {
                if (addSubDiscipline == null)
                {
                    addSubDiscipline = new RelayCommand<string>((subName) => AddSub(subName));
                }
                return addSubDiscipline;
            }
            set
            {
                addSubDiscipline = value;
                OnPropertyChanged("AddSubDiscipline");
            }
        }

        private ICommand deleteSubDiscipline;
        public ICommand DeleteSubDiscipline
        {
            get
            {
                if (deleteSubDiscipline == null)
                {
                    deleteSubDiscipline = new RelayCommand<string>((subName) => DeleteSub(subName));
                }
                return deleteSubDiscipline;
            }
            set
            {
                deleteSubDiscipline = value;
                OnPropertyChanged("DeleteSubDiscipline");
            }
        }

        private ObservableCollection<string> subDisciplines;
        public ObservableCollection<string> SubDisciplines
        {
            get { return subDisciplines; }
            set
            {
                subDisciplines = value;
                OnPropertyChanged("SubDisciplines");
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                Form.TaxonToSave.Discipline.Name = value;
                OnPropertyChanged("Name");
            }
        }

        private string subName;
        public string SubName
        {
            get { return subName; }
            set
            {
                subName = value;
                Form.TaxonToSave.Discipline.Name = value;
                OnPropertyChanged("SubName");
            }
        }

        private async void AddSub(string subName)
        {
            // make sure the name is not already in use
            if (SubDisciplines.Where(s => s.ToLower().Equals(subName)).ToList().Count > 0)
            {
                dialog.Content = "That Sub Discipline already exists";
                await dialog.ShowAsync();
                return;
            }
            SubDisciplines.Add(subName);
            Form.TaxonToSave.Discipline.SubDisciplines = new List<string>(SubDisciplines);
        }

        private void DeleteSub(string subName)
        {
            SubDisciplines.RemoveAll(s => s.ToLower().Equals(subName.ToLower()));
            Form.TaxonToSave.Discipline.SubDisciplines = new List<string>(SubDisciplines);
        }
    }
}
