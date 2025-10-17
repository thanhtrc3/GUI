using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.MODELS;

namespace BUS
{
    public class MajorService
    {
        public List<Major> GetAllByFaculty(int facultyID)
        {
            StudentContextDB context = new StudentContextDB();
            return context.Major.Where(p => p.FacultyID == facultyID).ToList();
        }
    }
}
