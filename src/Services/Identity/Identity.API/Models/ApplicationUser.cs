using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryBuddy.Services.Identity.API.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage ="First Name is required")]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is required")]
        [MaxLength(50)]
        public string LastName { get; set; }
        public string LibraryCardId { get; set; }
        [Required(ErrorMessage ="DOB is required")]
        public string DOB { get; set; }
        public virtual Address StreetAdress { get; set; } //Here we are using OwnedEntity type https://docs.microsoft.com/en-au/ef/core/modeling/owned-entities
    }
}
