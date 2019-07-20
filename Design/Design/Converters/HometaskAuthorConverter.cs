using Project.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Design.Converters
{
    public class HometaskAuthorConverter : IValueConverter
    {
    
        public object Convert(object value, Type targetType,
            object parameter, string language)
        {

            User user = (User)value;
            string result = "Автор: " + user.Name + " " + user.SurName;

        
            return result;
        }

        
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
