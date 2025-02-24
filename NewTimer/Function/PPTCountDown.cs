using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using PPTLib;
using PPTLib.Functions;
using TimerLib;
using TimerLib.Functions;

namespace NewTimer.Function
{
    public class PPTCountDown
    {
        #region 属性和字段
        IProgress<string>? progress;

        public PPTPlay Component_PPTPlay { get; }
        public CountDownTimer Component_Timer { get;}
        #endregion

        /// <summary>
        /// 创建整个PPT控制及倒计时模块
        /// </summary>
        /// <param name="countDownSeconds">倒计时时间(s)</param>
        /// <param name="countDownColor">倒计时颜色</param>
        /// <param name="warningSeconds">告警时间(s)</param>
        /// <param name="warningColor">告警颜色</param>
        /// <param name="timerInterval">刷新频率(s)</param>
        /// <param name="pg">进度状态指示</param>
        public PPTCountDown(int countDownSeconds, Brush countDownColor, int warningSeconds, Brush warningColor, int timerInterval, IProgress<string>? pg)
        {
            this.progress = pg;
            Component_Timer = TimerStarter.CreatCountDownTimer(countDownSeconds, countDownColor, warningSeconds, warningColor, timerInterval, false, CountDown_ZeroEvent, TimerClosing_Event, TimerTick_Event);
            Component_PPTPlay = PPTStarter.CreatPPTPlay(PPTShowBegin_Event, PPTShowBegin_End);
        }

        private void CountDown_ZeroEvent(object? sender, EventArgs e)
        {
            progress?.Report($"0时刻事件引发");
            Component_PPTPlay.PPTClose();
        }

        private void TimerClosing_Event(object? sender, int e)
        {
            if (e == 0) //0时刻时直接执行0时刻事件
                return;
            progress?.Report($"剩余时间：{e}s");
            Component_PPTPlay.PPTClose();
        }

        private void TimerTick_Event(object? sender, int e)
        {
            progress?.Report($"剩余时间：{e}s");
        }

        private void PPTShowBegin_Event(object? sender, EventArgs e)
        {
            Component_Timer.StartOrStop();
        }

        private void PPTShowBegin_End(object? sender, EventArgs e)
        {
            Component_Timer.Close();
        }

        /// <summary>
        /// 打开PPT并启动倒计时窗体
        /// </summary>
        /// <param name="filePath">PPT文件路径</param>
        public void PPTOpen(string filePath)
        {          
            Component_PPTPlay.PPTOpen(filePath);
        }

        /// <summary>
        /// 关闭PPT并关闭倒计时窗体
        /// </summary>
        public void PPTClose()
        {
            Component_PPTPlay.PPTClose();
        }
    }
}
