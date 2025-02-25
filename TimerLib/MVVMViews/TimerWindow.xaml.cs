using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;

namespace TimerLib.MVVMViews
{
    /* 1、《[小结][N种方法]实现WPF不规则窗体》
     * https://www.cnblogs.com/DebugLZQ/archive/2013/05/16/3081802.html
     * 2、《自定义WPF窗体形状》
     * https://blog.csdn.net/u012525929/article/details/82312546
     * 3、《WPF自定义界面WindowChrome》
     * https://www.cnblogs.com/choumengqizhigou/p/15739993.html
     * 4、《wpf 右键菜单的使用》
     * https://www.cnblogs.com/wjx-blog/p/11008445.html
     * 5、《WPF 中如何取消关闭窗口》
     * https://blog.csdn.net/BYH371256/article/details/135414248
     * 6、《ContextMenu 概述》右键菜单
     * https://learn.microsoft.com/zh-cn/dotnet/desktop/wpf/controls/contextmenu-overview?view=netframeworkdesktop-4.8&viewFallbackFrom=netdesktop-9.0
     * 7、《WPF中的嵌入的资源与Resource》
     * https://blog.csdn.net/u012522829/article/details/112532167
     * 8、《WPF 中 Visibility.Collapsed 与 Visibility.Hidden 区别》
     * https://blog.csdn.net/MrBaymax/article/details/89474702
     * Collapsed与Hidden相比，有非常大的优势，Hidden仅仅是属性设为不可视，但是属性在画面上依然占有空间。
     * 然而使用Collapsed的话，在不可视的基础上，它还能将属性在画面上的占位符清除，属性将彻底不影响画面。所以，某些时候使用Collapsed将更为合理。
     * 例如：在StackPanel中使用该属性的时候最为明显。
     * 三个对象在同一个StackPanel中，中间的对象如果Hidden，还将占有Stack中的位置，而Collapsed的话，下面的对象就会挤上来，占据中间对象的位置。
     */

    /// <summary>
    /// TimerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TimerWindow : Window, INotifyPropertyChanged
    {
        #region 定义属性发生变化时引发的事件及相关操作（里面的内容是固定的，直接用。）
        /*--------监听事件处理程序------------------------------------------------------------------------*/
        /// <summary>
        /// 属性发生变化时引发的事件
        /// </summary>
        //[field: NonSerializedAttribute()]//保证事件PropertyChanged不被序列化的必要设定。事件不能序列化！
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 提醒侦听者们（listeners）属性已经变化
        /// </summary>
        /// <param name="propertyName">变化的属性名称。
        /// 这是可选参数，能够被CallerMemberName自动提供。
        /// 当然你也可以手动输入</param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /* 上面原型
         protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
         */

        // 下面这个方法目前不知道干啥的
        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(storage, value))
            {
                return false;
            }
            else
            {
                storage = value;
                NotifyPropertyChanged(propertyName);

                return true;
            }
        }

        /*------------------------------------------------------------------------------------------------*/
        #endregion

        #region 字段和属性
        DispatcherTimer dispatcherTimer; //右下角滚动文字用的计时器
        private bool isUIOperationsAllowed = true;//是否允许UI界面点击按钮操作
        private bool isImageClickClose = false; //是否是点击关闭图片的形式关闭
        private string timeDisplay = "-00:00"; //"-00:00"
        public string TimeDisplay //计时数字显示
        {
            get { return timeDisplay; }
            private set { timeDisplay = value; NotifyPropertyChanged(); }
        }

        private Brush timeColor = Brushes.Black;
        public Brush TimeColor //计时颜色显示
        {
            get { return timeColor; }
            private set { timeColor = value; NotifyPropertyChanged(); }
        }

        private bool isRunning = false; //计时器是否运行
        public bool IsRunning
        {
            get { return isRunning; }
            private set
            {
                isRunning = value;
                NotifyPropertyChanged();
                if (isRunning)
                    runningImg.Dispatcher.Invoke(() => //也可使用this.Dispatcher.Invoke，意思都是切换到UI界面线程。
                    {
                        runningImg.Visibility = Visibility.Visible; //用是否运行标志显隐来指示运行状态
                        var uriStr = "pack://application:,,,/TimerLib;component/Pictures/stop.png"; //调用生成操作为“资源”的图片（不是“嵌入的资源”）
                        pauseImg.Source = new BitmapImage(new Uri(uriStr, UriKind.RelativeOrAbsolute)); //用于切换暂停按钮背景图片（指示启动或者停止）
                    });
                else
                    runningImg.Dispatcher.Invoke(() =>
                    {
                        runningImg.Visibility = Visibility.Hidden;
                        var uriStr = "pack://application:,,,/TimerLib;component/Pictures/start.png";
                        pauseImg.Source = new BitmapImage(new Uri(uriStr, UriKind.RelativeOrAbsolute));
                    });
            }
        }
        private string? mes = "--:--"; //右下角信息栏显示信息（一般为日期格式，如"MM:ss"）
        public string? Mes
        {
            get { return mes; }
            private set { mes = value; NotifyPropertyChanged(); }
        }

        private double left = 0; //TextBlock 位于 Canvas 面板的坐标，用于右下角信息栏滚动显示。
        public double CVLeft
        {
            get { return left; }
            private set { left = value; NotifyPropertyChanged(); }
        }

        #endregion

        public TimerWindow(string timeDisplay, Brush timeColor, bool allowUI)
        {
            InitializeComponent();
            this.DataContext = this;
            TimeDisplay = timeDisplay;
            TimeColor = timeColor;
            isUIOperationsAllowed = allowUI;

            //右下角信息栏滚动显示
            dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(300), };
            dispatcherTimer.Tick += (sender, e) =>
            {
                var actualWidth = tb_Mes.ActualWidth;
                var p = (CVLeft - 58); //58为Canvas的实际长度
                if (p > -actualWidth)
                    CVLeft -= 2; //负值向左滚动，正值向右边滚动
                else
                    CVLeft = 0;
            };
            dispatcherTimer.Start();
        }

        #region 事件
        /// <summary>
        /// 点击关闭按钮时，在关闭窗体之前的额外操作。
        /// </summary>
        public event EventHandler<string?>? BeforeTimerWindowClosed;
        private void OnBeforeTimerWindowClosed(string? para) => BeforeTimerWindowClosed?.Invoke(this, para);

        /// <summary>
        /// 点击暂停按钮时动作
        /// </summary>
        public event EventHandler<string?>? PauseClicked;
        private void OnPauseClicked(string? para) => PauseClicked?.Invoke(this, para);

        /// <summary>
        /// 点击设定按钮时的动作
        /// </summary>
        public event EventHandler<string?>? SettingsClicked;
        private void OnSettingsClicked(string? para) => SettingsClicked?.Invoke(this, para);

        /// <summary>
        /// 右键菜单按钮点击操作
        /// </summary>
        public event EventHandler<Dictionary<string, object>>? RightClickMenuItemClicked;
        private void OnRightClickMenuItemClicked(Dictionary<string, object> para) => RightClickMenuItemClicked?.Invoke(this, para);
        #endregion

        #region 方法
        public void Display(string str, Brush strColor, bool isRunning, string? mes)
        {
            TimeDisplay = str;
            TimeColor = strColor;
            IsRunning = isRunning;
            Mes = mes;
        }
        public void AllowUIOperations(bool state) => isUIOperationsAllowed = state;

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();//为Gird订阅的MouseLeftButtonDown路由事件，是为了实现窗体的拖动。
        }
        private void ImageClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;//若不加，this.DragMove();会报错
            isImageClickClose = true;
            this.Close();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (isImageClickClose)
            {
                var res = MessageBox.Show("关闭计时器？", "注意", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No)
                    e.Cancel = true; //取消关闭窗口
                else
                {
                    OnBeforeTimerWindowClosed(TimeDisplay);
                    //IsRunning = false; //都关闭窗口了，无所谓它运不运行了。外面用Display方法控制就可以。
                }
            }
            else
            { 
                OnBeforeTimerWindowClosed(TimeDisplay);
                //IsRunning = false; //都关闭窗口了，无所谓它运不运行了。外面用Display方法控制就可以。
            }
        }
        private void ImagePause_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            OnPauseClicked(TimeDisplay);
        }
        private void ImageSettings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            OnSettingsClicked(TimeDisplay);
        }
        #endregion

        #region 鼠标操作（鼠标划入划出、右键菜单）
        private void Tb_TimeDisplay_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (isUIOperationsAllowed)
                tb_TimeDisplay.ContextMenu.Visibility = Visibility.Visible;
            else
                tb_TimeDisplay.ContextMenu.Visibility = Visibility.Collapsed;
        }
        private void CMTest_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var source = e.Source as MenuItem;
            var infoDict = new Dictionary<string, object>()
            {
                [source!.Name] = source.Tag
            };
            OnRightClickMenuItemClicked(infoDict);
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var source = e.Source as MenuItem;
            var infoDict = new Dictionary<string, object>()
            {
                [source!.Name] = source.Tag
            };
            OnRightClickMenuItemClicked(infoDict);
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isUIOperationsAllowed)
                actionGrid.Visibility = Visibility.Visible;
        }
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (actionGrid.Visibility == Visibility.Visible)
                actionGrid.Visibility = Visibility.Collapsed;
        }
        #endregion      
    }
}
