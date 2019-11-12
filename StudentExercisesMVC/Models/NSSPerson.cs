
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace StudentExercisesMVC.Models
{
    public class NSSPerson
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Slack")]
        [StringLength(24, MinimumLength = 3)]
        public string SlackHandle { get; set; }
        [Display(Name = "Cohort")]
        public Cohort Cohort { get; set; }
    }
}