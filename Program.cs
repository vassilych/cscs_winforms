using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using SplitAndMerge;

namespace WindowsFormsCSCS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            CSCS_GUI.TheForm = new Form1();
            string scriptFilename = "../../CSCS/wingui.cscs";
            CSCS_GUI.RunScript(scriptFilename);

            Application.Run(CSCS_GUI.TheForm);
        }

    }
}
