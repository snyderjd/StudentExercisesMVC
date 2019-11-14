using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models.ViewModels
{
    public class InstructorEditViewModel
    {
        public List<Cohort> Cohorts { get; set; } = new List<Cohort>();
        public Instructor Instructor { get; set; }

        public List<SelectListItem> CohortOptions
        {
            get
            {
                if (Cohorts == null) return null;

                return Cohorts.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            }
        }
    }
}
