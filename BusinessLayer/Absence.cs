using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class Absence
    {
        public int Id { get; set; }
        public byte Type { get; set; }
        public int DaysCount { get; set; }
        public DateTime Created { get; set; }
        public byte Status { get; set; }
        public DateTime StartDate { get; set; }

        public User User { get; set; }
    }

}
