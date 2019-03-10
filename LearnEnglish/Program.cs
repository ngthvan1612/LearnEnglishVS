using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LearnEnglish
{
    static class Program
    {
        public static int MODULE_MOD;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Random r = new Random();
            MODULE_MOD = r.Next();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
