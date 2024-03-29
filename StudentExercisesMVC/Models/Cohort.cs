﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models
{
    public class Cohort
    {
        [Required]
        [StringLength(11, MinimumLength = 5)]
        public string Name { get; set; }
        public int Id { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Instructor> Instructors { get; set; } = new List<Instructor>();

        public Cohort(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public Cohort()
        {

        }
    }
}
