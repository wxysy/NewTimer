using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using PPTLib.Functions;
using TimerLib;
using TimerLib.Functions;

namespace PPTLib
{
    public class PPTCountDown
    {
        #region 属性和字段
        IProgress<string>? progress;
        PPTPlay pptPlay;
        CountDown timer;
        #endregion

        public PPTCountDown(IProgress<string>? pg)
        {
            progress = pg;
            timer = TimerStarter.CreatCountDownTimer(12, Brushes.Blue, 5, Brushes.Red, 1, false, CountDown_ZeroEvent, TimerClose_Event, TimerTick_Event);
            pptPlay = PPTStarter.CreatPPTPlay(PPTShowBegin_Event, PPTShowBegin_End);
        }

        private void CountDown_ZeroEvent(object? sender, EventArgs e)
        {
            progress?.Report($"0时刻事件引发");
            pptPlay.PPTClose();
        }

        private void TimerClose_Event(object? sender, int e)
        {
            if (e == 0) //0时刻时直接执行0时刻事件
                return;
            progress?.Report($"剩余时间：{e}s");
            pptPlay.PPTClose();
        }

        private void TimerTick_Event(object? sender, int e)
        {
            progress?.Report($"剩余时间：{e}s");
        }

        private void PPTShowBegin_Event(object? sender, EventArgs e)
        {
            timer.StartOrStop();
        }

        private void PPTShowBegin_End(object? sender, EventArgs e)
        {
            timer.Close();
        }

        public void PPTOpen(string filePath)
        {          
            pptPlay?.PPTOpen(filePath);
        }

        public void PPTClose()
        {
            pptPlay?.PPTClose();
        }
    }
}
