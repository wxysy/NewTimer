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
        string? processName = null; //打开文件所关联的进程名称
        MainSettingModel mainSettings = new();
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

        #region 打开文件并启动Timer 
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
            var selectPath = dataGrid.SelectedItem as string;
            if (File.Exists(selectPath))
            {
                string[] extensionList = [".pptx", ".ppt"];
                string ext = System.IO.Path.GetExtension(selectPath);

                pptCD = null;
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
                    separateTimer ??= TimerStarter.CreatCountDownTimer(mainSettings.CountDownSeconds, mainSettings.CountDownColor, mainSettings.WarningSeconds, mainSettings.WarningColor, mainSettings.TimerInterval, mainSettings.IsUIControlActived, ZeroEvent, null, null);
                    processName = AnyFileOpen(selectPath); //存储打开文件关联的进程名称，用于之后关闭进程。
                    separateTimer?.StartOrStop();
                }
            }

        }
        private void ZeroEvent(object? sender, EventArgs e)
        {
            cb_IsZeroEventActived.Dispatcher.Invoke(() =>
            {
                if (cb_IsZeroEventActived.IsChecked == true)
                {
                    /* 参考
                     * 1、《C#实现关闭某个指定程序》
                     * https://blog.csdn.net/laozhuxinlu/article/details/50422057
                     * 2、《Process类的CloseMainWindow, Kill, Close》
                     * https://www.cnblogs.com/zjoch/p/3654940.html
                     * 3、《C#各种结束进程的方法详细介绍》
                     * https://blog.csdn.net/yl2isoft/article/details/54176740
                     * 4、PowerPoint进程实测信息
                     * string pptProgressName = "POWERPNT";//进程名称：POWERPNT.EXE
                     * string pptMainWindowTitle = "PowerPoint";
                     */
                    progress.Report("时间到");
                    Process[] processes = Process.GetProcesses();
                    foreach (Process p in processes)
                    {
                        if (p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                        {
                            //p.Kill();//太刚猛，没必要
                            p.CloseMainWindow();//Process.CloseMainWindow是GUI程序的最友好结束方式
                        }
                    }
                    processName = default; //使变量回到初始值
                }
                else
                { }
            });
        }
        private static string? AnyFileOpen(string selectPath)
        {
            //【启动程序】
            var startInfo = new ProcessStartInfo()
            {
                //ArgumentList = { "abc", "def" }, //启动参数列表。MainWindow(string[]? startUpArgs)
                FileName = selectPath,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                UseShellExecute = true,
            };

            var ps = Process.Start(startInfo);
            return ps?.ProcessName;
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
            separateTimer?.Close();
            separateTimer ??= TimerStarter.CreatCountDownTimer(mainSettings.CountDownSeconds, mainSettings.CountDownColor, mainSettings.WarningSeconds, mainSettings.WarningColor, mainSettings.TimerInterval, mainSettings.IsUIControlActived, null, null, null);
            separateTimer?.StartOrStop();
        }

        private void Tb_Show_TextChanged(object sender, TextChangedEventArgs e)
        {
            tb_Show.ScrollToEnd();
        }
    }
}