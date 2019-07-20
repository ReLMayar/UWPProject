using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.DB
{
    public class Pair
    {
        public int Num { get; set; }
        public string State { get; set; }
        public string Lesson { get; set; }
        public string Teacher { get; set; }
        public string Cabinet { get; set; }
    }

    public class Day
    {
        public List<Pair> Pairs { get; set; }
        public string Name { get; set; }
    }

    public class SheduleGroup
    {
        public List<Day> Days { get; set; }
        public string Name { get; set; }
    }

    public class Shedule
    {
        public int SheduleId { get; set; }
        public DateTime Date { get; set; }
        public bool Type { get; set; }

        public List<SheduleGroup> SheduleGroups { get; set; }
    }
}
