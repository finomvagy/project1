using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace BookCatalog
{
    public partial class MainWindow : Window
    {
        private ComboBox categoryFilterComboBox;
        private TextBox searchTextBox;

        public MainWindow()
        {
            InitializeComponent();
            SetupToolbar();
        }

        void SetupToolbar()
        {
            toolBar.Children.Clear();

            searchTextBox = new TextBox { MinWidth = 150, Margin = new Thickness(5) };
            searchTextBox.TextChanged += SearchTextBox_TextChanged;
            toolBar.Children.Add(new Label { Content = "Search:", VerticalAlignment = VerticalAlignment.Center });
            toolBar.Children.Add(searchTextBox);

            categoryFilterComboBox = new ComboBox { MinWidth = 150, Margin = new Thickness(5) };
            categoryFilterComboBox.SelectionChanged += CategoryFilterComboBox_SelectionChanged;
            toolBar.Children.Add(new Label { Content = "Category:", VerticalAlignment = VerticalAlignment.Center });
            toolBar.Children.Add(categoryFilterComboBox);

            Button resetFiltersButton = NewToolbarButton("Reset Filters", (s, e) => {
                searchTextBox.Text = "";
                categoryFilterComboBox.SelectedIndex = 0;
            });

            if (!API.LoggedIn)
            {
                NewToolbarButton("Log in", LoginButton_Click);
            }
            else
            {
                NewToolbarButton("Log out", LogoutButton_Click);
                NewToolbarButton("User info", UserInfoButton_Click);
                NewToolbarButton("Add new book", AddButton_Click);
                NewToolbarButton("Manage Categories", ManageCategoriesButton_Click);
                NewToolbarButton("Category Stats", CategoryStatsButton_Click);
            }
            _ = LoadCategoryFilters();
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await UpdateBooks();
        }

        private async void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (categoryFilterComboBox.IsDropDownOpen || e.RemovedItems.Count > 0 || e.AddedItems.Count > 0) // Csak akkor fusson, ha ténylegesen a user változtatott
            {
                await UpdateBooks();
            }
        }

        async Task LoadCategoryFilters()
        {
            if (categoryFilterComboBox == null) return;

            categoryFilterComboBox.SelectionChanged -= CategoryFilterComboBox_SelectionChanged;

            var currentSelection = categoryFilterComboBox.SelectedItem as dynamic;

            categoryFilterComboBox.Items.Clear();
            categoryFilterComboBox.Items.Add(new { Id = (int?)null, Name = "All Categories" });

            if (API.LoggedIn || true)
            {
                try
                {
                    List<Category> categories = await API.GetCategories();
                    foreach (var cat in categories.OrderBy(c => c.Name))
                    {
                        categoryFilterComboBox.Items.Add(new { Id = (int?)cat.Id, Name = cat.Name });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not load categories for filter: {ex.Message}");
                }
            }

            if (currentSelection != null)
            {
                var itemToRestore = categoryFilterComboBox.Items.OfType<dynamic>().FirstOrDefault(i => i.Id == currentSelection.Id);
                if (itemToRestore != null)
                {
                    categoryFilterComboBox.SelectedItem = itemToRestore;
                }
                else
                {
                    categoryFilterComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                categoryFilterComboBox.SelectedIndex = 0;
            }

            categoryFilterComboBox.DisplayMemberPath = "Name";
            categoryFilterComboBox.SelectionChanged += CategoryFilterComboBox_SelectionChanged;

            if (categoryFilterComboBox.SelectedIndex == 0 && (currentSelection != null && currentSelection.Id != null))
            {
                await UpdateBooks();
            }
            else if (categoryFilterComboBox.SelectedItem != currentSelection)
            {
                await UpdateBooks();
            }
        }

        async Task UpdateBooks()
        {
            int? selectedCategoryId = null;
            if (categoryFilterComboBox?.SelectedItem != null)
            {
                dynamic selectedCategoryItem = categoryFilterComboBox.SelectedItem;
                if (selectedCategoryItem.Id != null)
                {
                    selectedCategoryId = selectedCategoryItem.Id;
                }
            }
            string searchTerm = searchTextBox?.Text;

            try
            {
                List<Book> books = await API.GetBooks(searchTerm, selectedCategoryId);
                booksPanel.Children.Clear();
                foreach (Book book in books)
                {
                    booksPanel.Children.Add(new BookControl(book, async () => await UpdateBooks()));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load books: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        Button NewToolbarButton(string text, RoutedEventHandler clickHandler)
        {
            Button button = new Button { Content = text, Margin = new Thickness(5), Padding = new Thickness(5) };
            button.Click += clickHandler;
            toolBar.Children.Add(button);
            return button;
        }

        async void LoginButton_Click(object s, RoutedEventArgs e)
        {
            new LoginWindow().ShowDialog();
            SetupToolbar();
            await LoadCategoryFilters(); // Explicit hívás a kategóriák frissítésére
            await UpdateBooks();      // És a könyvek frissítésére
        }

        async void LogoutButton_Click(object s, RoutedEventArgs e)
        {
            API.LogOut();
            SetupToolbar();
            await LoadCategoryFilters();
            await UpdateBooks();
        }

        async void UserInfoButton_Click(object s, RoutedEventArgs e)
        {
            try
            {
                UserInfo user = await API.GetUserInfo();
                MessageBox.Show($"Name: {user.Name}\nAccount created at {user.CreatedAt}", "User info");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        async void AddButton_Click(object s, RoutedEventArgs e)
        {
            BookEditorWindow editor = new BookEditorWindow();
            try
            {
                if (editor.ShowDialog() == true)
                {
                    await API.SendBook(editor.BookToEdit, false);
                    await UpdateBooks();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ManageCategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            CategoryWindow catWindow = new CategoryWindow();
            if (catWindow.ShowDialog() == true)
            {
                await LoadCategoryFilters();
                await UpdateBooks();
            }
        }

        private async void CategoryStatsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<CategoryStat> stats = await API.GetCategoryStats();
                string statsMessage = "Category Statistics:\n\n";
                if (stats.Any())
                {
                    foreach (var stat in stats)
                    {
                        statsMessage += $"{stat.Name}: {stat.BookCount} book(s)\n";
                    }
                }
                else
                {
                    statsMessage += "No category statistics available.";
                }
                MessageBox.Show(statsMessage, "Category Stats");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load category stats: {ex.Message}", "Error");
            }
        }
    }
}