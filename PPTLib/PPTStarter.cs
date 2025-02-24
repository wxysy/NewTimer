using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PPTLib.Functions;

namespace PPTLib
{
    public class PPTStarter
    {
        /// <summary>
        /// 创建PPT操作类
        /// </summary>
        /// <param name="pptShowBegin">PPT开始放映事件</param>
        /// <param name="pptShowEnd">PPT结束放映事件</param>
        /// <returns>PPTPlay实例</returns>
        public static PPTPlay CreatPPTPlay(EventHandler? pptShowBegin, EventHandler? pptShowEnd)
        {
            PPTPlay pptPlay = new();
            pptPlay.PPTShowBegin += pptShowBegin;
            pptPlay.PPTShowEnd += pptShowEnd;
            return pptPlay;
        }
    }
}
