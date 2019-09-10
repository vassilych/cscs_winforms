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

            CSCS_SQL.Init();
            CSCS_GUI.TheForm = new Form1();
            CSCS_GUI.RunScript("../../scripts/wingui.cscs");

            Application.Run(CSCS_GUI.TheForm);
        }

    }
}
