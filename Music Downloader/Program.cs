using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Music_Downloader
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            /*
            string[] dlls = { "AxInterop.WMPLib.dll", "CSkin.dll", "ID3.dll", "Interop.WMPLib.dll", "MetroFramework.Design.dll", "MetroFramework.dll", "MetroFramework.Fonts.dll", "Newtonsoft.Json.dll" };
            foreach (string a in dlls)
            {
                if (!File.Exists(Environment.CurrentDirectory + "\\" + a))
                {
                    MessageBox.Show("缺少DLL", caption: "警告:");
                    Environment.Exit(0);
                }
            }
            */
            Application.Run(new Form1());
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            MessageBox.Show(e.ExceptionObject.ToString());
        }
    }
}
