﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FYP_WebApp.Models
{
    public class Note
    {
        public int Id { get; set; }
        [Required]
        public string Content { get; set; }
        public int StoredLocationId { get; set; }
        public StoredLocation StoredLocation { get; set; }
        public string SenderId { get; set; }
        public ApplicationUser Sender { get; set; }
        public DateTime TimeCreated { get; set; }
        public bool IsInactive { get; set; }
    }
}