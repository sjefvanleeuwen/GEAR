﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ST.Identity.Abstractions.Models.MultiTenants;

namespace ST.Identity.Razor.Users.ViewModels.UserViewModels
{
    public class UserProfileViewModel
    {
        public UserProfileViewModel()
        {
            Roles = new HashSet<string>();
            Groups = new HashSet<string>();
            Tenant = new Tenant();
        }

        public Guid UserId { get; set; }


        [Display(Name = "User Name", Description = "user name", Prompt = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Email Address", Description = "email", Prompt = "email")]
        public string Email { get; set; }

        [MaxLength(50)]
        [Display(Name = "First name", Description = "first name", Prompt = "ex: John")]
        public string UserFirstName { get; set; }

        [MaxLength(50)]
        [Display(Name = "Last name", Description = "last name", Prompt = "ex: Doe")]
        public string UserLastName { get; set; }

        [MaxLength(20)]
        [Display(Name = "Phone number", Description = "phone number ", Prompt = "0123456789")]
        public string UserPhoneNumber { get; set; }

        [Display(Name = "User status", Description = "user status")]
        public bool IsDisabled { get; set; }

        [Display(Name = "Birthday", Description = "birthday")]
        public DateTime Birthday { get; set; }

        [MaxLength(500)]
        [Display(Name = "About me", Description = "same description")]
        public string AboutMe { get; set; }

        public Tenant Tenant { get; set; }

        public IEnumerable<string> Roles { get; set; }
        public IEnumerable<string> Groups { get; set; }
    }
}