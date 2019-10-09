using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace TilerFront.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
        public string UserName { get; set; }
        public string Id { get; set; }
        public string FullName { get; set; }
    }

    public class ThirdPartyAuthenticationForView
    {
        [Display(Name = "Calendar Provider")]
        public string ProviderName { get; set; }
        [Display(Name = "Associated Email")]
        public string Email { get; set; }

        [Display(Name = "CalendarID")]
        public string ID { get; set; }
        [Display(Name = "Calendars")]
        public ICollection<ThirdPartyCalendarGroupForView> CalendarGroups { get; set; }
    }

    public class ThirdPartyCalendarGroupForView
    {
        public string ID { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Enabled")]
        public bool Active { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Address Name")]
        public string AddressNickName { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangeStartOfDayModel
    {
        [Required]
        [Display(Name = "Time Of Day")]
        [StringLength(7, ErrorMessage = "In valid time provided", MinimumLength = 6)]
        public string TimeOfDay { get; set; }
        [Display(Name = "TimeZoneOffSet")]
        public string TimeZoneOffSet { get; set; }
        [Display(Name = "TimeZone")]
        public string TimeZone { get; set; } = "UTC";
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }
}