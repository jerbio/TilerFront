using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class IndexedThirdPartyAuthentication:ThirdPartyCalendarAuthenticationModel
    {
        public uint ThirdPartyIndex { set; get; }
        
        IndexedThirdPartyAuthentication()
        { }
        public IndexedThirdPartyAuthentication(ThirdPartyCalendarAuthenticationModel AuthenticationData, uint OrderIndex) 
        {
            ThirdPartyIndex = OrderIndex;
            ID =AuthenticationData.ID;
            TilerID =AuthenticationData.TilerID;
            isLongLived =AuthenticationData.isLongLived;
            Email =AuthenticationData.Email;
            Token =AuthenticationData.Token;
            RefreshToken =AuthenticationData.RefreshToken;
            ProviderID =AuthenticationData.ProviderID;
            Deadline = AuthenticationData.Deadline;
        }
    }
}