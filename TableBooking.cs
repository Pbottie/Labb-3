using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb_3
{
    internal class TableBooking : IBooking
    {
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
        public TableBooking(string date, string time, string name, string table)
        {
            if (date == "" || name.Trim() == "")
            {
                throw new Exception("Välj ett datum och namn!");
            }
            this.Date = date;
            this.Time = time;
            this.Name = name.Trim();
            this.Table = table;
        }
        public override string ToString()
        {
            return $"{Date} {Time} {Name} {Table}";
        }


    }
}
