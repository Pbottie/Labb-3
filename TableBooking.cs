using System;
using System.Text.RegularExpressions;

namespace Labb_3
{
    internal class TableBooking : IBooking
    {
        public int Seats { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Name { get; set; }
        public string Table { get; set; }

        public string Compare { get { return Date + Time + Table; } }
        /// <summary>
        /// Creates a new TableBooking
        /// </summary>
        /// <param name="date">Date format YYYY-MM-DD</param>
        /// <param name="time">Time format HH:MM 24h</param>
        /// <param name="name">Name of Booker</param>
        /// <param name="table">Table Number</param>
        public TableBooking(string date, string time, string name, string table, int seats)
        {
            if (date == "" || name.Trim() == "")
            {
                throw new Exception("Välj ett datum och namn!");
            }
            this.Date = date;
            this.Time = time;
            this.Name = name.Trim();
            this.Table = table;
            this.Seats = seats;
        }

        public TableBooking(string line)
        {
            if (IsValidBooking(line))
            {
                string[] args = line.Split(' ');
                this.Date = args[0];
                this.Time = args[1];
                this.Name = args[2].Trim();
                this.Table = args[3];
                this.Seats = Int32.Parse(args[5]);

            }
            else
            {
                throw new Exception(line + " är inte ett giltigt bokningsformat!\n" + "Använd formatet:\nYYYY-MM-DD HH:MM Namn Bordsnummer Platser: Antal");
            }
        }
        public static bool IsValidBooking(string input)
        {
            Regex regex = new Regex(@"^2[0-9][0-9][0-9]-([0][1-9]|[1][0-2])-([0][1-9]|[1-2][0-9]|[3][0-1])\s+([0-1][0-9]|[2][0-4]):00\s+[a-zA-Z]+\s+\d\s+Platser:\s+\d+$");
            return regex.IsMatch(input);
        }
        public override string ToString()
        {
            return $"{Date} {Time} {Name} {Table} Platser: {Seats}";
        }


    }
}
