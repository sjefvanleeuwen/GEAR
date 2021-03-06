﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GR.Core.Attributes;
using GR.Identity.Abstractions.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GR.Identity.Profile.Abstractions.ViewModels.UserProfileViewModels
{
    public class AddUserProfileAddressViewModel
    {
        [StringLength(450)]
        [Required]
        [Display(Name = "Contact Name")]
        [DisplayTranslate(Key = IdentityResources.Translations.CONTACT_NAME)]
        public string ContactName { get; set; }

        [StringLength(450)]
        [Display(Name = nameof(Phone), Prompt = "0123456789")]
        [DisplayTranslate(Key = IdentityResources.Translations.PHONE)]
        public string Phone { get; set; }

        [StringLength(450)]
        [Display(Name = "Address Line 1")]
        [DisplayTranslate(Key = IdentityResources.Translations.ADDRESS_LINE1)]
        public string AddressLine1 { get; set; }

        [StringLength(450)]
        [Display(Name = "Address Line 2")]
        [DisplayTranslate(Key = IdentityResources.Translations.ADRESS_LINE2)]
        public string AddressLine2 { get; set; }

        [StringLength(450)]
        [Required]
        [Display(Name = "Zip Code", Prompt = "zip code")]
        [DisplayTranslate(Key = IdentityResources.Translations.ZIP_CODE)]
        public string ZipCode { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [DisplayTranslate(Key = IdentityResources.Translations.CITY)]
        public Guid CityId { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(450)]
        [DisplayTranslate(Key = IdentityResources.Translations.COUNTRY)]
        public string CountryId { get; set; }

        [Required]
        [DisplayTranslate(Key = IdentityResources.Translations.IS_DEFAULT)]
        public bool IsDefault { get; set; }

        public IEnumerable<SelectListItem> CountrySelectListItems { get; set; } = new List<SelectListItem>();

        public string Region { get; set; }
    }
}