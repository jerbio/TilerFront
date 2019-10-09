using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront.Models
{
    public class ThirdPartyCalendarSubGroup
    {
        public string Id { get; set; }
        public ThirdPartyCalendarAuthenticationModel ThirdPartyCalendarAuthentication { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string Description { get; set; }
        public string ThirdPartyId { get; set; }
        public string LocationId { get; set; }
        [ForeignKey("LocationId")]
        public Location DefaultLocation { get; set; }

        public virtual ThirdPartyCalendarGroupForView getThirdPartyOut()
        {
            ThirdPartyCalendarGroupForView retValue = new ThirdPartyCalendarGroupForView
            {
                Description = this.Description,
                ID = this.Id,
                Active = this.IsActive,
                Address = this.DefaultLocation?.Address,
                AddressNickName = this.DefaultLocation?.Description,
            };

            return retValue;
        }
    }
}