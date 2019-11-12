namespace StudentExercisesMVC.Models
{
    public class Instructor : NSSPerson
    {
        // Properties
        public string Specialty { get; set; }
        public int Id { get; set; }
        public int CohortId { get; set; }

        // Constructor
        public Instructor(int id, string firstName, string lastName, string slackHandle, Cohort cohort, string specialty)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            SlackHandle = slackHandle;
            Specialty = specialty;
            Cohort = cohort;
        }

        public Instructor()
        {

        }

        // Method to assign an exercise to a student
        public void AssignExercise(Exercise exercise, Student student)
        {
            student.Exercises.Add(exercise);
        }

    }
}