using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BL.Models
{
    public class ModelState
    {
        public List<string> __invalid_name__ { get; set; }
    }

    public class ServerError
    {
        public string Message { get; set; }
        public ModelState ModelState { get; set; }
    }
}
