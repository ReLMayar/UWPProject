using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.DB.Models
{
    public class VkProfile
    {
        public string Login { get; set; }
        public long ProfileId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Key { get; set; }
    }
}
