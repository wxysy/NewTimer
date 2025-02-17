using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimerLib.MVVMViews
{
    /// <summary>
    /// CountdownSettingsView.xaml 的交互逻辑
    /// </summary>
    public partial class CountdownSettingsView : Window
    {
        public CountdownSettingsView()
        {
            InitializeComponent();
        }
        public event EventHandler<int[]>? CallBack;
        private void OnCallback(int[] p) => CallBack?.Invoke(this, p);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            int[] para = [int.Parse(tb_Countdown.Text), int.Parse(tb_Warning.Text)];
            OnCallback(para);
        }
    }
}
