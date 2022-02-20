using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HaritaOrnek
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

            var thread = new Thread(ThreadStart);

            thread.TrySetApartmentState(ApartmentState.STA);
            thread.Start();

            Application.Run(new Form1());
        }
        private static void ThreadStart()
        {
            Application.Run(new LoginForm()); // <-- other form started on its own UI thread
        }
    }
}
