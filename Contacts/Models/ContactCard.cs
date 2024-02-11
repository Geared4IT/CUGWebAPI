using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Contacts.Models
{
    public class ContactCard
    {
        public int Id { get; set; }

        [DisplayName("First Name")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        [StringLength(50)]
        public string LastName { get; set; }

        [DisplayName("Middle Initial")]
        [StringLength(1)]
        public string MiddleInitial { get; set; }

        [DisplayName("Email")]
        [Required(ErrorMessage = "The email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [DisplayName("Phone")]
        [Required(ErrorMessage = "The Phone is required")]
        public string Phone { get; set; }

        [DisplayName("Address")]
        [Required(ErrorMessage = "The Address is required")]
        public string Address1 { get; set; }

        [DisplayName("Address 2")]
        public string Address2 { get; set; }

        [DisplayName("City")]
        [Required(ErrorMessage = "The City is required")]
        public string City { get; set; }

        [DisplayName("State")]
        [Required(ErrorMessage = "The State is required")]
        [StringLength(2)]
        public string State { get; set; }

        [DisplayName("Zip Code")]
        [Required(ErrorMessage = "The Zip Code is Required")]
        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid Zip")]
        public string Zipcode { get; set; }
    }
}