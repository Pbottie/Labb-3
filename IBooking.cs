using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb_3
{
    internal interface IBooking
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string Name { get; set; }
        
    }
}
