using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DBTilerElement;
using System.Threading.Tasks;

namespace TilerFront
{
    public class LocationControl
    {
        public virtual async Task<IEnumerable<DB_LocationElements>> getCachedLocationByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}