using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using DAL.MODELS;

namespace BUS
{
    public class StudentService
    {
        public List<Student> GetAll()
        {
            using (var context = new StudentContextDB())
            {
                return context.Student
                    .Include(s => s.Faculty)
                    .Include(s => s.Major)
                    .ToList();
            }
        }

        public List<Student> GetAllHasNoMajor()
        {
            using (var context = new StudentContextDB())
            {
                return context.Student
                    .Where(p => p.MajorID == null)
                    .Include(s => s.Faculty)
                    .ToList();
            }
        }

        public List<Student> GetAllHasNoMajor(int facultyID)
        {
            using (var context = new StudentContextDB())
            {
                return context.Student
                    .Where(p => p.MajorID == null && p.FacultyID == facultyID)
                    .Include(s => s.Faculty)
                    .ToList();
            }
        }

        public Student FindById(string studentId)
        {
            using (var context = new StudentContextDB())
            {
                return context.Student
                    .Include(s => s.Faculty)
                    .Include(s => s.Major)
                    .FirstOrDefault(p => p.StudentID == studentId);
            }
        }

        public void InsertUpdate(Student s)
        {
            using (var context = new StudentContextDB())
            {
                context.Student.AddOrUpdate(s);
                context.SaveChanges();
            }
        }

        public void Delete(string studentId)
        {
            using (var context = new StudentContextDB())
            {
                var sv = context.Student.FirstOrDefault(p => p.StudentID == studentId);
                if (sv != null)
                {
                    context.Student.Remove(sv);
                    context.SaveChanges();
                }
            }
        }
    }
}