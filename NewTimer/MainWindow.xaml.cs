using System.Collections.ObjectModel;
using System.Diagnostics;
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
using NewTimer.ModelDir;
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
        #region 属性和字段
        string? processName = null; //打开文件所关联的进程名称
        MainSettingModel mainSettings = new();
        IProgress<string> progress;
        PPTCountDown? pptCD;
        AppCountDown? appCD;
        CountDownTimer? separateTimer; 
        public ObservableCollection<string> DGItems { get; set; } = [];
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            var brushList = new Brush[] { Brushes.Red, Brushes.Blue, Brushes.Black };
            cb_CDTotalColor.ItemsSource = brushList;
            cb_CDWarnColor.ItemsSource = brushList;
            this.DataContext = this;

            progress = new Progress<string>(p => tb_Show.Text += $"{p}\n");           
        }

        #region 打开文件并启动Timer 
        private void Btn_ReadFile_Click(object sender, RoutedEventArgs e)
        {
            DGItems.Clear();
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
            var selectPath = dataGrid.SelectedItem as string;
            if (File.Exists(selectPath))
            {
                string[] extensionList = [".pptx", ".ppt"];
                string ext = System.IO.Path.GetExtension(selectPath);

                pptCD = null;
                appCD = null;
                separateTimer?.CloseTimerWindow();
                separateTimer = null;

                if (extensionList.Contains(ext)) //PPT文件的处理
                {
                    pptCD ??= new(mainSettings.CountDownSeconds, mainSettings.CountDownColor, mainSettings.WarningSeconds, mainSettings.WarningColor, mainSettings.TimerInterval, progress)
                    {
                        IsZeroEventActived = mainSettings.IsZeroEventActived,
                        IsUIControlActived = mainSettings.IsUIControlActived,
                    }; //初始化默认值

                    pptCD?.PPTOpen(selectPath);
                }
                else //其他文件的处理
                {
                    appCD ??= new(mainSettings.CountDownSeconds, mainSettings.CountDownColor, mainSettings.WarningSeconds, mainSettings.WarningColor, mainSettings.TimerInterval, progress)
                    {
                        IsZeroEventActived = mainSettings.IsZeroEventActived,
                        IsUIControlActived = mainSettings.IsUIControlActived,
                    };
                    appCD?.AppOpen(selectPath);
                }
            }

        }       
        #endregion

        //参数设定
        private void Btn_SetPara_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainSettings.CountDownSeconds = int.Parse(tb_CDTotalTime.Text);
                mainSettings.CountDownColor = (Brush)cb_CDTotalColor.SelectedItem;
                mainSettings.WarningSeconds = int.Parse(tb_CDWarnTime.Text);
                mainSettings.WarningColor = (Brush)cb_CDWarnColor.SelectedItem;
                mainSettings.TimerInterval = int.Parse(tb_CDRefresh.Text);
                mainSettings.IsUIControlActived = cb_IsUIControlActived.IsChecked == true;
                mainSettings.IsZeroEventActived = cb_IsZeroEventActived.IsChecked == true;
                progress.Report("参数已设定");
            }
            catch (Exception ex)
            {
                progress.Report(ex.Message);
            }
        }
        //单独启动计时器
        private void Btn_OpenTimer_Click(object sender, RoutedEventArgs e)
        {
            pptCD = null;
            appCD = null;
            separateTimer?.CloseTimerWindow();
            separateTimer = null;
            separateTimer ??= TimerStarter.CreatCountDownTimer(mainSettings.CountDownSeconds, mainSettings.CountDownColor, mainSettings.WarningSeconds, mainSettings.WarningColor, mainSettings.TimerInterval, true, null, null, null); //单独启动计时器必须允许UI操作，要不怎么搞法。
            separateTimer?.StartOrStop();
        }

        private void Tb_Show_TextChanged(object sender, TextChangedEventArgs e)
        {
            tb_Show.ScrollToEnd();
        }
    }
}