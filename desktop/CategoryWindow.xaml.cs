using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BookCatalog
{
    public partial class CategoryWindow : Window
    {
        private ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();
        private bool _dataChanged = false;

        public CategoryWindow()
        {
            InitializeComponent();
            categoriesListBox.ItemsSource = Categories;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCategories();
        }

        private async Task LoadCategories()
        {
            try
            {
                List<Category> fetchedCategories = await API.GetCategories();
                Categories.Clear();
                foreach (var cat in fetchedCategories.OrderBy(c => c.Name))
                {
                    Categories.Add(cat);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void addCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            string newCategoryName = categoryNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(newCategoryName))
            {
                MessageBox.Show("Category name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await API.CreateCategory(newCategoryName);
                categoryNameTextBox.Clear();
                await LoadCategories();
                _dataChanged = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void editCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (categoriesListBox.SelectedItem is Category selectedCategory)
            {
                string newName = Microsoft.VisualBasic.Interaction.InputBox($"Enter new name for '{selectedCategory.Name}':", "Edit Category", selectedCategory.Name);

                if (!string.IsNullOrWhiteSpace(newName) && newName.Trim() != selectedCategory.Name)
                {
                    try
                    {
                        await API.UpdateCategory(selectedCategory.Id, newName.Trim());
                        await LoadCategories();
                        _dataChanged = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void deleteCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (categoriesListBox.SelectedItem is Category selectedCategory)
            {
                if (MessageBox.Show($"Are you sure you want to delete category '{selectedCategory.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        await API.DeleteCategory(selectedCategory.Id);
                        await LoadCategories();
                        _dataChanged = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void categoriesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isItemSelected = categoriesListBox.SelectedItem != null;
            editCategoryButton.IsEnabled = isItemSelected;
            deleteCategoryButton.IsEnabled = isItemSelected;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = _dataChanged;
            this.Close();
        }
    }
}