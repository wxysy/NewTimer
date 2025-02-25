using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NewTimer.FunctionDir.BindingDir
{
    class DGRowNoteCov : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path)
            {
                var ext = Path.GetExtension(path);
                string[] extensionList = [".pptx", ".ppt"];
                if (extensionList.Contains(ext))
                    return string.Empty;
                else
                    return "非PPT文件";
            }                
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
