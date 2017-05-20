using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BiSComparer.Converters
{
	public class BooleanToObtainedColourConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((bool)value)
			{
				{
					return Colors.Green.ToString();
				}
			}
			return Colors.Gray.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}