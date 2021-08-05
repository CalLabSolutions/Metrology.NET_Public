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
    class FormExtRefPageViewModel : Notifier
    {
        private ContentDialog dialog = new ContentDialog
        {
            Title = "Validation Error",
            CloseButtonText = "Ok"
        };

        public FormExtRefPageViewModel()
        {
            // Make sure our object is setup right
            if (Form.TaxonToSave.ExternalReference == null)
            {
                Form.TaxonToSave.ExternalReference = new ExternalReference
                {
                    CategoryTags = new List<CategoryTag>()
                };
            }
            
            else if (Form.TaxonToSave.ExternalReference != null && Form.TaxonToSave.ExternalReference.CategoryTags == null)
            {
                Form.TaxonToSave.ExternalReference.CategoryTags = new List<CategoryTag>(); 
            }

            Name = Form.TaxonToSave.ExternalReference.Name;
            Url = Form.TaxonToSave.ExternalReference.Url;
            CategoryTags = new ObservableCollection<CategoryTag>(Form.TaxonToSave.ExternalReference.CategoryTags);
        }

        private ICommand addCategoryTag;
        public ICommand AddCategoryTag
        {
            get
            {
                if (addCategoryTag == null)
                {
                    addCategoryTag = new RelayCommand<CategoryTag>((catTag) => AddCat(catTag));
                }
                return addCategoryTag;
            }
            set
            {
                addCategoryTag = value;
                OnPropertyChanged("AddCategoryTag");
            }
        }

        private ICommand deleteCategoryTag;
        public ICommand DeleteCategoryTag
        {
            get
            {
                if (deleteCategoryTag == null)
                {
                    deleteCategoryTag = new RelayCommand<CategoryTag>((catTag) => DeleteCat(catTag));
                }
                return deleteCategoryTag;
            }
            set
            {
                deleteCategoryTag = value;
                OnPropertyChanged("DeleteCategoryTag");
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                Form.TaxonToSave.ExternalReference.Name = value;
                OnPropertyChanged("Name");
            }
        }

        private string url;
        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                Form.TaxonToSave.ExternalReference.Url = value;
                OnPropertyChanged("Url");
            }
        }        

        private CategoryTag categoryTag = new CategoryTag();
        public CategoryTag CategoryTag
        {
            get { return categoryTag; }
            set
            {
                categoryTag = value;
                OnPropertyChanged("CategoryTag");
            }
        }

        private ObservableCollection<CategoryTag> categoryTags;
        public ObservableCollection<CategoryTag> CategoryTags
        {
            get { return categoryTags;  }
            set
            {
                categoryTags = value;
                OnPropertyChanged("CategoryTags");
            }
        }

        private async void AddCat(CategoryTag catTag)
        {              
            if (catTag.Name == null) catTag.Name = "";
            if (catTag.Name != "" && CategoryTags.Where(c => c.Name.ToLower().Equals(catTag.Name)).ToList().Count > 0)
            {
                dialog.Content = "That Category Name already exists";
                await dialog.ShowAsync();
                return;
            }
            if (catTag.Value == null)
            {
                dialog.Content = "Category Tag must at least have a Value";
                await dialog.ShowAsync();
                return;
            }
            CategoryTags.Add(catTag);
            Form.TaxonToSave.ExternalReference.CategoryTags = new List<CategoryTag>(CategoryTags);
            CategoryTag = new CategoryTag();
        }

        private void DeleteCat(CategoryTag catTag)
        {
           CategoryTags.RemoveAll(c => c.Value.ToLower().Equals(catTag.Value.ToLower()));
           Form.TaxonToSave.ExternalReference.CategoryTags = new List<CategoryTag>(CategoryTags);
        }
    }
}
