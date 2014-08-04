using L3dtFileManager.L3dt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace L3dtFileManager
{
    class Run
    {
        private static String pathToTestFile = "C:\\Git\\Repos\\L3DT_Filemanager\\L3dtFileManager";

        static void Main()
        {
            string fileName = pathToTestFile + "\\testfile\\l3dt_hf_level\\l3dttest.proj";
            L3dtFileManager manager = new L3dtFileManager();
            L3dtFile result = manager.loadL3dtProject(fileName);
        }
    }
}
