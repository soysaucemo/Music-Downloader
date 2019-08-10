using System;
using System.Windows.Forms;
using System.IO.Compression;
namespace Update
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length == 2)
            {
                Application.Run(new Form1(args));
            }
            else
            {
                MessageBox.Show(args.Length.ToString());
                Application.Exit();
            }
        }
    }
}
