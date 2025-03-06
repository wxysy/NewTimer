using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Infrastructure.Common.ProcessDir;
using PPTLib.Functions;
using TimerLib;
using TimerLib.Functions;
using Windows.Media.Capture.Core;
using ProcessCommon = Infrastructure.Common.ProcessDir.ProcessCommon;

namespace NewTimer.FunctionDir
{
    public class AppCountDown(int countDownSeconds, Brush countDownColor, int warningSeconds, Brush warningColor, int timerInterval, IProgress<string>? pg)
    {
        #region 属性和字段
        IProgress<string>? progress = pg;
        public Process? Component_Process { get; private set; }
        public CountDownTimer? Component_Timer { get; private set; }
        public bool IsZeroEventActived { get; set; } = true;
        public bool IsUIControlActived { get; set; } = false;
        #endregion

        public void AppOpen(string filePath)
        {
            progress?.Report($"|打开APP|{Path.GetFileName(filePath)}...");
            Component_Process = ProcessCommon.StartProcessOnlyOneByName(filePath, null);
            if(Component_Process is not null)
            {
                Component_Timer = TimerStarter.CreatCountDownTimer(countDownSeconds, countDownColor, warningSeconds, warningColor, timerInterval, IsUIControlActived, CountDown_ZeroEvent, TimerClosing_Event, TimerTick_Event);
                Component_Timer.StartOrStop();
                Component_Process.Exited += Component_Process_Exited;//此方法触发条件：设定ps.EnableRaisingEvents = true;
            }
        }

        private void Component_Process_Exited(object? sender, EventArgs e)
        {
            Component_Timer?.Close();
            if (Component_Timer != null)
                Component_Timer = null;
        }

        private void CountDown_ZeroEvent(object? sender, EventArgs e)
        {
            if (IsZeroEventActived)
            {
                progress?.Report($"|执行0时刻事件|关闭APP...");
                if (Component_Process != null)
                    ProcessCommon.CloseProcessByName(Component_Process.ProcessName);
            }
            else
            { }
        }
        private void TimerClosing_Event(object? sender, int e)
        {
            if (e == 0) //0时刻时直接执行0时刻事件
                return;
            progress?.Report($"|执行关闭计时器事件|关闭APP，剩余时间：{e}s...");
            if (Component_Process != null)
                ProcessCommon.CloseProcessByName(Component_Process.ProcessName);
        }
        private void TimerTick_Event(object? sender, int e)
        {
            if (e == 0) //0时刻时直接执行0时刻事件
                return;
            progress?.Report($"|计时器运行中|剩余时间：{e}s...");           
        }
    }
}
