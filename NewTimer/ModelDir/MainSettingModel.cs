using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NewTimer.ModelDir
{
    record MainSettingModel
    {
        public int CountDownSeconds { get; set; } = 12;
        public Brush CountDownColor { get; set; } = Brushes.Blue;
        public int WarningSeconds { get; set; } = 6;
        public Brush WarningColor { get; set; } = Brushes.Red;
        public int TimerInterval { get; set; } = 1;
        public bool IsUIControlActived { get; set; } = false;
        public bool IsZeroEventActived { get; set; } = true;
    }
}
