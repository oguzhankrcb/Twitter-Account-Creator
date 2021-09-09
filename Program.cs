using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Twitter_Account_Creator
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
            //Application.Run(new Form1());
             Start();
        }

        public static void Start()   // <-- must be marked public!
        {
            //Application.Run(new Form1());
            Form1 frm = new Form1();
            frm.ShowDialog();
        }
    }
}
