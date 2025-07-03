using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class BusinessTrip
    {
        private readonly EapDbContext _eapDbContext;
        public BusinessTrip(EapDbContext eapDbContext) {
            _eapDbContext = eapDbContext ?? throw new ArgumentNullException(nameof(eapDbContext));
        }
    }
}
