using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront.Models
{
	public class LocationModel : AuthorizedUser
	{
        public string Id { get; set; }
        public string ThirdPartyId { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public bool IsVerified { get; set; }

        virtual public TilerElements.Location toLocation()
        {
            Location retValue = new Location()
            {
                Longitude = this.Longitude,
                Latitude = this.Latitude,
                Address = this.Address,
                Description = this.Description,
                IsVerified = this.IsVerified,
                UserId = this.UserID
            };

            if(this.ThirdPartyId.isNot_NullEmptyOrWhiteSpace())
            {
                retValue.setThirdPartyId(this.ThirdPartyId);
            }
            
            return retValue;
        }
    }
}