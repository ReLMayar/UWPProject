using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Project.DB.Models
{
    public class news
    {
        public string Id { get; set; }
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPictureUrl { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public List<Photos> Photos { get; set; }
    }

    public class Photos
    {
        public string PhotoUrl { get; set; }
    }
}
