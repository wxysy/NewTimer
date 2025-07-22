using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TimerLib;
using TimerLib.Functions;
using Windows.Media.Capture.Core;

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
                Component_Timer = TimerStarter.CreatCountDownTimer(countDownSeconds, countDownColor, warningSeconds, warningColor, timerInterval, IsUIControlActived, ACD_TimerTickEvent, ACD_ZeroEvent, ACD_TimerWindowClosedEvent);
                Component_Timer.StartOrStop();
                Component_Process.Exited += Component_Process_Exited;//此方法触发条件：ps.EnableRaisingEvents = true;
            }
        }

        private void Component_Process_Exited(object? sender, EventArgs e)
        {
            Component_Timer?.CloseTimerWindow();
            if (Component_Timer != null)
                Component_Timer = null;
        }       
        
        private void ACD_TimerTickEvent(object? sender, int e)
        {
            progress?.Report($"|计时器运行中|剩余时间：{e}s...");
        }
        private void ACD_ZeroEvent(object? sender, EventArgs e)
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
        private void ACD_TimerWindowClosedEvent(object? sender, int e)
        {
            progress?.Report($"|执行关闭计时器事件|关闭APP，剩余时间：{e}s...");
            if (Component_Process != null)
                ProcessCommon.CloseProcessByName(Component_Process.ProcessName);
        }
    }
}
