using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models
{
    public class Student : NSSPerson
    {
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
        public int Id { get; set; }

        public int CohortId { get; set; }
    }
}
