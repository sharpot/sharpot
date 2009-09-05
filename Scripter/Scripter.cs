using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Scripter
{
    //This is all being coded as psuedo ATM, this isnt made to be functional yet
 
    class Scripter
    {
        List<Script> Scripts;
        public bool RaiseEvent(string EventName, object[] Args)
        {
            foreach (Script S in Scripts) 
            {

            }
            return true;
        }
    }
}
