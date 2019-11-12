using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models
{
    public class Exercise
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Language { get; set; }

        public List<Student> Students { get; set; } = new List<Student>();

        public Exercise(int id, string name, string language)
        {
            Id = id;
            Name = name;
            Language = language;
        }

        public Exercise()
        {

        }
    }
}
