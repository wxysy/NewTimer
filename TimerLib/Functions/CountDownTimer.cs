using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using TimerLib.MVVMViews;

namespace TimerLib.Functions
{
    public class CountDownTimer: INotifyPropertyChanged
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
        int countdownTimeSet = 360; //设定倒计时时间(360s，6分钟)
        int warningTimeSet = 60; //设定提醒时间(60s，1分钟)
        int interval = 1;//刷新频率(1s) 
        Brush countdownColor = Brushes.Black; //倒计时颜色
        Brush warningColor = Brushes.Red; //提醒颜色            
        TimerWindow? timerWindow;
        System.Timers.Timer? mainTimer;//主计时器
        bool isTimerWindowOpened = false;
        bool isUIOperationsAllowedSet = true;
        public bool IsUIOperatesAllowed { get; private set; }//是否允许UI界面点击按钮操作

        private bool isTimerRunning = false;//是否在运行
        /// <summary>
        /// 是否运行
        /// </summary>
        public bool IsTimerRunning
        {
            get { return isTimerRunning; }
            private set { isTimerRunning = value; NotifyPropertyChanged(); }
        }

        private int leftTime = 0;//剩余时间
        /// <summary>
        /// 剩余时间(s)
        /// </summary>
        public int TimeLeft
        {
            get { return leftTime; }
            private set { leftTime = value; NotifyPropertyChanged(); }
        }

        private string timeDisplay = "-00:00"; //计时数字显示
        public string TimeDisplay 
        {
            get => timeDisplay;
            private set { timeDisplay = value; NotifyPropertyChanged(); }
        }

        private Brush timeColor = Brushes.Black; //计时颜色显示
        public Brush TimeColor 
        {
            get => timeColor;
            private set { timeColor = value; NotifyPropertyChanged(); }
        }
        #endregion

        public CountDownTimer(int countDownSeconds, Brush countDownColor, int warningSeconds, Brush warningColor, int timerInterval, bool allowUIControl)
        {
            LoadingParas(countDownSeconds, countDownColor, warningSeconds, warningColor, timerInterval, allowUIControl);//设定主要参数
            InitializeTimer();//初始化Timer
        }

        #region 方法和事件
        /// <summary>
        /// 0时刻事件(除停止计时器和关闭窗体外的)
        /// </summary>
        public event EventHandler? ZeroEvent;
        private void OnZeroEvent() => ZeroEvent?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// 倒计时器在关闭之前的操作(e:剩余时间s)。
        /// 【注意】如果在0时刻关闭，也会引发此事件。
        /// </summary>
        public event EventHandler<int>? TimerClosingEvent;
        private void OnTimerClosingEvent(int para) => TimerClosingEvent?.Invoke(this, para);

        /// <summary>
        /// 计时器每次Tick时的额外操作(不用在此类中编写倒计时变化，且0时刻不会引发此事件)
        /// </summary>
        public event EventHandler<int>? TimerTickEvent;
        private void OnTimerTickEvent(int timeLeft) => TimerTickEvent?.Invoke(this, timeLeft);


        private void LoadingParas(int cdSeconds, Brush cdColor, int wnSeconds, Brush wnColor, int timerInterval, bool allowUIControl)
        {
            if (cdSeconds >= 6000 || cdSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(cdSeconds), "倒计时时间有效范围是1-5999s");
            if (timerInterval < 1)
                throw new ArgumentOutOfRangeException(nameof(timerInterval), "刷新频率最高为1s");
            countdownTimeSet = cdSeconds;
            countdownColor = cdColor;
            warningTimeSet = wnSeconds;
            warningColor = wnColor;
            TimeLeft = countdownTimeSet;
            interval = timerInterval;
            isUIOperationsAllowedSet = allowUIControl;
            IsUIOperatesAllowed = allowUIControl;
        }
        private void InitializeTimer()
        {           
            mainTimer = new() { Interval = interval * 1000 }; //1s刷新频率
            mainTimer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {           
            TimeLeft -= interval; //每次减少1s
            timerWindow?.Display(DisplayFormat(TimeLeft), TimeLeft > warningTimeSet ? countdownColor : warningColor, IsTimerRunning, "MM:ss");
            
            if (TimeLeft == 0)
            {
                mainTimer?.Stop(); //停止计时
                OnZeroEvent(); //执行0时刻事件
                Close();    
            }
            else
            { OnTimerTickEvent(TimeLeft); }
        }
        private static string DisplayFormat(int input)
        {
            string m = Math.Abs(input / 60).ToString().PadLeft(2, '0'); //分钟，Math.Abs是为了处理负数前面的“-”号
            string s = Math.Abs(input % 60).ToString().PadLeft(2, '0'); //秒
            string symbol = input < 0 ? "-" : string.Empty; //正负号
            string res = $"{symbol}{m}:{s}";
            return res ;
        }
        public void StartOrStop()
        {
            switch (IsTimerRunning)
            {
                case true:
                    PauseOrStop();
                    break;
                case false:
                    ContinueOrStart();
                    break;
            }
            timerWindow?.Display(DisplayFormat(TimeLeft), TimeLeft > warningTimeSet ? countdownColor : warningColor, IsTimerRunning, "MM:ss");
        }
        private void PauseOrStop()
        {
            mainTimer?.Stop();
            IsTimerRunning = false;           
        }

        private void ContinueOrStart()
        {
            if (isTimerWindowOpened == false)
            {
                isTimerWindowOpened = true;

                //《WPF 之 调用线程必须为 STA,因为许多 UI 组件都需要》
                //https://www.cnblogs.com/xinaixia/p/5706096.html
                Application.Current.Dispatcher.BeginInvoke(() => //必须要BeginInvoke，Invoke会卡死。
                {
                    timerWindow = new(DisplayFormat(TimeLeft), TimeLeft > warningTimeSet ? countdownColor : warningColor, IsUIOperatesAllowed);
                    timerWindow.BeforeTimerWindowClosed += TimerWindow_BeforeTimerWindowClosed;
                    timerWindow.RightClickMenuItemClicked += TimerWindow_RightClickMenuItemClicked;
                    timerWindow.PauseClicked += TimerWindow_PauseClicked;
                    timerWindow.SettingsClicked += TimerWindow_SettingsClicked;
                    timerWindow?.Show();
                });
            }
            else
            { }
            mainTimer?.Start();
            IsTimerRunning = true;            
        }

        public void Close()
        {
            //timerWindow?.Dispatcher.InvokeShutdown(); //这会把主程序一并给关了
            //《程序在Dispatcher.Run()处挂起 c#》
            //https://dev59.com/hoXca4cB1Zd3GeqPLKAx
            //但这里的使用和参考文献还有不同，参考文献给的是Dispatcher.CurrentDispatcher.InvokeShutdown();

            //解决“调用线程无法访问此对象，因为另一个线程拥有该对象。”引发的问题
            timerWindow?.Dispatcher.Invoke(() => //这种方法有潜藏的线程问题，与其他部件结合时要小心。
            {
                timerWindow?.Close();
                //《C# WPF 调用线程无法访问此对象，因为另一个线程拥有该对象，解决办法》
                //https://blog.csdn.net/YouthMe/article/details/102852580
                //但这里的使用和参考文献还有不同，不是this.Dispatcher.Invoke而是timerWindow?.Dispatcher.Invoke。
            });
        }

        public void AllowUIOperations(bool state) 
        {
            IsUIOperatesAllowed = state;
            timerWindow?.AllowUIOperations(IsUIOperatesAllowed); 
        }

        private void TimerWindow_BeforeTimerWindowClosed(object? sender, string? e)
        {
            mainTimer?.Stop(); //停止计时
            OnTimerClosingEvent(TimeLeft);//0时刻也会引发此事件
            Reset(); //计时器复位
            isTimerWindowOpened = false;
            IsTimerRunning = false;
            IsUIOperatesAllowed = isUIOperationsAllowedSet;
        }
        private void TimerWindow_RightClickMenuItemClicked(object? sender, Dictionary<string, object> e)
        {
            //这里放TimerWindow菜单右键的各种执行（目前没想好干什么）
            foreach (var kvp in e) 
            {
                MessageBox.Show($"（目前没想好干什么）{kvp.Key}--{kvp.Value}");
            }
        }
        private void TimerWindow_PauseClicked(object? sender, string? e)
        {
            StartOrStop();
        }
        private void TimerWindow_SettingsClicked(object? sender, string? e)
        {
            CountdownSettingsView settingsView = new();
            settingsView.CallBack += (sender, e) =>
            {
                countdownTimeSet = e[0];
                warningTimeSet = e[1];
                Reset();
            };
            settingsView.Show();
        }

        public void Reset() => TimeLeft = countdownTimeSet;
        #endregion
    }
}
