using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Labb_3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int OPENAT = 12;
        const string BOOKINGSFILENAME = "Bookings.txt";
        string[] tables = { "1", "2", "3", "4", "5", "6", "7", "8" };
        string[] openingHours = new string[11];
        List<TableBooking> bookings = new();


        public MainWindow()
        {
            InitializeComponent();

            #region Initialize data
            tableBox.ItemsSource = tables;
            tableBox.SelectedIndex = 0;

            for (int i = 0; i < openingHours.Length; i++)
            {
                openingHours[i] = (OPENAT + i) + ":00";
            }

            timeBox.ItemsSource = openingHours;
            timeBox.SelectedIndex = 0;

            bookings.Add(new TableBooking("2022-10-21", "14:00", "Karl", "3"));
            bookings.Add(new TableBooking("2022-11-22", "15:00", "Mina", "1"));
            bookings.Add(new TableBooking("2022-10-21", "14:00", "Bengt", "4"));
            bookings.Add(new TableBooking("2022-10-01", "12:00", "Sara", "8"));
            sortBookings();
            listBoxBookings.ItemsSource = bookings;

            try
            {
                if (File.Exists(BOOKINGSFILENAME))
                    loadBookings(BOOKINGSFILENAME);
                else
                    writeBookings();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\nAvslutar", "Fel", MessageBoxButton.OK, MessageBoxImage.Stop);
                System.Windows.Application.Current.Shutdown();
            }

            #endregion



        }
       
        void sortBookings()
        {
            bookings = bookings.OrderBy(booking => booking.Date).
                ThenBy(booking => booking.Time).
                ThenBy(booking => booking.Table).
                ToList();

        }
        /// <summary>
        /// Writes booking list to Bookings.txt file
        /// </summary>
        void writeBookings()
        {
            try
            {
                string text = "";
                foreach (TableBooking booking in bookings)
                {
                    text += booking.ToString() + "\n";
                }
                File.WriteAllText(BOOKINGSFILENAME, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kunde inte skriva till bokningfil: " + ex.Message);
            }


        }
        /// <summary>
        /// Opens dialog for txt files and let's user select a Booking file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void openBookings(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = BOOKINGSFILENAME;
            dlg.Filter = "Text documents (.txt)|*.txt";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string fileName = dlg.FileName;
                try
                {
                loadBookings(fileName);

                }
                catch (Exception ex)
                {

                    MessageBox.Show("Fel vid öppnande av " + fileName);

                }
            }


        }
        /// <summary>
        /// Removes selected booking from list and writes to file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void removeBooking(object sender, RoutedEventArgs e)
        {

            if (listBoxBookings.SelectedItem != null)
            {

                int index = bookings.FindIndex(booking => booking.ToString() == listBoxBookings.SelectedValue.ToString());
                bookings.RemoveAt(index);
                updateListBoxBinding();
                writeBookings();

            }
        }
        /// <summary>
        /// Books table if possible and adds to list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bookTable(object sender, RoutedEventArgs e)
        {
            TableBooking bookingToAdd = null;
            try
            {
                bookingToAdd = new TableBooking(datePickerBox.Text,
                   timeBox.Text,
                   nameBox.Text,
                   tableBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fel", MessageBoxButton.OK, MessageBoxImage.Error);

            }

            if (bookingToAdd != null)
            {
                if (bookings.Exists(booking => booking.Compare == bookingToAdd.Compare))
                    MessageBox.Show("Bordet är redan bokat vid denna tid!", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    int bookingsAtSameTime = bookings.
                        Select(booking => booking).
                        Where(booking => booking.Date == bookingToAdd.Date && booking.Time == bookingToAdd.Time).
                        Count();

                    if (bookingsAtSameTime > 4)
                        MessageBox.Show("För många bokningar denna tid!", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        bookings.Add(bookingToAdd);
                    sortBookings();
                    updateListBoxBinding();
                    writeBookings();
                }
            }


        }
        /// <summary>
        /// Loads a textfile into bookings list, but Bookings.txt is always the save location
        /// </summary>
        /// <param name="fileName"></param>
        void loadBookings(string fileName)
        {
            string[] input = null;

            try
            {
                input = File.ReadAllLines(fileName);
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Kunde inte läsa filen \"{fileName}\": " + ex.Message);
            }

            if (input != null)
            {
                Regex regex = new Regex(@"^2[0-9][0-9][0-9]-([0][1-9]|[1][0-2])-([0][1-9]|[1-2][0-9]|[3][0-1])\s+([0-1][0-9]|[2][0-4]):00\s+[a-zA-Z]+\s+\d$");
                List<TableBooking> loadedBookings = new();

                foreach (string line in input)
                {
                    if (regex.IsMatch(line))
                    {
                        string[] args = line.Split(' ');
                        loadedBookings.Add(new TableBooking(args[0], args[1], args[2], args[3]));
                    }
                    else
                    {
                        MessageBox.Show(line + " är inte en giltig bokning.\n" + "Använd formatet: YYYY-MM-DD HH:MM Namn Bordsnummer");
                        throw new Exception("Bokningsfilen innehåller fel!");
                    }
                }

                if (isValidBookingList(loadedBookings))
                {
                    bookings = loadedBookings;
                    sortBookings();
                    updateListBoxBinding();

                }
                else
                {
                    MessageBox.Show(fileName + " innehåller fel!");
                    throw new Exception("Bokningsfilen har dubletter eller för många bokningar vid ett klockslag!");
                }


            }

        }
        /// <summary>
        /// Checks if loaded booking file has duplicate bookings or more than 5 bookings per timeslot
        /// </summary>
        /// <param name="bookingList"></param>
        /// <returns></returns>
        bool isValidBookingList(List<TableBooking> bookingList)
        {

            var dateTimeQuery = bookingList.
                Select(booking => booking.Date + booking.Time).
                GroupBy(dateTime => dateTime).
                Select(group => group.Count()).
                ToList();

            var checkDuplicateQuery = bookingList.
                GroupBy(booking => booking.Compare).
                Where(group => group.Count() > 1);

            if (dateTimeQuery.Exists(count => count > 5) || checkDuplicateQuery.Count() > 0)
            {
                return false;
            }

            return true;

        }
        void updateListBoxBinding()
        {
            listBoxBookings.ItemsSource = null;
            listBoxBookings.ItemsSource = bookings;

        }

    }
}
