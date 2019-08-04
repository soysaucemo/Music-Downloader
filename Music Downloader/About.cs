using System;
using System.Diagnostics;
using System.Windows.Forms;
namespace Music_Downloader
{
    public partial class About : MetroFramework.Forms.MetroForm
    {
        public About(string version, string latestversion)
        {
            InitializeComponent();
            linkLabel4.Text = "当前版本：" + version;
            linkLabel5.Text = "最新版本：" + latestversion;
        }
        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText("1024028162");
        }
        private void About_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1.about = null;
        }
        private void LinkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText("348846978");
        }
        private void LinkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.nitian1207.top/2019/08/01/Music-Downloader/");
        }
        private void LinkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.52pojie.cn/thread-983304-1-1.html");
        }
        private void MetroButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
