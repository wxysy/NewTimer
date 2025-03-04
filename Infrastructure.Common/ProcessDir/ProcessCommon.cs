using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Common.ProcessDir
{
    /// <summary>
    /// 该类本质上都是对System.Diagnostics.Process类的封装
    /// </summary>
    public class ProcessCommon
    {
        public static Process? StartProcess(string filePath, string[]? args)
        {
            /*《ProcessStartInfo.ArgumentList 属性》
             * https://learn.microsoft.com/zh-cn/dotnet/api/system.diagnostics.processstartinfo.argumentlist?view=net-8.0#system-diagnostics-processstartinfo-argumentlist
             * ArgumentList Arguments和 属性彼此独立，并且只能同时使用其中一个属性。 
             * 这两个 API 之间的main区别在于负责ArgumentList转义提供的参数，并在内部生成在调用 Process.Start(info)时传递给操作系统的单个字符串。 
             * 因此，如果不确定如何正确转义参数，则应选择 ArgumentList 而不是 Arguments。
             */

            //【启动程序】
            var startInfo = new ProcessStartInfo()
            {
                //ArgumentList = { "abc", "def" }, //启动参数列表。MainWindow(string[]? startUpArgs)
                FileName = filePath, //@".\WpfPluginManager.exe",
                WorkingDirectory = Directory.GetCurrentDirectory(),
                UseShellExecute = true, //指定是否使用操作系统 shell 启动进程
            };
            //ArgumentList是只读属性，不能直接操作,只能用Add()方法。
            if (args != null)
                foreach (var arg in args)
                    startInfo.ArgumentList.Add(arg);
            
            var ps = Process.Start(startInfo);
            return ps;
        }

        //启动且仅保留1个相同名称的线程
        public static Process? StartProcessOnlyOneByName(string filePath, string[]? args)
        {
            var ps = StartProcess(filePath, args);

            //确保只保留1个程序进程
            var id = ps?.Id;
            var name = ps?.ProcessName;
            var pList = GetProcessByName(name);
            if (pList?.Length > 1)
            {
                foreach (var p in pList)
                {
                    if (p.Id != id)
                    {
                        var res = p.CloseMainWindow();
                        if (!res)
                            p.Kill();
                    }
                    else
                    { }
                }
            }
            else
            { }

            return ps;
        }

        public static Process[] GetProcessByName(string? processName)
        {
            /*《Process.GetProcessesByName 方法》
             * https://learn.microsoft.com/zh-cn/dotnet/api/system.diagnostics.process.getprocessesbyname?view=net-8.0
             * 进程名称是进程（如 Outlook）的友好名称，不包括 .exe 扩展或路径。 GetProcessesByName 有助于获取和操作与同一可执行文件关联的所有进程。
             * 例如，可以将可执行文件名称作为 processName 参数传递，以便关闭该可执行文件的所有正在运行的实例。
             * 尽管进程 Id 对系统上的单个进程资源是唯一的，但本地计算机上的多个进程可以运行由 processName 参数指定的应用程序。
             * 因此，GetProcessById 最多返回一个进程，但 GetProcessesByName 返回包含所有关联进程的数组。
             */
            return Process.GetProcessesByName(processName);
        }
        public static void CloseProcessByName(string processName)
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

            Process[] processes = Process.GetProcesses();
            foreach (Process p in processes)
            {
                if (p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                {

                    var res = p.CloseMainWindow();//Process.CloseMainWindow是GUI程序的最友好结束方式
                    if (!res)
                        p.Kill();//如果友好的方式结束不了，那就来硬的！
                }
                else
                { }
            }

            //【启动程序】
            //var startInfo = new ProcessStartInfo()
            //{
            //    //ArgumentList = { "abc", "def" }, //启动参数列表。MainWindow(string[]? startUpArgs)
            //    FileName = filePath, //@".\WpfPluginManager.exe",
            //    WorkingDirectory = Directory.GetCurrentDirectory(),
            //    UseShellExecute = true, //指定是否使用操作系统 shell 启动进程
            //};  
            //if (args != null)
            //    foreach (var arg in args)
            //        startInfo.ArgumentList.Add(arg); //ArgumentList是只读属性，不能直接操作,只能用Add()方法。
            //var ps = Process.Start(startInfo);
        }
    }
}
