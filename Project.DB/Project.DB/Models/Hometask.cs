using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.DB.Models
{
    public class Hometask
    {
        public int HometaskId { get; set; }
        public string Lesson { get; set; }
        public DateTime DateRecord { get; set; }
        public DateTime DateLesson { get; set; }
        public string Value { get; set; }
        public User User { get; set; }
    }
}
