using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.DB.Models
{
    public class NotificationKey
    {
        public int NotificationKeyId { get; set; }
        public string Login { get; set; }
        public string Key { get; set; }
        public string Platform { get; set; }
    }
}
