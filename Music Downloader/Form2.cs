using System.Collections;
using System.IO;
using System.Windows.Forms;
namespace Music_Downloader
{
    public partial class Form2 : MetroFramework.Forms.MetroForm
    {
        public Form2()
        {
            InitializeComponent();
        }
        public void Exchange(string dir)
        {
            string[] files = Directory.GetFiles(dir);
            ArrayList filesname = new ArrayList();
            string[] b = new string[2];
            string c;
            string newname;
            foreach (string a in files)
            {
                if (Path.GetFileName(a).IndexOf('-') != -1)
                {
                    filesname.Add(Path.GetFileName(a));
                }
            }
            for (int i = 0; i < filesname.Count; i++)
            {
                try
                {
                    b = filesname[i].ToString().Split('-');
                    c = b[1].Replace(".mp3", "");
                    newname = c.Replace(" ", "") + " - " + b[0].Replace(" ", "") + ".mp3";
                    //FileInfo f = new FileInfo(filesname[i].ToString());
                    if (dir.Substring(dir.Length - 1) == "\\")
                    {
                        FileInfo f = new FileInfo(dir + filesname[i].ToString());
                        f.MoveTo(dir + newname);
                    }
                    else
                    {
                        FileInfo f = new FileInfo(dir + "\\" + filesname[i].ToString());
                        f.MoveTo(dir + "\\" + newname);
                    }
                }
                catch
                {
                }
            }
        }
        public void Classify(string dir)
        {
            string[] files = Directory.GetFiles(dir);
            ArrayList filesname = new ArrayList();
            string singer;
            string songname;
            string newdir;
            foreach (string a in files)
            {
                if (Path.GetFileName(a).IndexOf('-') != -1)
                {
                    filesname.Add(Path.GetFileName(a));
                }
            }
            for (int i = 0; i < filesname.Count; i++)
            {
                try
                {
                    songname = filesname[i].ToString().Substring(0, filesname[i].ToString().LastIndexOf('-')).Replace(" ", "");
                    singer = filesname[i].ToString().Substring(filesname[i].ToString().LastIndexOf('-') + 1, filesname[i].ToString().LastIndexOf('.') - filesname[i].ToString().LastIndexOf('-') - 1).Replace(" ", "");
                    if (dir.Substring(dir.Length - 1) == "\\")
                    {
                        newdir = dir + singer;
                        FileInfo f = new FileInfo(dir + filesname[i].ToString());
                        if (!Directory.Exists(newdir))
                        {
                            Directory.CreateDirectory(newdir);
                        }
                        f.MoveTo(newdir + "\\" + filesname[i].ToString());
                    }
                    else
                    {
                        newdir = dir + "\\" + singer;
                        FileInfo f = new FileInfo(dir + "\\" + filesname[i].ToString());
                        if (!Directory.Exists(newdir))
                        {
                            Directory.CreateDirectory(newdir);
                        }
                        f.MoveTo(newdir + "\\" + filesname[i].ToString());
                    }
                }
                catch
                {
                }
            }
        }
        private void MetroButton4_Click(object sender, System.EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            textBox1.Text = fbd.SelectedPath;
        }
        private void MetroButton3_Click(object sender, System.EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            textBox2.Text = fbd.SelectedPath;
        }
        private void MetroButton1_Click(object sender, System.EventArgs e)
        {
            if (textBox1.Text == null || textBox1.Text == "" || !Directory.Exists(textBox1.Text))
            {
                MessageBox.Show("目录有误", caption: "警告：");
                return;
            }
            Exchange(textBox1.Text);
        }
        private void MetroButton2_Click(object sender, System.EventArgs e)
        {
            if (textBox2.Text == null || textBox2.Text == "" || !Directory.Exists(textBox2.Text))
            {
                MessageBox.Show("目录有误", caption: "警告：");
                return;
            }
            Classify(textBox2.Text);
        }
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Music_Downloader.Form1.f2 = null;
        }
        private void Form2_Load(object sender, System.EventArgs e)
        {
            textBox1.Text = Form1.mainform.Controls["DownloadPathtextBox"].Text;
            textBox2.Text = Form1.mainform.Controls["DownloadPathtextBox"].Text;
        }
    }
}
