using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimerLib;
using TimerLib.Functions;
using TimerLib.MVVMViews;

namespace NewTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            timer = TimerStarter.CreatCountDownTimer(12, Brushes.Blue, 5, Brushes.Red, 1, false, CountDown_ZeroEvent, TimerClose_Event, TimerTick_Event);
        }

        CountDown timer;
        private void CountDown_ZeroEvent(object? sender, EventArgs e)
        {
            MessageBox.Show("0时刻事件引发");
        }
        private void TimerClose_Event(object? sender, int e)
        {
            if (e == 0) //0时刻时直接执行0时刻事件
                return;
            MessageBox.Show($"剩余时间：{e}s");
        }
        private void TimerTick_Event(object? sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            timer.StartOrStop();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var state = !(timer.IsUIOperatesAllowed);
            timer.AllowUIOperations(state);
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            timer.Close();
        }
    }
}