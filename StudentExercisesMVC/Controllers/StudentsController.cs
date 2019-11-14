using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentExercisesMVC.Models;
using StudentExercisesMVC.Models.ViewModels;

namespace StudentExercisesMVC.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: Students
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId,
                                               c.Name
                                        FROM Student s LEFT JOIN Cohort c ON s.CohortId = c.Id";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Student> students = new List<Student>();

                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }
                        };

                        students.Add(student);
                    }

                    reader.Close();
                    return View(students);

                }
            }
        }

        // GET: Students/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id as 'TheStudentId', s.FirstName, s.LastName, s.SlackHandle,                              s.CohortId, 
                                               c.Id as 'TheCohortId', c.Name as 'CohortName', 
                                            se.ExerciseId, e.Name as 'ExerciseName', e.Language
                                        FROM Student s LEFT JOIN Cohort c ON s.CohortId = c.Id
                                        LEFT JOIN StudentExercise se ON se.StudentId = s.Id
                                        LEFT JOIN Exercise e ON se.ExerciseId = e.Id
                                        WHERE s.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, Student> students = new Dictionary<int, Student>();
                    while (reader.Read())
                    {
                        int studentId = reader.GetInt32(reader.GetOrdinal("TheStudentId"));
                        if (!students.ContainsKey(studentId))
                        {
                            Student newStudent = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("TheStudentId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName"))
                                }
                            };

                            students.Add(studentId, newStudent);
                        }

                        Student fromDictionary = students[studentId];

                        if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                        {
                            Exercise exercise = new Exercise
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                Language = reader.GetString(reader.GetOrdinal("Language"))
                            };
                            fromDictionary.Exercises.Add(exercise);
                        }
                    }
                    reader.Close();
                    return View(students.Values.First());
                }
            }
        }

        // GET: Students/Create
        public ActionResult Create()
        {
            var cohorts = GetAllCohorts();
            var viewModel = new StudentCreateViewModel()
            {
                Cohorts = cohorts
            };
            return View(viewModel);
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentCreateViewModel viewModel)
        {
            try
            {
                var newStudent = viewModel.Student;
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Student(FirstName, LastName, SlackHandle, CohortId)
                                                VALUES(@firstName, @lastName, @slackHandle, @cohortId)";
                        cmd.Parameters.Add(new SqlParameter("@firstName", newStudent.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", newStudent.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", newStudent.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", newStudent.CohortId));

                        cmd.ExecuteNonQuery();
                    }
                }// TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Students/Edit/5
        public ActionResult Edit(int id)
        {
            var cohorts = GetAllCohorts();
            var exercises = GetAllExercises();

            // Create SelectListItems for all of the exercises
            var exerciseSelectItems = exercises.Select(e => new SelectListItem(e.Name, e.Id.ToString())).ToList();
            var assignedExercises = GetAssignedExercises(id);
            List<int> exerciseIds = new List<int>();

            // Mark all of the student's currently-assigned exercises as Selected
            foreach (SelectListItem e in exerciseSelectItems)
            {
                if (assignedExercises.Any(assigned => assigned.Id == int.Parse(e.Value)))
                {
                    e.Selected = true;
                    exerciseIds.Add(int.Parse(e.Value));
                }
            }

            MultiSelectList exerciseOptions = new MultiSelectList(exerciseSelectItems);

            var student = GetById(id);

            var viewModel = new StudentEditViewModel()
            {
                Cohorts = cohorts,
                Student = student,
                ExerciseOptions = exerciseSelectItems,
                Exercises = exercises,
                ExerciseIds = exerciseIds
            };

            return View(viewModel);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, StudentEditViewModel viewModel)
        {
            try
            {
                var assignedExercises = viewModel.ExerciseIds;
                var updatedStudent = viewModel.Student;
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        // Loop over the list of assigned exerciseIds and create SQL string that will assign the exercise to the student if it hasn't already been assigned
                        string sqlQuery = "DELETE FROM StudentExercise WHERE StudentId = @id;";

                        for (int i = 0; i < assignedExercises.Count(); i++)
                        {
                            sqlQuery += $@"INSERT INTO StudentExercise(StudentId, ExerciseId)
                                            VALUES(@id, @exercise{i});";
                            cmd.Parameters.Add(new SqlParameter($"@exercise{i}", assignedExercises[i]));
                        }

                        cmd.CommandText = $@"
                                        UPDATE Student
                                        SET FirstName = @firstName, LastName = @lastName, 
                                            SlackHandle = @slackHandle, CohortId = @cohortId 
                                        WHERE Id = @id;";

                        cmd.CommandText += sqlQuery;

                        cmd.Parameters.Add(new SqlParameter("@firstName", updatedStudent.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", updatedStudent.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", updatedStudent.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", updatedStudent.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@sqlQuery", sqlQuery));
                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Students/Delete/5
        public ActionResult Delete(int id)
        {
            Student student = GetById(id);
            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM StudentExercise WHERE StudentId = @id;
                                            DELETE FROM Student WHERE id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();
                    }
                }
                    return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private Student GetById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId FROM Student s WHERE s.id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Student student = null;

                    if (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };

                    }

                    reader.Close();
                    return student;
                }
            }
        }

        private List<Cohort> GetAllCohorts()
        {
            List<Cohort> cohorts = new List<Cohort>();

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id, c.Name FROM Cohort c";
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Cohort cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };

                        cohorts.Add(cohort);
                    }

                    reader.Close();
                    return cohorts;
                }
            }
        }

        private List<Exercise> GetAllExercises()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, Language FROM Exercise";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Exercise> exercises = new List<Exercise>();
                    Exercise exercise = null;

                    while (reader.Read())
                    {
                        exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        };

                        exercises.Add(exercise);
                    }
                    reader.Close();

                    return exercises;
                }
            }
        }

        private List<Exercise> GetAssignedExercises(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id, e.Name, e.Language 
                                        FROM Exercise e LEFT JOIN StudentExercise se on se.ExerciseId = e.Id
                                        WHERE se.StudentId = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Exercise> exercises = new List<Exercise>();
                    Exercise exercise = null;

                    while (reader.Read())
                    {
                        exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        };

                        exercises.Add(exercise);
                    }
                    reader.Close();

                    return exercises;
                }
            }
        }
    }
}