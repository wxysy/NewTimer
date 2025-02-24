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
        public static PPTPlay CreatPPTPlay(EventHandler? pptShowBegin, EventHandler? pptShowEnd)
        {
            PPTPlay pptPlay = new();
            pptPlay.PPTShowBegin += pptShowBegin;
            pptPlay.PPTShowEnd += pptShowEnd;
            return pptPlay;
        }
    }
}
