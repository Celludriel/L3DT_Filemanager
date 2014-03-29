using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager
{
    public abstract class AbstractManager
    {
        protected Boolean debug = false;

        protected StreamWriter writeLog = null;

        protected void debugLine(string debugLine)
        {
            if (debug)
            {
                writeLog.WriteLine(debugLine);
            }
        }

        public void setDebug(Boolean state, string debugFile)
        {
            this.debug = state;
            if (state)
            {
                writeLog = new StreamWriter(new FileStream(@debugFile, FileMode.Create));
            }
            else
            {
                if (writeLog != null)
                {
                    writeLog.Close();
                    writeLog = null;
                }
            }
        }
    }
}
