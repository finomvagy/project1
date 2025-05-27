using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;

namespace BookCatalog
{
    public partial class BookControl : UserControl
    {
        Action UpdateBooksAction;
        Book CurrentBook;

        public BookControl(Book book, Action updateBooks)
        {
            InitializeComponent();
            CurrentBook = book;
            DataContext = CurrentBook;
            UpdateBooksAction = updateBooks;
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BookDetailsWindow detailsWindow = new BookDetailsWindow(CurrentBook);
            if (detailsWindow.ShowDialog() == true)
            {
                UpdateBooksAction?.Invoke();
            }
        }
    }
}