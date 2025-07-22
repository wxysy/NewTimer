using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TimerLib.Functions;

namespace TimerLib
{
    public class TimerStarter
    {
        /// <summary>
        /// 创建倒计时器
        /// </summary>
        /// <param name="countDownSeconds">倒计时时间(s)</param>
        /// <param name="countDownColor">倒计时颜色</param>
        /// <param name="warningSeconds">告警时间(s)</param>
        /// <param name="warningColor">告警颜色</param>
        /// <param name="timerInterval">刷新频率(s)</param>
        /// <param name="allowUIOperation">是否允许UI界面操作</param>
        /// <param name="zeroEvent">0时刻动作(除停止计时器和关闭窗体外的)</param>
        /// <param name="timerWindowClosedEvent">倒计时器窗体在关闭之后的操作(e:剩余秒数，若0时刻关闭也会引发此事件)</param>
        /// <param name="timerTickEvent">计时器每次Tick时的额外操作(不用在此类中编写倒计时变化，0时刻会引发专门的0时刻事件，0时刻不会引发此事件)</param>
        /// <returns>倒计时器实例</returns>
        public static CountDownTimer CreatCountDownTimer(int countDownSeconds, Brush countDownColor, int warningSeconds, Brush warningColor, int timerInterval, bool allowUIOperation, EventHandler<int>? timerTickEvent, EventHandler? zeroEvent, EventHandler<int>? timerWindowClosedEvent)
        {
            CountDownTimer countDown = new(countDownSeconds, countDownColor, warningSeconds, warningColor, timerInterval, allowUIOperation);
            countDown.CDT_TimerTickEvent += timerTickEvent;
            countDown.CDT_ZeroEvent += zeroEvent;
            countDown.CDT_TimerWindowClosedEvent += timerWindowClosedEvent;
            return countDown;
        }
    }
}
