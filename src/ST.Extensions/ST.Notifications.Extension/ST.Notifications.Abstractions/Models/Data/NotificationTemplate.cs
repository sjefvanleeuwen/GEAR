﻿using System.ComponentModel.DataAnnotations;
using ST.Core;

namespace ST.Notifications.Abstractions.Models.Data
{
    public class NotificationTemplate : BaseModel
    {
        /// <summary>
        /// Event id
        /// </summary>
        [Required]
        public string NotificationEventId { get; set; }

        /// <summary>
        /// Event reference
        /// </summary>
        public NotificationEvent NotificationEvent { get; set; }

        /// <summary>
        /// Subject
        /// </summary>
        [Required]
        public string Subject { get; set; }

        /// <summary>
        /// Html template
        /// </summary>
        [Required]
        public string Value { get; set; }
    }
}
