using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Office2024Core = Microsoft.Office.Core;
using Office2024PPT = Microsoft.Office.Interop.PowerPoint;

namespace PPTLib.Functions
{
    /// <summary>
    /// 一个文件执行一个实例
    /// </summary>
    public class PPTPlay : INotifyPropertyChanged
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

        /* 参考
         * 1、《C# 编程：在 winform 窗口中实现幻灯片pptx播放》
         * https://blog.csdn.net/caifox/article/details/145231396
         * 2、《（转）C#操作PPT》
         * https://www.cnblogs.com/hhhh2010/p/4630738.html
         * 3、《如何在wpf窗口中播放PPT。》
         * https://www.cnblogs.com/tony-god/p/7650747.html
         */

        #region 属性和字段
        const string pptProgressName = "POWERPNT";//进程名称：POWERPNT.EXE
        Office2024PPT.Application? pptApp;
        Office2024PPT.Presentation? pptPresentation;
        Office2024PPT.SlideShowSettings? pptSlideShowSettings;
        Office2024PPT.SlideShowWindows? pptSlideShowWindows; //（暂无用）pptApp.SlideShowWindows
        Office2024PPT.SlideShowView? pptSlideShowView; //（暂无用）pptPresentation.SlideShowWindow.View

        private bool isPPTShowed = false;
        public bool IsPPTShowed
        {
            get { return isPPTShowed; }
            private set
            { isPPTShowed = value; NotifyPropertyChanged(); }
        }
        #endregion

        public PPTPlay()
        {
            PPTInitialize();
        }

        #region 方法和事件
        public event EventHandler? PPTShowBegin;
        private void OnPPTShowBegin() => PPTShowBegin?.Invoke(this, EventArgs.Empty);

        public event EventHandler? PPTShowEnd;
        private void OnPPTShowEnd() => PPTShowEnd?.Invoke(this, EventArgs.Empty);

        private void PPTInitialize()
        {
            if (pptApp != null)  //防止连续打开多个PPT程序
                return;

            pptApp = new Office2024PPT.Application(); //{ Visible = Office2024Core.MsoTriState.msoTrue, }; //设置为可见(不要设，设定后程序界面就出现了)
            pptApp.SlideShowBegin += PptApp_SlideShowBegin; //PPT开始放映
            pptApp.SlideShowEnd += PptApp_SlideShowEnd; //PPT结束放映
        }
        private void PptApp_SlideShowBegin(Office2024PPT.SlideShowWindow Wn)
        {
            IsPPTShowed = true;
            OnPPTShowBegin();
        }
        private void PptApp_SlideShowEnd(Office2024PPT.Presentation Pres)
        {
            IsPPTShowed = false;
            OnPPTShowEnd();
        }

        public bool PPTOpen(string filepath)
        {           
            try
            {
                if (File.Exists(filepath) != true)
                    return false;

                //【付费】《调用Application.Quit()方法时报"This operation cannot be performed in this event handler."错误的一种解决方法》
                // https://blog.csdn.net/Neils03/article/details/9878575
                //在子线程中打开并播放PPTX文件时遇到的问题
                //第一次打开一个文件并结束幻灯片时一切顺利，但是重复几次同样的操作，就会报出"Application (unknown member) : Invalid request.  This operation cannot be performed in this event handler."的COMException异常。
                //先后进行了多次修改调用的位置（不使用子线程和使用子线程调用），但是总是运行到Quit方法时报错，从Google上也没有找到明确的答案，只是说COM对象是出于STA模式下。
                //后来突然看到了Open方法的参数说明：
                //其中WithWindow开始使用的是默认值，即MsoTriState.msoTrue。这样在关闭幻灯片时，如果不调用Quit方法会留下PPT窗口。而当WithWindow设置为MsoTriState.msoFalse，则关闭幻灯片不会留下窗口，这样就不用调用Quit方法了。
                //当我的程序关闭时再调用Application.Quit()方法，这样能保证即关闭了PPT程序，同时也不会因为打开几次PPT文件而报错了。
                //至于为什么不允许在事件中调用Quit，暂时还不清楚。

                pptPresentation = pptApp!.Presentations.Open(filepath, Office2024Core.MsoTriState.msoTrue, Office2024Core.MsoTriState.msoFalse, Office2024Core.MsoTriState.msoFalse);
                pptSlideShowSettings = pptPresentation.SlideShowSettings;
                pptSlideShowSettings!.ShowType = Office2024PPT.PpSlideShowType.ppShowTypeSpeaker; //设置播放模式
                pptSlideShowSettings.Run();

                return true;
            }
            catch (Exception)
            { return false; }
        }
        public bool PPTClose()
        {           
            try
            {
                if (pptPresentation != null)
                {
                    pptPresentation?.Close();
                    pptPresentation = null;
                }
                //if (pptApp != null) //pptApp不能关闭，详见PPTOpen方法。
                //{
                //    pptApp?.Quit();
                //    pptApp = null;
                //}
                //GC.Collect();//强制内存回收（也不知道这一句现在还有没有用）

                //Office2024新问题：前面操作关闭只能关闭文档，不能关闭PPT程序。
                //添加一个C#关闭进程方法
                ClosePPTProgram(pptProgressName);
                return true;
            }
            catch (Exception)
            { return false; }
        }
        private static void ClosePPTProgram(string processName)
        {
            /* 参考
             * 1、《C#实现关闭某个指定程序》
             * https://blog.csdn.net/laozhuxinlu/article/details/50422057
             * 2、《C#各种结束进程的方法详细介绍》
             * https://blog.csdn.net/yl2isoft/article/details/54176740
             * https://www.cnblogs.com/zjoch/p/3654940.html
             * 3、PowerPoint进程实测信息
             * string pptProgressName = "POWERPNT";//进程名称：POWERPNT.EXE
             * string pptMainWindowTitle = "PowerPoint";
             */

            Process[] processes = Process.GetProcesses();
            foreach (Process p in processes)
            {
                if (p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                {
                    p.Kill();//太刚猛，没必要
                    //p.CloseMainWindow();//Process.CloseMainWindow是GUI程序的最友好结束方式
                }
            }

            //【启动程序】
            //var startInfo = new ProcessStartInfo()
            //{
            //    ArgumentList = { "abc", "def" }, //启动参数列表。MainWindow(string[]? startUpArgs)
            //    FileName = @".\WpfPluginManager.exe",
            //    WorkingDirectory = Directory.GetCurrentDirectory(),
            //    UseShellExecute = true,
            //};
            //var ps = Process.Start(startInfo);
        }
        #endregion

    }
}
