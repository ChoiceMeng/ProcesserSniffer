using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace ProcesserSniffer
{
    public class ProgramInfo
    {
        public Process mProcess = null;
        public string mstrPath;
        public int mnHours, mnMinus, mnSeconds;
        public string szWorkDir;
        public string szLastRun;
        public string szShowName;
        public string Name
        {
            get {
                if (!string.IsNullOrEmpty(szShowName))
                    return szShowName;

                string rt = mstrPath.Substring(szWorkDir.Length + 1, mstrPath.Length - (szWorkDir.Length + 1));
                
                return rt;
            }
        }

        public ProgramInfo()
        {
        }
    }
}
