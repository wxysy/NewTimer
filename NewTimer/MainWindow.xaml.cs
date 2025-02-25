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
        public ObservableCollection<string> DGItems { get; set; } = [];
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            progress = new Progress<string>(p => tb_Show.Text += $"{p}\n");
            pptCD = new(12, Brushes.Blue, 5, Brushes.Red, 1, progress); //初始化默认值
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
            var selectPath = dataGrid.SelectedItem as string;
            string[] extensionList = [".pptx", ".ppt"];
            if (File.Exists(selectPath))
                if (extensionList.Contains(System.IO.Path.GetExtension(selectPath)))
                    pptCD?.PPTOpen(selectPath);
        }
        private void Btn_ClosePPT_Click(object sender, RoutedEventArgs e)
        {
            pptCD?.PPTClose();
        }
        private void Btn_SetPara_Click(object sender, RoutedEventArgs e)
        {
            pptCD = new(int.Parse(tb_CDTotalTime.Text), Brushes.Blue, int.Parse(tb_CDWarnTime.Text), Brushes.Red, int.Parse(tb_CDRefresh.Text), progress); //设定主要参数
            progress.Report("参数已设定");
        }
        #endregion
    }
}