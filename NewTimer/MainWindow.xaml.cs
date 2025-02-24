using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PPTLib;
using PPTLib.Functions;
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
            progress = new Progress<string>(p => tb_Show.Text += $"{p}\n");
            timer = TimerStarter.CreatCountDownTimer(12, Brushes.Blue, 5, Brushes.Red, 1, false, CountDown_ZeroEvent, TimerClose_Event, TimerTick_Event);
            pptPlay = new();
            pptPlay.PPTShowBegin += (sender, e) => progress.Report("PPT打开");
            pptPlay.PPTShowEnd += (sender, e) => progress.Report("PPT关闭");

            pptCD = new(progress);
        }
        IProgress<string> progress;


        #region Timer测试
        CountDown timer;
        private void CountDown_ZeroEvent(object? sender, EventArgs e)
        {
            progress.Report("0时刻事件引发");
        }
        private void TimerClose_Event(object? sender, int e)
        {
            if (e == 0) //0时刻时直接执行0时刻事件
                return;
            progress.Report($"剩余时间：{e}s");
        }
        private void TimerTick_Event(object? sender, int e)
        {
            progress.Report($"触发：Tick");
        }

        private void Button_StartStop_Click(object sender, RoutedEventArgs e)
        {
            timer.StartOrStop();
        }

        private void Button_UI_Click(object sender, RoutedEventArgs e)
        {
            var state = !(timer.IsUIOperatesAllowed);
            timer.AllowUIOperations(state);
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            timer.Close();
        }
        #endregion

        #region PPT测试
        string filePath = @"D:\ProgrammingProjects\2-Testing\test.pptx";
        PPTPlay pptPlay;
        private async void Btn_Open_Click(object sender, RoutedEventArgs e)
        {
            //pptPlay.PPTOpen(filePath);
            //await Task.Delay(15000);
            //pptPlay.PPTClose();

            pptCD.PPTOpen(filePath);
        }
        private void Btn_ClosePPT_Click(object sender, RoutedEventArgs e)
        {
            //pptPlay?.PPTClose();

            pptCD.PPTClose();
        }
        #endregion

        #region 组合测试
        PPTCountDown pptCD;
        #endregion
    }
}