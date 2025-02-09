﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace FYP_WebApp.Models
{
    public class Team
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Team Name")]
        public string Name { get; set; }
        [Required]
        public string ManagerId { get; set; }
        public ApplicationUser Manager { get; set; }
        public List<ApplicationUser> TeamMembers { get; set; }
        [Display(Name = "Inactivated?")]
        public bool IsInactive { get; set; }
    }
}