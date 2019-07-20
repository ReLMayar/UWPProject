using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BL.Helpers
{
    public static class SheduleTypeGetter
    {
        public static bool GetSheduleType()
        {
            int _dayOfWeek = Convert.ToInt32(DateTime.UtcNow.AddHours(9).DayOfWeek);

            bool _sheduleType = DateTime.UtcNow.AddHours(9).GetWeekNumber() % 2 == 0 ? false : true;

            // Поправка на воскресенье.
            _sheduleType = _dayOfWeek == 0 ? _sheduleType == false ? true : true : _sheduleType;

            return _sheduleType;
        }
    }
}
