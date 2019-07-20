using System;
using System.Collections.Generic;

namespace Project.DB.Models
{
    public class ChangeList
    {
        public int ChangeListId { get; set; }
        public DateTime DateUpload { get; set; }
        public DateTime DateOfChanges { get; set; }
        public List<Change> Changes { get; set; }
    }

    public class Change
    {
        public int Num { get; set; }
        public string GroupName { get; set; }
        public string Teacher { get; set; }
        public string Cabinet { get; set; }
        public string Lesson { get; set; }
    }
}
