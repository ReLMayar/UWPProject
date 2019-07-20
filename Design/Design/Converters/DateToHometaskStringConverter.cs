using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Design.Converters
{
    // Custom class implements the IValueConverter interface.
    public class DateToHometaskStringConverter : IValueConverter
    {
        // Define the Convert method to change a DateTime object to 
        // a month string.
        public object Convert(object value, Type targetType,
            object parameter, string language)
        {
            // The value parameter is the data from the source object.
            DateTime thisdate = (DateTime)value;
            string result = "Дата сдачи: "  + thisdate.ToString("d MMMM");

            // Return the month value to pass to the target.
            return result;
        }

        // ConvertBack is not implemented for a OneWay binding.
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
