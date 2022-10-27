using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Labb_3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int OPENAT = 12;
        const string BOOKINGSFILENAME = "BookingsWithSeats.txt";
        string[] tables = { "1", "2", "3", "4", "5", "6", "7", "8" };
        int[] seats = { 1, 2, 3, 4, 5 };
        string[] openingHours = new string[11];
        List<TableBooking> bookings = new();


        public MainWindow()
        {
            InitializeComponent();

            #region Initialize data
            tableBox.ItemsSource = tables;
            tableBox.SelectedIndex = 0;
            seatBox.ItemsSource = seats;
            seatBox.SelectedIndex = 0;

            for (int i = 0; i < openingHours.Length; i++)
            {
                openingHours[i] = (OPENAT + i) + ":00";
            }

            timeBox.ItemsSource = openingHours;
            timeBox.SelectedIndex = 0;

            bookings.Add(new TableBooking("2022-10-21", "14:00", "Karl", "3", 3));
            bookings.Add(new TableBooking("2022-11-22", "15:00", "Mina", "1", 4));
            bookings.Add(new TableBooking("2022-10-21", "14:00", "Bengt", "4", 2));
            bookings.Add(new TableBooking("2022-10-01", "12:00", "Sara", "8", 5));
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
                MessageBox.Show(ex.Message + "\nAvslutar applikationen", "Fel", MessageBoxButton.OK, MessageBoxImage.Stop);
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

                    MessageBox.Show("Fel vid öppnande av " + fileName + "\n\n" + ex.Message, "Fel", MessageBoxButton.OK, MessageBoxImage.Error);

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
            int addingSeats = Int32.Parse(seatBox.Text);
            try
            {
                bookingToAdd = new TableBooking(datePickerBox.Text,
                   timeBox.Text,
                   nameBox.Text,
                   tableBox.Text,
                   addingSeats);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fel", MessageBoxButton.OK, MessageBoxImage.Error);

            }

            if (bookingToAdd != null)
            {

                int seatsAtSameTime = bookings
                    .Where(booking => booking.Compare.Equals(bookingToAdd.Compare))
                    .Sum(booking => booking.Seats);
                    

                if (seatsAtSameTime + addingSeats > 5)
                    MessageBox.Show("Inte tillräckligt med platser kvar för denna tid!", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    bookings.Add(bookingToAdd);
                sortBookings();
                updateListBoxBinding();
                writeBookings();

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
                List<TableBooking> loadedBookings = new();

                foreach (string line in input)
                {
                    try
                    {
                        loadedBookings.Add(new TableBooking(line));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Fel", MessageBoxButton.OK, MessageBoxImage.Error);

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
                    throw new Exception("Bokningsfilen har för många platsbokningar vid ett klockslag!");
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

            var groupSeatingQuery = bookingList.
                Select(booking => booking)
                .GroupBy(booking => booking.Compare)
                .Where(group => group.Count() > 1)
                .Select(item => new
                {
                    Sum = item.Sum(booking => booking.Seats)
                })
                .ToList();


            if (groupSeatingQuery.Exists(seating => seating.Sum > 5))
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

        private void deleteBooking(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                removeBooking(sender,e);
            
        }
    }
}
