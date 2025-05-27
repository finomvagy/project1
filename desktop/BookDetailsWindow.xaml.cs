using System;
using System.Windows;

namespace BookCatalog
{
    public partial class BookDetailsWindow : Window
    {
        private Book CurrentBook;

        public BookDetailsWindow(Book book)
        {
            InitializeComponent();
            CurrentBook = book;
            DataContext = CurrentBook;
            if (API.LoggedIn) actionsPanel.Visibility = Visibility.Visible;
        }

        private async void editButton_Click(object sender, RoutedEventArgs e)
        {
            BookEditorWindow editor = new BookEditorWindow(CurrentBook);
            try
            {
                if (editor.ShowDialog() == true)
                {
                    await API.SendBook(editor.BookToEdit, true);
                    DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this book?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    await API.DeleteBook(CurrentBook.Id);
                    DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}