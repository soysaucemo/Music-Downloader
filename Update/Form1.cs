using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.IO;

namespace Update
{
    public partial class Form1 : Form
    {
        string[] args = new string[2];
        public Form1(string[] a)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            args = a;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Thread a = new Thread(Update);
            a.Start();
        }
        private void Update()
        {
            try
            {
                DownloadFile(args[0], Environment.CurrentDirectory + "\\" + "Update.zip", progressBar1);
                label1.Text = "创建临时文件夹";
                if (Directory.Exists(Environment.CurrentDirectory + "\\Update"))
                {
                    Directory.Delete(Environment.CurrentDirectory + "\\Update", true);
                }
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Update");
                label1.Text = "解压文件";
                ZipFile.ExtractToDirectory(Environment.CurrentDirectory + "\\" + "Update.zip", Environment.CurrentDirectory + "\\Update");
                label1.Text = "转移文件";
                DirectoryInfo d = new DirectoryInfo(Environment.CurrentDirectory + "\\Update");
                foreach (FileInfo f in d.GetFiles())
                {
                    f.CopyTo(Environment.CurrentDirectory + "\\" + Path.GetFileName(f.FullName), true);
                }
                File.Delete(Environment.CurrentDirectory + "\\" + "Update.zip");
                Directory.Delete(Environment.CurrentDirectory + "\\Update", true);
                label1.Text = "更新完成";
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message+e.InnerException);
            }
        }
        public void DownloadFile(string URL, string filename, System.Windows.Forms.ProgressBar prog)
        {
            float percent = 0;
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                long totalBytes = myrp.ContentLength;
                if (prog != null)
                {
                    prog.Maximum = (int)totalBytes;
                }
                System.IO.Stream st = myrp.GetResponseStream();
                System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                long totalDownloadedByte = 0;
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte;
                    so.Write(by, 0, osize);
                    if (prog != null)
                    {
                        prog.Value = (int)totalDownloadedByte;
                    }
                    osize = st.Read(by, 0, (int)by.Length);
                    percent = (float)totalDownloadedByte / (float)totalBytes * 100;
                }
                so.Close();
                st.Close();
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
