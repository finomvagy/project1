using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BookCatalog
{
    public class CategoryViewModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }

        private bool _isSelectedForBook;
        public bool IsSelectedForBook
        {
            get { return _isSelectedForBook; }
            set
            {
                if (_isSelectedForBook != value)
                {
                    _isSelectedForBook = value;
                    OnPropertyChanged(nameof(IsSelectedForBook));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class BookEditorWindow : Window
    {
        public Book BookToEdit { get; private set; }
        private ObservableCollection<CategoryViewModel> AllCategoriesVM { get; set; } = new ObservableCollection<CategoryViewModel>();
        // private bool IsEditMode; // Ezt a property-t nem használjuk végül, a konstruktor típusa határozza meg

        public BookEditorWindow()
        {
            InitializeComponent();
            // IsEditMode = false;
            BookToEdit = new Book();
            DataContext = BookToEdit;
            saveButton.Content = "Add";
            Title = "Add new book";
        }

        public BookEditorWindow(Book book)
        {
            InitializeComponent();
            // IsEditMode = true;
            BookToEdit = new Book // Létrehozunk egy másolatot, vagy használjuk az eredetit, attól függően, hogy a cancel mit csináljon
            {
                Id = book.Id,
                Author = book.Author,
                Title = book.Title,
                Details = book.Details,
                Categories = new List<Category>(book.Categories ?? new List<Category>())
            };
            DataContext = BookToEdit; // Fontos, hogy a DataContext a BookToEdit legyen
            saveButton.Content = "Save";
            Title = "Edit book";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCategoriesAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var availableCategories = await API.GetCategories();
                AllCategoriesVM.Clear();

                List<int> bookCategoryIds = BookToEdit.Categories?.Select(c => c.Id).ToList() ?? new List<int>();

                foreach (var cat in availableCategories.OrderBy(c => c.Name))
                {
                    AllCategoriesVM.Add(new CategoryViewModel
                    {
                        Id = cat.Id,
                        Name = cat.Name,
                        IsSelectedForBook = bookCategoryIds.Contains(cat.Id)
                    });
                }
                categoriesListBox.ItemsSource = AllCategoriesVM;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            // Az Author, Title, Details már frissültek a DataBinding miatt
            // A BookToEdit.Categories property-t kell frissíteni a ViewModel alapján
            BookToEdit.Categories = AllCategoriesVM
                                     .Where(vm => vm.IsSelectedForBook)
                                     .Select(vm => new Category { Id = vm.Id, Name = vm.Name }) // Itt vissza kell alakítani Category objektumokká
                                     .ToList();
            DialogResult = true;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}