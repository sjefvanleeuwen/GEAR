﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ST.Calendar.Abstractions.Enums;
using ST.Core.Abstractions;
using ST.Identity.Abstractions;
using ST.Identity.Models.UserViewModels;

namespace ST.Calendar.Abstractions.Models.ViewModels
{
    public class GetEventViewModel : BaseEventViewModel, IBaseModel
    {
        /// <summary>
        /// Event duration
        /// </summary>
        public virtual EventDuration Duration
        {
            get
            {
                var time = EndDate - StartDate;
                return new EventDuration(time.Days, time.Hours, time.Minutes, time.Seconds);
            }
        }

        /// <summary>
        /// Organizer info
        /// </summary>
        public virtual CalendarUserViewModel OrganizerInfo { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Ignore members
        /// </summary>
        [JsonIgnore]
        public override ICollection<Guid> Members { get; set; }

        /// <summary>
        /// Invited users
        /// </summary>
        public virtual ICollection<CalendarUserViewModel> InvitedUsers { get; set; } = new List<CalendarUserViewModel>();
        public Guid Id { get; set; }
        public string Author { get; set; }
        public DateTime Created { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime Changed { get; set; }
        [JsonIgnore]
        public bool IsDeleted { get; set; }
        [JsonIgnore]
        public int Version { get; set; }
        [JsonIgnore]
        public Guid? TenantId { get; set; }
    }

    public sealed class CalendarUserViewModel : SampleGetUserViewModel
    {
        public CalendarUserViewModel(ApplicationUser user) : base(user) { }
        public EventAcceptance Acceptance { get; set; } = EventAcceptance.Tentative;
    }
}
