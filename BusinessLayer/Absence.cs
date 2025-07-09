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
        public AbsenceType Type { get; set; }
        public int DaysCount { get; set; }
        public DateTime Created { get; set; }
        public AbsenceStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public string UserId { get; set; }

        public string? UserName { get; set; }
    }
}
