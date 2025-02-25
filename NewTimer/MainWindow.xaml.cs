using System.Collections.ObjectModel;
using System.IO;
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
using Infrastructure.Files.FileCommon;
using NewTimer.FunctionDir;
using PPTLib.Functions;
using ScoreCaculatorLib;
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
        #region 属性和字段
        IProgress<string> progress;
        PPTCountDown? pptCD;
        CountDownTimer? separateTimer; 
        public ObservableCollection<string> DGItems { get; set; } = [];
        public ScoreCaculator ScoreCa { get; set; }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            var brushList = new Brush[] { Brushes.Red, Brushes.Blue, Brushes.Black };
            cb_CDTotalColor.ItemsSource = brushList;
            cb_CDWarnColor.ItemsSource = brushList;
            this.DataContext = this;

            progress = new Progress<string>(p => tb_Show.Text += $"{p}\n");
            ScoreCa = new(progress);
        }

        #region PPT+Timer 
        private void Btn_ReadFile_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = MyFilePath.SelectFolderPath(null);
            if (string.IsNullOrEmpty(folderPath))
                return;
            var filePaths = MyFilePath.GetFilePathInSelectedFolder(null, folderPath);
            foreach (var item in filePaths)
            {
                DGItems.Add(item);
            }
        }
        private void Btn_Open_Click(object sender, RoutedEventArgs e)
        {
            pptCD ??= new(12, Brushes.Blue, 5, Brushes.Red, 1, progress); //初始化默认值
            separateTimer = null;

            var selectPath = dataGrid.SelectedItem as string;
            string[] extensionList = [".pptx", ".ppt"];
            if (File.Exists(selectPath))
                if (extensionList.Contains(System.IO.Path.GetExtension(selectPath)))
                    pptCD?.PPTOpen(selectPath);
        }
        private void Btn_SetPara_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pptCD = new(int.Parse(tb_CDTotalTime.Text), (Brush)cb_CDTotalColor.SelectedItem, int.Parse(tb_CDWarnTime.Text), (Brush)cb_CDWarnColor.SelectedItem, int.Parse(tb_CDRefresh.Text), progress)
                {
                    IsZeroEventActived = cb_IsZeroEventActived.IsChecked == true
                }; //设定主要参数
                separateTimer = TimerStarter.CreatCountDownTimer(int.Parse(tb_CDTotalTime.Text), (Brush)cb_CDTotalColor.SelectedItem, int.Parse(tb_CDWarnTime.Text), (Brush)cb_CDWarnColor.SelectedItem, int.Parse(tb_CDRefresh.Text), true, ZeroEvent, null, null);
                progress.Report("参数已设定");
            }
            catch (Exception ex)
            {
                progress.Report(ex.Message);
            }            
        }
        #endregion

        #region 单独启动计时器
        private void Btn_OpenTimer_Click(object sender, RoutedEventArgs e)
        {
            separateTimer ??= separateTimer = TimerStarter.CreatCountDownTimer(12, Brushes.Blue, 5, Brushes.Red, 1, true, ZeroEvent, null, null);
            pptCD = null;

            separateTimer?.StartOrStop();
        }
        private void ZeroEvent(object? sender, EventArgs e)
        {
            cb_IsZeroEventActived.Dispatcher.Invoke(() =>
            {
                if (cb_IsZeroEventActived.IsChecked == true)
                    progress.Report("时间到");
            });           
        }
        #endregion

        private void Tb_Show_TextChanged(object sender, TextChangedEventArgs e)
        {
            tb_Show.ScrollToEnd();
        }
    }
}