using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ch.Hurni.AP_MaJ.Converters
{
    public class PropertyTypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string TypeLongName = (string)value;

            if (!string.IsNullOrEmpty(TypeLongName) && TypeLongName.Contains("."))
            {
                return TypeLongName.Split('.').LastOrDefault();
            }

            return TypeLongName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
