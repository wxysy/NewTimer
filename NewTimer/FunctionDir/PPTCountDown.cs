using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Infrastructure.Files.FileCommon;
using PPTLib;
using PPTLib.Functions;
using TimerLib;
using TimerLib.Functions;

namespace NewTimer.FunctionDir
{
    public class PPTCountDown(int countDownSeconds, Brush countDownColor, int warningSeconds, Brush warningColor, int timerInterval, IProgress<string>? pg)
    {
        #region 属性和字段
        IProgress<string>? progress = pg;
        public PPTPlay? Component_PPTPlay { get; private set; }
        public CountDownTimer? Component_Timer { get; private set; }
        public bool IsZeroEventActived { get; set; } = true;
        public bool IsUIControlActived { get; set; } = false;
        #endregion

        /// <summary>
        /// 打开PPT并启动倒计时窗体
        /// </summary>
        /// <param name="filePath">PPT文件路径</param>
        public void PPTOpen(string filePath)
        {
            PPTPlay.ClosePPTProgram("POWERPNT");//必须有此句，终于找到取消0时刻事件后，再启动ppt显示两个计时器的原因了。本质上不是两个计时器，而是有两个PPT程序。 

            progress?.Report($"|打开PPT|{Path.GetFileName(filePath)}...");
            Component_PPTPlay = PPTStarter.CreatPPTPlay(PPTShowBegin_Event, PPTShowBegin_End);
            Component_PPTPlay.PPTOpen(filePath);
        }
        private void PPTShowBegin_Event(object? sender, EventArgs e)
        {
            Component_Timer = TimerStarter.CreatCountDownTimer(countDownSeconds, countDownColor, warningSeconds, warningColor, timerInterval, IsUIControlActived, CountDown_ZeroEvent, TimerClosing_Event, TimerTick_Event);
            Component_Timer.StartOrStop();
        }
        private void PPTShowBegin_End(object? sender, EventArgs e)
        {
            Component_Timer?.Close();
            if (Component_Timer != null)
                Component_Timer = null;
        }
        private void CountDown_ZeroEvent(object? sender, EventArgs e)
        {
            if (IsZeroEventActived)
            {
                progress?.Report($"|执行0时刻事件|关闭PPT...");
                Component_PPTPlay?.PPTClose();
            }
            else
            { }
        }
        private void TimerClosing_Event(object? sender, int e)
        {
            if (e == 0) //0时刻时直接执行0时刻事件
                return;
            progress?.Report($"|执行关闭计时器事件|关闭PPT，剩余时间：{e}s...");
            Component_PPTPlay?.PPTClose();
        }
        private void TimerTick_Event(object? sender, int e)
        {
            progress?.Report($"|计时器运行中|剩余时间：{e}s...");
        }
    }
}
