using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    internal class DB_NowProfile:NowProfile
    {
        internal DB_NowProfile(DateTimeOffset preferredTimeData, bool InitializedData)
            : base(preferredTimeData, InitializedData)
        {
        
        }
    }
}