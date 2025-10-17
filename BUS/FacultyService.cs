using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.MODELS;

namespace BUS
{
    public class FacultyService
    {
        public List<Faculty> GetAll()
        {
            StudentContextDB context = new StudentContextDB();
            return context.Faculty.ToList();
        }
    }
}
