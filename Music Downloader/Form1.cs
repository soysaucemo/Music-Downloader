using ID3;
using ID3.ID3v2Frames.BinaryFrames;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using WMPLib;
namespace Music_Downloader
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            this.StartPosition = FormStartPosition.CenterScreen;
        }
        private List<SearchResult> Searchresult;
        private ArrayList downloadindices = new ArrayList();
        private Thread a;
        private List<PlayList> pl = new List<PlayList>();
        private string playmode = "shunxu";
        private LrcDetails lrcd = new LrcDetails();
        public string latestversion = "获取中";
        private string ver = "1.4.0";
        private List<Thread> downloadthreadlist = new List<Thread>();
        private ArrayList canceldownloadindex = new ArrayList();
        private string latestversionurl;
        public static Form2 f2;
        public static About about;
        public static Form1 mainform;
        private double Timec = 0; //s
        //API 1网易云 2酷狗 3QQ 4酷我 5百度
        public List<SearchResult> GetMusiclistJson(string id, int musicapicode)
        {
            string url = null;
            WebClient wc = new WebClient();
            Stream stream;
            StreamReader sr = null;
            if (musicapicode == 1)
            {
                string left = "playlist?id=";
                if (id.IndexOf(left) != -1)
                {
                    if (id.IndexOf("&userid") != -1)
                    {
                        url = "https://v1.itooi.cn/netease/songList?id=" + GetMidText(id, left, "&userid");
                    }
                    else
                    {
                        url = "https://v1.itooi.cn/netease/songList?id=" + id.Substring(id.IndexOf(left) + left.Length);
                    }
                }
                else
                {
                    url = "https://v1.itooi.cn/netease/songList?id=" + id;
                }
                try
                {
                    stream = wc.OpenRead(url);
                    sr = new StreamReader(stream);
                    NeteaseMusiclist.NeateaseMusicList json = JsonConvert.DeserializeObject<NeteaseMusiclist.NeateaseMusicList>(sr.ReadToEnd());
                    List<SearchResult> re = new List<SearchResult>();
                    for (int i = 0; i < json.data.tracks.Count; i++)
                    {
                        SearchResult s = new SearchResult
                        {
                            id = json.data.tracks[i].id.ToString(),
                            Album = json.data.tracks[i].album.name,
                            lrcurl = "https://v1.itooi.cn/netease/lrc?id=" + json.data.tracks[i].id.ToString(),
                            url = "https://v1.itooi.cn/netease/url?id=" + json.data.tracks[i].id.ToString(),
                            SongName = json.data.tracks[i].name,
                            SingerName = json.data.tracks[i].artists[0].name
                        };
                        re.Add(s);
                    }
                    return re;
                }
                catch
                {
                    return null;
                }
            }
            if (musicapicode == 2)
            {
                url = "https://v1.itooi.cn/kugou/songList?id=" + id + "&pageSize=100&page=0";
                if (id.IndexOf("https://www.kugou.com") != -1)
                {
                    url = "https://v1.itooi.cn/kugou/songList?id=" + GetMidText(id, "/single/", ".html") + "&pageSize=100&page=0";
                }
                try
                {
                    stream = wc.OpenRead(url);
                    sr = new StreamReader(stream);
                    string ss = sr.ReadToEnd();
                    KugouMusicList.KugouMusicList json = JsonConvert.DeserializeObject<KugouMusicList.KugouMusicList>(ss);
                    List<SearchResult> re = new List<SearchResult>();
                    for (int i = 0; i < json.data.info.Count; i++)
                    {
                        string[] n = json.data.info[i].filename.Replace(" ", "").Split('-');
                        SearchResult s = new SearchResult
                        {
                            id = json.data.info[i].hash,
                            Album = json.data.info[i].remark,
                            lrcurl = "https://v1.itooi.cn/netease/lrc?id=" + json.data.info[i].hash,
                            url = "https://v1.itooi.cn/netease/url?id=" + json.data.info[i].hash,
                            SongName = n[1],
                            SingerName = n[0]
                        };
                        re.Add(s);
                    }
                    return re;
                }
                catch
                {
                    return null;
                }
            }
            if (musicapicode == 3)
            {
                if (id.IndexOf("http://url.cn/") != -1 || id.IndexOf("https://c.y.qq.com/") != -1)
                {
                    string qqid = GetRealUrl(id);
                    url = "https://v1.itooi.cn/tencent/songList?id=" + qqid.Substring(qqid.IndexOf("id=") + 3) + "&pageSize=100&page=0";
                }
                else
                {
                    if (id.IndexOf("/playlist/") != -1)
                    {
                        url = "https://v1.itooi.cn/tencent/songList?id=" + GetMidText(id, "/playlist/", ".html") + "&pageSize=100&page=0";
                    }
                    else
                    {
                        url = "https://v1.itooi.cn/tencent/songList?id=" + id + "&pageSize=100&page=0";
                    }
                }
                try
                {
                    stream = wc.OpenRead(url);
                    sr = new StreamReader(stream);
                    QQMusicList.QQMusicList json = JsonConvert.DeserializeObject<QQMusicList.QQMusicList>(sr.ReadToEnd());
                    List<SearchResult> re = new List<SearchResult>();
                    string sn = "";
                    for (int i = 0; i < json.data[0].songlist.Count; i++)
                    {
                        for (int x = 0; x < json.data[0].songlist[i].singer.Count; x++)
                        {
                            if (json.data[0].songlist[i].singer.Count - x == 1)
                            {
                                sn += json.data[0].songlist[i].singer[x].name;
                            }
                            else
                            {
                                sn += json.data[0].songlist[i].singer[x].name + "、";
                            }
                        }
                        SearchResult s = new SearchResult
                        {
                            id = json.data[0].songlist[i].mid,
                            Album = json.data[0].songlist[i].album.name,
                            lrcurl = "https://v1.itooi.cn/tencent/lrc?id=" + json.data[0].songlist[i].mid,
                            url = "https://v1.itooi.cn/tencent/url?id=" + json.data[0].songlist[i].mid,
                            SongName = json.data[0].songlist[i].name,
                            SingerName = sn
                        };
                        sn = "";
                        re.Add(s);
                    }
                    return re;
                }
                catch
                {
                    return null;
                }
            }
            if (musicapicode == 4)
            {
                if (id.IndexOf("http://") != -1)
                {
                    if (id.IndexOf("?channelId") != -1)
                    {
                        GetMidText(id, "playlist/", "?channelId");
                    }
                    else
                    {
                        string[] a = id.Split('/');
                        url = "https://v1.itooi.cn/kuwo/songList?id=" + a[a.Length] + "&pageSize=100&page=0";
                    }
                }
                else
                {
                    url = "https://v1.itooi.cn/kuwo/songList?id=" + id + "&pageSize=100&page=0";
                }
                try
                {
                    stream = wc.OpenRead(url);
                    sr = new StreamReader(stream);
                    KuwoMusiclist.KuwoMusicList json = JsonConvert.DeserializeObject<KuwoMusiclist.KuwoMusicList>(sr.ReadToEnd());
                    List<SearchResult> re = new List<SearchResult>();
                    for (int i = 0; i < json.data.musiclist.Count; i++)
                    {
                        SearchResult s = new SearchResult
                        {
                            id = json.data.musiclist[i].id,
                            Album = json.data.musiclist[i].album,
                            lrcurl = "https://v1.itooi.cn/netease/lrc?id=" + json.data.musiclist[i].id,
                            url = "https://v1.itooi.cn/netease/url?id=" + json.data.musiclist[i].id,
                            SongName = json.data.musiclist[i].name,
                            SingerName = json.data.musiclist[i].artist
                        };
                        re.Add(s);
                    }
                    return re;
                }
                catch
                {
                    return null;
                }
            }
            if (musicapicode == 5)
            {
                if (id.IndexOf("http://") != -1)
                {
                    string[] a = id.Split('/');
                    url = "https://v1.itooi.cn/baidu/songList?id=" + a[a.Length];
                }
                else
                {
                    url = "https://v1.itooi.cn/baidu/songList?id=" + id;
                }
                try
                {
                    stream = wc.OpenRead(url);
                    sr = new StreamReader(stream);
                    BaiduMusiclist.BaiduMusicList json = JsonConvert.DeserializeObject<BaiduMusiclist.BaiduMusicList>(sr.ReadToEnd());
                    List<SearchResult> re = new List<SearchResult>();
                    for (int i = 0; i < json.data.Count; i++)
                    {
                        SearchResult s = new SearchResult
                        {
                            id = json.data[i].song_id,
                            Album = json.data[i].album_title,
                            lrcurl = "https://v1.itooi.cn/netease/lrc?id=" + json.data[i].song_id,
                            url = "https://v1.itooi.cn/netease/url?id=" + json.data[i].song_id,
                            SongName = json.data[i].title,
                            SingerName = json.data[i].author
                        };
                        re.Add(s);
                    }
                    return re;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
        /// <summary>
        /// 取文本中间
        /// </summary>
        /// <param name="text">源文本</param>
        /// <param name="left">前文本</param>
        /// <param name="right">后文本</param>
        /// <returns></returns>
        public string GetMidText(string text, string left, string right)
        {
            try
            {
                int leftnum = text.IndexOf(left);
                int rightnum = text.IndexOf(right);
                return text.Substring(leftnum + left.Length, rightnum - (leftnum + left.Length));
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        public string GetRealUrl(string url)
        {
            HttpWebRequest wbrequest = (HttpWebRequest)WebRequest.Create(url);
            wbrequest.Method = "GET";
            wbrequest.KeepAlive = true;
            HttpWebResponse wbresponse = (HttpWebResponse)wbrequest.GetResponse();
            return wbresponse.ResponseUri.ToString();
        }
        public void GetMusicListThread(object id)
        {
            if ((string)id == null || (string)id == "")
            {
                MessageBox.Show("ID不能为空", caption: "警告：");
                metroButton1.Enabled = true;
                metroButton2.Enabled = true;
                return;
            }
            if (id.ToString().IndexOf("qq.com") != -1)
            {
                radioButton3.Checked = true;
            }
            if (id.ToString().IndexOf("163.com") != -1)
            {
                radioButton1.Checked = true;
            }
            Searchresult = GetMusiclistJson(id.ToString(), GetApiCode());
            if (Searchresult == null)
            {
                MessageBox.Show("歌单获取错误", caption: "警告：");
                listView1.Items.Clear();
                metroButton1.Enabled = true;
                metroButton2.Enabled = true;
                return;
            }
            listView1.Items.Clear();
            for (int i = 0; i < Searchresult.Count; i++)
            {
                listView1.Items.Add(Searchresult[i].SongName);
                listView1.Items[i].SubItems.Add(Searchresult[i].SingerName);
                listView1.Items[i].SubItems.Add(Searchresult[i].Album);
            }
            Musicnumlabel.Text = "歌曲总数：" + listView1.Items.Count;
            metroButton1.Enabled = true;
            metroButton2.Enabled = true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            metroButton1.Enabled = false;
            metroButton2.Enabled = false;
            listView1.Items.Clear();
            listView1.Items.Add("获取中...");
            skinTabControl1.SelectedIndex = 0;
            a = new Thread(new ParameterizedThreadStart(GetMusicListThread));
            a.Start(IDtextBox.Text);
        }
        public string NameCheck(string name)
        {
            string re = name.Replace("*", " ");
            re = re.Replace("\\", " ");
            re = re.Replace("\"", " ");
            re = re.Replace("<", " ");
            re = re.Replace(">", " ");
            re = re.Replace("|", " ");
            re = re.Replace("?", " ");
            re = re.Replace("/", ",");
            re = re.Replace(":", "：");
            return re;
        }
        public void Download(object o)
        {
            string songname = "";
            string ID = "";
            string singername = "";
            string url = "";
            string lrcurl = "";
            Stream s;
            string downloadpath = "";
            string filename = "";
            List<DownloadList> dl = (List<DownloadList>)o;
            int listviewindicesnum = listView3.Items.Count;
            for (int i = 0; i < dl.Count; i++)
            {
                songname = NameCheck(dl[i].Songname);
                ID = dl[i].ID;
                singername = NameCheck(dl[i].Singername);
                downloadpath = dl[i].Savepath;
                WebClient wb = new WebClient();
                url = dl[i].Url + "&quality=" + dl[i].DownloadQuality;
                s = wb.OpenRead(url + "&isRedirect=0");
                StreamReader sr = new StreamReader(s);
                if (sr.ReadToEnd().IndexOf(".flac") != -1)
                {
                    filename = dl[i].Songname + " - " + dl[i].Singername + ".flac";
                }
                else
                {
                    filename = dl[i].Songname + " - " + dl[i].Singername + ".mp3";
                }
                if (!Ifcanceldownload(dl[i].index))
                {
                    if (dl[i].IfDownloadSong)
                    {
                        if (!File.Exists(downloadpath + "\\" + filename))
                        {
                            listView3.Items[dl[i].index].SubItems[2].Text = "下载音乐中";
                            try { wb.DownloadFile(url, downloadpath + "\\" + filename); }
                            catch (Exception e)
                            {
                                if (File.Exists(downloadpath + "\\" + filename))
                                {
                                    File.Delete(downloadpath + "\\" + filename);
                                }
                                listView3.Items[dl[i].index].SubItems[2].Text = "音乐下载错误:" + e.Message;
                                continue;
                            }
                            listView3.Items[dl[i].index].SubItems[2].Text = "音乐下载完成";
                            try { AddMusicDetails(downloadpath + "\\" + filename, dl[i].Songname, dl[i].Singername, dl[i].Album, GetPicUrl(dl[i].ID, dl[i].Api), downloadpath, dl[i].ifdownloadpic); } catch { }
                        }
                        else
                        {
                            listView3.Items[dl[i].index].SubItems[2].Text = "音乐已存在";
                        }
                    }
                    if (dl[i].IfDownloadlrc && dl[i].LrcUrl != null && dl[i].LrcUrl != "")
                    {
                        try
                        {
                            listView3.Items[dl[i].index].SubItems[2].Text += "下载歌词中";
                            if (!File.Exists(downloadpath + "\\" + songname + " - " + singername + ".lrc"))
                            {

                                lrcurl = dl[i].LrcUrl;
                                s = wb.OpenRead(lrcurl);
                                StreamReader sr1 = new StreamReader(s);
                                File.WriteAllText(downloadpath + "\\" + songname + " - " + singername + ".lrc", sr1.ReadToEnd(), Encoding.Default);

                            }
                        }
                        catch (Exception e)
                        {
                            listView3.Items[dl[i].index].SubItems[2].Text += "歌词下载错误:" + e.Message;
                        }
                        listView3.Items[dl[i].index].SubItems[2].Text += "歌词下载完成";
                    }
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            if (!string.IsNullOrEmpty(fbd.SelectedPath))
            {
                DownloadPathtextBox.Text = fbd.SelectedPath;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Thread thread_update = new Thread(update);
            thread_update.Start();
            skinTabControl1.ItemSize = new Size(0, 1);
            axWindowsMediaPlayer1.settings.volume = 50;
            string settingpath = Environment.CurrentDirectory + "\\Setting.json";
            axWindowsMediaPlayer1.settings.setMode("shuffle", false);
            mainform = this;
            skinTabControl1.SelectedIndex = 0;
            try
            {
                if (File.Exists(settingpath))
                {
                    StreamReader sr = new StreamReader(settingpath);
                    Setting s = JsonConvert.DeserializeObject<Music_Downloader.Setting>(sr.ReadToEnd());
                    pl = s.PlayList;
                    metroTrackBar2.Value = s.Volume;
                    sr.Close();
                    DownloadPathtextBox.Text = s.SavePath;
                    checkBox1.Checked = s.ifdownloadlrc;
                    checkBox3.Checked = s.ifdownloadpic;
                    metroComboBox1.SelectedItem = s.DownloadQuality;
                    metroComboBox2.SelectedIndex = s.MultiDownload;
                    switch (s.Color)
                    {
                        case 0:
                            this.Style = MetroFramework.MetroColorStyle.Black;
                            break;
                        case 1:
                            this.Style = MetroFramework.MetroColorStyle.White;
                            break;
                        case 2:
                            this.Style = MetroFramework.MetroColorStyle.Silver;
                            break;
                        case 3:
                            this.Style = MetroFramework.MetroColorStyle.Blue;
                            break;
                        case 4:
                            this.Style = MetroFramework.MetroColorStyle.Green;
                            break;
                        case 5:
                            this.Style = MetroFramework.MetroColorStyle.Lime;
                            break;
                        case 6:
                            this.Style = MetroFramework.MetroColorStyle.Teal;
                            break;
                        case 7:
                            this.Style = MetroFramework.MetroColorStyle.Orange;
                            break;
                        case 8:
                            this.Style = MetroFramework.MetroColorStyle.Brown;
                            break;
                        case 9:
                            this.Style = MetroFramework.MetroColorStyle.Pink;
                            break;
                        case 10:
                            this.Style = MetroFramework.MetroColorStyle.Magenta;
                            break;
                        case 11:
                            this.Style = MetroFramework.MetroColorStyle.Purple;
                            break;
                        case 12:
                            this.Style = MetroFramework.MetroColorStyle.Red;
                            break;
                        case 13:
                            this.Style = MetroFramework.MetroColorStyle.Yellow;
                            break;
                    }
                    metroComboBox3.SelectedIndex = s.Color;
                    IWMPPlaylist l = axWindowsMediaPlayer1.currentPlaylist;
                    for (int i = 0; i < s.PlayList.Count; i++)
                    {
                        IWMPMedia media = axWindowsMediaPlayer1.newMedia(s.PlayList[i].Url);
                        l.appendItem(media);
                        listView2.Items.Add(s.PlayList[i].SongName);
                        listView2.Items[i].SubItems.Add(s.PlayList[i].SingerName);
                        listView2.Items[i].SubItems.Add(s.PlayList[i].Album);
                    }
                    axWindowsMediaPlayer1.currentPlaylist = l;
                    axWindowsMediaPlayer1.Ctlcontrols.stop();
                }
                else
                {
                    DownloadPathtextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    metroComboBox1.SelectedIndex = 4;
                    metroComboBox2.SelectedIndex = 0;
                    metroComboBox3.SelectedIndex = 5;
                    About aboutform = new About(ver, latestversion);
                    aboutform.ShowDialog();
                }
            }
            catch
            {
            }
        }
        private void IDtextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                button1_Click(this, new EventArgs());
            }
        }
        private void SearchtextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Searchbutton_Click(this, new EventArgs());
            }
        }
        public class SearchResult
        {
            public string SongName;
            public string SingerName;
            public string Album;
            public string url;
            public string lrcurl;
            public string id;
        }
        /// <summary>
        /// 音乐搜索
        /// </summary>
        /// <param name="key">关键词</param>
        /// <param name="api"></param>
        /// <returns></returns>
        public List<SearchResult> SearchMusic(string key, int api, string quality)
        {
            string url = null;
            List<SearchResult> re = new List<SearchResult>();
            if (api == 1)
            {
                url = "https://v1.itooi.cn/netease/search?keyword=" + key + "&type=song&pageSize=100&page=0"; //网易云音乐接口
                try
                {
                    WebClient wc = new WebClient();
                    Stream stream = wc.OpenRead(url);
                    StreamReader sr = new StreamReader(stream);
                    Music_Downloader.Netease.NeteaseSearchRoot root = JsonConvert.DeserializeObject<Music_Downloader.Netease.NeteaseSearchRoot>(sr.ReadToEnd());
                    string sn = "";
                    for (int i = 0; i < root.data.songs.Count; i++)
                    {
                        for (int x = 0; x < root.data.songs[i].ar.Count; x++)
                        {
                            if (root.data.songs[i].ar.Count - x == 1)
                            {
                                sn += root.data.songs[i].ar[x].name;
                            }
                            else
                            {
                                sn += root.data.songs[i].ar[x].name + "、";
                            }
                        }
                        SearchResult s = new SearchResult
                        {
                            SongName = root.data.songs[i].name,
                            SingerName = sn,
                            Album = root.data.songs[i].al.name,
                            id = root.data.songs[i].id.ToString(),
                            url = "https://v1.itooi.cn/netease/url?id=" + root.data.songs[i].id.ToString(),
                            lrcurl = "https://v1.itooi.cn/netease/lrc?id=" + root.data.songs[i].id.ToString()
                        };
                        sn = "";
                        re.Add(s);
                    }
                    return re;
                }
                catch
                {
                    return null;
                }
            }
            if (api == 2)
            {
                url = "https://v1.itooi.cn/kugou/search?keyword=" + key + "&type=song&pageSize=100&page=0"; //酷狗音乐接口
                try
                {
                    WebClient wc = new WebClient();
                    Stream stream = wc.OpenRead(url);
                    StreamReader sr = new StreamReader(stream);
                    Music_Downloader.Kugou.KugouSearchRoot root = JsonConvert.DeserializeObject<Music_Downloader.Kugou.KugouSearchRoot>(sr.ReadToEnd());
                    for (int i = 0; i < root.data.info.Count; i++)
                    {
                        SearchResult s = new SearchResult
                        {
                            SongName = root.data.info[i].songname,
                            SingerName = root.data.info[i].singername,
                            Album = root.data.info[i].album_name,
                            id = root.data.info[i].hash.ToString(),
                            url = "https://v1.itooi.cn/kugou/url?id=" + root.data.info[i].hash.ToString(),
                            lrcurl = "https://v1.itooi.cn/kugou/lrc?id=" + root.data.info[i].hash.ToString()
                        };
                        re.Add(s);
                    }
                    return re;
                }
                catch
                {
                    return null;
                }
            }
            if (api == 3)
            {
                url = "https://v1.itooi.cn/tencent/search?keyword=" + key + "&type=song&pageSize=100&page=0"; //QQ音乐接口
                try
                {
                    WebClient wc = new WebClient();
                    Stream stream = wc.OpenRead(url);
                    StreamReader sr = new StreamReader(stream);
                    Music_Downloader.QQ.QQSearchRoot root = JsonConvert.DeserializeObject<Music_Downloader.QQ.QQSearchRoot>(sr.ReadToEnd());
                    string sn = "";
                    for (int i = 0; i < root.data.list.Count; i++)
                    {
                        for (int x = 0; x < root.data.list[i].singer.Count; x++)
                        {
                            if (root.data.list[i].singer.Count - x == 1)
                            {
                                sn += root.data.list[i].singer[x].name;
                            }
                            else
                            {
                                sn += root.data.list[i].singer[x].name + "、";
                            }
                        }
                        SearchResult s = new SearchResult
                        {
                            SongName = root.data.list[i].songname,
                            SingerName = sn,
                            Album = root.data.list[i].albumname,
                            id = root.data.list[i].songmid,
                            url = "https://v1.itooi.cn/tencent/url?id=" + root.data.list[i].songmid,
                            lrcurl = "https://v1.itooi.cn/tencent/lrc?id=" + root.data.list[i].songmid
                        };
                        sn = "";
                        re.Add(s);
                    }
                    return re;
                }
                catch
                {
                    return null;
                }
            }
            if (api == 4)
            {
                url = "https://v1.itooi.cn/kuwo/search?keyword=" + key + "&type=song&pageSize=100&page=0"; //酷我音乐接口
                try
                {
                    WebClient wc = new WebClient();
                    Stream stream = wc.OpenRead(url);
                    StreamReader sr = new StreamReader(stream);
                    Music_Downloader.Kuwo.KuwoSearchRoot root = JsonConvert.DeserializeObject<Music_Downloader.Kuwo.KuwoSearchRoot>(sr.ReadToEnd());
                    for (int i = 0; i < root.data.Count; i++)
                    {
                        SearchResult s = new SearchResult
                        {
                            SongName = root.data[i].SONGNAME,
                            SingerName = root.data[i].ARTIST,
                            Album = root.data[i].ALBUM,
                            id = root.data[i].MUSICRID.Replace("MUSIC_", ""),
                            url = "https://v1.itooi.cn/kuwo/url?id=" + root.data[i].MUSICRID.Replace("MUSIC_", ""),
                            lrcurl = "https://v1.itooi.cn/kuwo/lrc?id=" + root.data[i].MUSICRID.Replace("MUSIC_", "")
                        };
                        re.Add(s);
                    }
                    return re;
                }
                catch
                {
                    return null;
                }
            }
            if (api == 5)
            {
                url = "https://v1.itooi.cn/baidu/search?keyword=" + key + "&type=song&pageSize=100&page=0"; //咪咕音乐接口
                try
                {
                    WebClient wc = new WebClient();
                    Stream stream = wc.OpenRead(url);
                    StreamReader sr = new StreamReader(stream);
                    Music_Downloader.Baidu.BaiduSearchRoot root = JsonConvert.DeserializeObject<Music_Downloader.Baidu.BaiduSearchRoot>(sr.ReadToEnd());
                    for (int i = 0; i < root.data.song_list.Count; i++)
                    {
                        SearchResult s = new SearchResult
                        {
                            SongName = root.data.song_list[i].title,
                            SingerName = root.data.song_list[i].author,
                            Album = root.data.song_list[i].album_title,
                            id = root.data.song_list[i].song_id,
                            url = "https://v1.itooi.cn/baidu/url?id=" + root.data.song_list[i].song_id,
                            lrcurl = "https://v1.itooi.cn/baidu/lrc?id=" + root.data.song_list[i].song_id
                        };
                        re.Add(s);
                    }
                    return re;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
        public int GetApiCode()
        {
            if (radioButton1.Checked)
            {
                return 1;
            }
            if (radioButton2.Checked)
            {
                return 2;
            }
            if (radioButton3.Checked)
            {
                return 3;
            }
            if (radioButton4.Checked)
            {
                return 4;
            }
            if (radioButton5.Checked)
            {
                return 5;
            }
            return 0;
        }
        public void SearchThread()
        {
            if (SearchtextBox.Text == null || SearchtextBox.Text == "")
            {
                MessageBox.Show("搜索内容不能为空", caption: "警告：");
                metroButton1.Enabled = true;
                metroButton2.Enabled = true;
                return;
            }
            try
            {
                Searchresult = SearchMusic(SearchtextBox.Text, GetApiCode(), metroComboBox1.SelectedItem.ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show("搜索异常:" + e.Message, caption: "警告：");
                metroButton1.Enabled = true;
                metroButton2.Enabled = true;
                return;
            }
            if (Searchresult == null || Searchresult.Count == 0)
            {
                MessageBox.Show("未搜索到相关内容", caption: "提示:");
            }
            listView1.Items.Clear();
            for (int i = 0; i < Searchresult.Count; i++)
            {
                listView1.Items.Add(Searchresult[i].SongName);
                listView1.Items[i].SubItems.Add(Searchresult[i].SingerName);
                listView1.Items[i].SubItems.Add(Searchresult[i].Album);
            }
            Musicnumlabel.Text = "歌曲总数：" + listView1.Items.Count;
            metroButton1.Enabled = true;
            metroButton2.Enabled = true;
        }
        private void Searchbutton_Click(object sender, EventArgs e)
        {
            skinTabControl1.SelectedIndex = 0;
            listView1.Items.Clear();
            listView1.Items.Add("正在搜索...");
            try
            {
                metroButton1.Enabled = false;
                metroButton2.Enabled = false;
                a = new Thread(SearchThread);
                a.Start();
            }
            catch
            {
            }
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            List<DownloadList> dl = GetDownloadLists(true, checkBox1.Checked, true);
            MultiFilesDownload(dl, metroComboBox2.SelectedIndex + 1);
        }
        public DownloadList SetDownloadMedia(int Api, string ID, bool IfDownloadlrc, bool IfDownloadSong, string Savepath, string Songname, string Singername, string Url, string LrcUrl, string Album, string DownloadQulity, int Index, bool Ifdownloadpic)
        {
            DownloadList dd = new DownloadList
            {
                Api = Api,
                ID = ID,
                IfDownloadlrc = IfDownloadlrc,
                IfDownloadSong = IfDownloadSong,
                Savepath = Savepath,
                Songname = Songname,
                Singername = Singername,
                Url = Url,
                LrcUrl = LrcUrl,
                Album = Album,
                DownloadQuality = DownloadQulity,
                index = Index,
                ifdownloadpic = Ifdownloadpic
            };
            return dd;
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            List<DownloadList> dl = GetDownloadLists(true, checkBox1.Checked);
            MultiFilesDownload(dl, metroComboBox2.SelectedIndex + 1);
        }
        private void 下载所有歌词ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<DownloadList> dl = GetDownloadLists(false, true, true);
            MultiFilesDownload(dl, metroComboBox2.SelectedIndex + 1);
        }
        private void 下载选中歌词ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<DownloadList> dl = GetDownloadLists(false, true);
            MultiFilesDownload(dl, metroComboBox2.SelectedIndex + 1);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < downloadthreadlist.Count; i++)
            {
                if (downloadthreadlist[i].IsAlive)
                {
                    if (MessageBox.Show("下载未完成,确定关闭?", caption: "提示:", buttons: MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        Setting ss = new Setting
                        {
                            SavePath = DownloadPathtextBox.Text,
                            PlayList = pl,
                            DownloadQuality = metroComboBox1.SelectedItem.ToString(),
                            Volume = metroTrackBar2.Value,
                            MultiDownload = metroComboBox2.SelectedIndex,
                            ifdownloadpic = checkBox3.Checked,
                            ifdownloadlrc = checkBox1.Checked,
                            Color = metroComboBox3.SelectedIndex
                        };
                        string json_ = JsonConvert.SerializeObject(ss);
                        StreamWriter sw_ = new StreamWriter(Environment.CurrentDirectory + "\\Setting.json");
                        sw_.Write(json_);
                        sw_.Flush();
                        sw_.Close();
                        Environment.Exit(0);
                    }
                }
            }
            Setting s = new Setting
            {
                SavePath = DownloadPathtextBox.Text,
                PlayList = pl,
                DownloadQuality = metroComboBox1.SelectedItem.ToString(),
                Volume = metroTrackBar2.Value,
                MultiDownload = metroComboBox2.SelectedIndex,
                ifdownloadpic = checkBox3.Checked,
                ifdownloadlrc = checkBox1.Checked,
                Color = metroComboBox3.SelectedIndex
            };
            string json = JsonConvert.SerializeObject(s);
            StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + "\\Setting.json");
            sw.Write(json);
            sw.Flush();
            sw.Close();
        }
        public void update()
        {
            try
            {
                WebClient wb = new WebClient();
                Stream webdata = wb.OpenRead("https://www.nitian1207.top/update/MusicDownloader.txt");
                StreamReader sr = new StreamReader(webdata);
                string data = sr.ReadToEnd();
                latestversion = data.Replace("\n", "");
                if (ver != data && ver + "\n" != data)
                {
                    if (MessageBox.Show("检测到新版本，是否更新？", caption: "提示：", buttons: MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        latestversionurl = "https://www.nitian1207.top/download//MusicDownloader" + data + ".zip";
                        Process p = new Process();
                        ProcessStartInfo ps = new ProcessStartInfo(Environment.CurrentDirectory + "\\" + "Update.exe", "\"" + latestversionurl + "\" \"" + Path.GetFullPath(Application.ExecutablePath) + "\"");
                        p.StartInfo = ps;
                        p.Start();
                        Environment.Exit(0);
                    }
                }
            }
            catch
            {
                latestversion = "获取错误";
            }
        }
        public ArrayList GetListViewSelectedIndices()
        {
            ArrayList a = new ArrayList();
            string mes = null;
            for (int i = 0; i < listView1.SelectedIndices.Count; i++)
            {
                a.Add(listView1.SelectedIndices[i]);
            }
            foreach (int i in a)
            {
                mes += i.ToString();
            }
            return a;
        }
        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", DownloadPathtextBox.Text);
        }
        private void PictureBox3_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.next();
            label8.Text = "当前音乐无歌词";
            label8.Location = new Point((424 - label8.Width) / 2, label8.Location.Y);
        }
        private void PictureBox2_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.previous();
            label8.Text = "当前音乐无歌词";
            label8.Location = new Point((424 - label8.Width) / 2, label8.Location.Y);
        }
        private void PictureBox1_Click(object sender, EventArgs e)
        {
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                axWindowsMediaPlayer1.Ctlcontrols.pause();
                pictureBox1.Image = Properties.Resources.play;
                return;
            }
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPaused)
            {
                axWindowsMediaPlayer1.Ctlcontrols.play();
                pictureBox1.Image = Properties.Resources.pause;
                return;
            }
            if (axWindowsMediaPlayer1.currentPlaylist.count != 0)
            {
                axWindowsMediaPlayer1.Ctlcontrols.play();
                pictureBox1.Image = Properties.Resources.pause;
            }
        }
        private void PictureBox5_Click(object sender, EventArgs e)
        {
            skinTabControl1.SelectedIndex = 1;
        }
        private void PictureBox6_Click(object sender, EventArgs e)
        {
            skinTabControl1.SelectedIndex = 0;
        }
        private void ToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                label8.Text = "当前音乐无歌词";
                label8.Location = new Point((424 - label8.Width) / 2, label8.Location.Y);
                ArrayList a = new ArrayList();
                a = GetListViewSelectedIndices();
                if (Searchresult != null)
                {
                    Play(Searchresult[(int)a[0]].url + "&quality=" + metroComboBox1.SelectedItem.ToString(), (int)a[0], Searchresult[(int)a[0]].SongName, Searchresult[(int)a[0]].SingerName, Searchresult[(int)a[0]].Album);
                }
                LrcDetails lrcdd = LrcReader(Searchresult[(int)a[0]].lrcurl);
                label9.Text = Searchresult[(int)a[0]].SongName + " - " + Searchresult[(int)a[0]].SingerName;
                label9.Location = new Point((424 - label9.Width) / 2, label9.Location.Y);
            }
            catch
            {
            }
        }
        private void ListView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ArrayList a = new ArrayList();
            a = GetListViewSelectedIndices();
            if (Searchresult != null)
            {
                Play(Searchresult[(int)a[0]].url + "&quality=" + metroComboBox1.SelectedItem.ToString(), (int)a[0], Searchresult[(int)a[0]].SongName, Searchresult[(int)a[0]].SingerName, Searchresult[(int)a[0]].Album);
            }
        }
        public void Play(string url, int n, string songname, string singername, string album)
        {
            int ret = CheckRepeat(songname, singername, album);
            if (ret != -1)
            {
                axWindowsMediaPlayer1.Ctlcontrols.currentItem = axWindowsMediaPlayer1.currentPlaylist.Item[ret];
                axWindowsMediaPlayer1.Ctlcontrols.play();
                pictureBox1.Image = Properties.Resources.pause;
            }
            else
            {
                ToolStripMenuItem4_Click(this, new EventArgs());
                Play(url + "&quality=" + metroComboBox1.SelectedItem.ToString(), n, songname, singername, album);
            }
        }
        public void Volumechange(int num)
        {
            axWindowsMediaPlayer1.settings.volume = num;
        }
        public void Positionchange(int p)
        {
            axWindowsMediaPlayer1.Ctlcontrols.currentPosition = (double)p;
        }
        private void MetroTrackBar2_ValueChanged(object sender, EventArgs e)
        {
            Volumechange(metroTrackBar2.Value);
        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                metroTrackBar1.Value = (int)axWindowsMediaPlayer1.Ctlcontrols.currentPosition;
                if (axWindowsMediaPlayer1.currentMedia.duration != 0)
                {
                    metroTrackBar1.Maximum = (int)axWindowsMediaPlayer1.currentMedia.duration + 2;
                }
            }
            catch
            {
            }
        }
        private void MetroTrackBar1_Scroll(object sender, ScrollEventArgs e)
        {
            Positionchange(metroTrackBar1.Value);
        }
        private void ToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            ArrayList a = new ArrayList();
            a = GetListViewSelectedIndices();
            for (int i = 0; i < a.Count; i++)
            {
                if (Searchresult != null)
                {
                    PlayList p = new PlayList
                    {
                        SongName = Searchresult[(int)a[i]].SongName,
                        SingerName = Searchresult[(int)a[i]].SingerName,
                        Url = Searchresult[(int)a[i]].url,
                        LrcUrl = Searchresult[(int)a[i]].lrcurl,
                        ID = Searchresult[(int)a[i]].id,
                        Album = Searchresult[(int)a[i]].Album
                    };
                    AddMusicToList(p);
                }
            }
        }
        public void AddMusicToList(PlayList p)
        {
            /*
            for (int i = 0; i < listView2.Items.Count; i++)
            {
                if (p.SongName == pl[i].SongName && p.SingerName == pl[i].SingerName & p.Album == pl[i].Album)
                {
                    return;
                }
            }
            */
            pl.Add(p);
            listView2.Items.Add(p.SongName);
            listView2.Items[listView2.Items.Count - 1].SubItems.Add(p.SingerName);
            listView2.Items[listView2.Items.Count - 1].SubItems.Add(p.Album);
            IWMPMedia media = axWindowsMediaPlayer1.newMedia(p.Url);
            axWindowsMediaPlayer1.currentPlaylist.appendItem(media);
        }
        private void PictureBox7_Click(object sender, EventArgs e)
        {
            skinTabControl1.SelectedIndex = 2;
        }
        private void ListView1_DoubleClick_1(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                ToolStripMenuItem3_Click(this, new EventArgs());
            }
            else
            {
                toolStripMenuItem2_Click(this, new EventArgs());
            }
        }
        private void PictureBox4_Click(object sender, EventArgs e)
        {
            if (playmode == "shunxu")
            {
                pictureBox4.Image = Properties.Resources.suiji;
                playmode = "suiji";
                axWindowsMediaPlayer1.settings.setMode("shuffle", true);
            }
            else
            {
                pictureBox4.Image = Properties.Resources.shunxu;
                playmode = "shunxu";
                axWindowsMediaPlayer1.settings.setMode("shuffle", false);
            }
        }
        public class DownloadList
        {
            public string Songname { get; set; }
            public string Singername { get; set; }
            public string Url { get; set; }
            public string Savepath { get; set; }
            public string ID { get; set; }
            public int Api { get; set; }
            public bool IfDownloadlrc { get; set; }
            public bool IfDownloadSong { get; set; }
            public string LrcUrl { set; get; }
            public string DownloadQuality { set; get; }
            public string Album { get; set; }
            public int index { set; get; }
            public bool ifdownloadpic { set; get; }
        }
        private void ToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            List<DownloadList> dl = GetDownloadLists_musiclist(true, checkBox1.Checked, true);
            MultiFilesDownload(dl, metroComboBox2.SelectedIndex + 1);
        }
        private void ToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            List<DownloadList> dl = GetDownloadLists_musiclist(true, checkBox1.Checked);
            MultiFilesDownload(dl, metroComboBox2.SelectedIndex + 1);
        }
        private void ToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            List<DownloadList> dl = GetDownloadLists_musiclist(false, true, true);
            MultiFilesDownload(dl, metroComboBox2.SelectedIndex + 1);
        }
        private void ToolStripMenuItem8_Click(object sender, EventArgs e)
        {
            List<DownloadList> dl = GetDownloadLists_musiclist(false, true);
            MultiFilesDownload(dl, metroComboBox2.SelectedIndex + 1);
        }
        private void ToolStripMenuItem9_Click(object sender, EventArgs e)
        {
            ArrayList a = new ArrayList();
            a = GetListViewSelectedIndices_musiclist();
            for (int i = 0; i < a.Count; i++)
            {
                IWMPMedia media = axWindowsMediaPlayer1.currentPlaylist.Item[(int)a[i] - i];
                axWindowsMediaPlayer1.currentPlaylist.removeItem(media);
                ListViewItem l = listView2.Items[(int)a[i] - i];
                listView2.Items.Remove(l);
                pl.Remove(pl[(int)a[i] - i]);
            }
        }
        public ArrayList GetListViewSelectedIndices_musiclist()
        {
            ArrayList a = new ArrayList();
            string mes = null;
            for (int i = 0; i < listView2.SelectedIndices.Count; i++)
            {
                a.Add(listView2.SelectedIndices[i]);
            }
            foreach (int i in a)
            {
                mes += i.ToString();
            }
            return a;
        }
        public ArrayList GetListViewSelectedIndices_downloadlist()
        {
            ArrayList a = new ArrayList();
            string mes = null;
            for (int i = 0; i < listView3.SelectedIndices.Count; i++)
            {
                a.Add(listView3.SelectedIndices[i]);
            }
            foreach (int i in a)
            {
                mes += i.ToString();
            }
            return a;
        }
        private void 删除该项ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < downloadthreadlist.Count; i++)
            {
                if (downloadthreadlist[i].IsAlive)
                {
                    MessageBox.Show("该功能不能用于取消下载，请等待所有下载完成后再试。", caption: "提示：");
                    return;
                }
                else
                {
                    ArrayList a = GetListViewSelectedIndices_downloadlist();
                    for (int i_ = 0; i_ < a.Count; i_++)
                    {
                        listView3.Items[(int)a[i_] - i_].Remove();
                    }
                }
            }
        }
        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (about == null)
            {
                about = new About(ver, latestversion);
                about.Show();
            }
            else
            {
                about.Activate();
            }
        }
        private void ListView2_DoubleClick(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                ToolStripMenuItem10_Click(this, new EventArgs());
            }
            else
            {
                ToolStripMenuItem6_Click(this, new EventArgs());
            }
        }
        private void ToolStripMenuItem10_Click(object sender, EventArgs e)
        {
            try
            {
                label8.Text = "当前音乐无歌词";
                label8.Location = new Point((424 - label8.Width) / 2, label8.Location.Y);
                ArrayList a = new ArrayList();
                a = GetListViewSelectedIndices_musiclist();
                IWMPMedia media = axWindowsMediaPlayer1.newMedia(pl[(int)a[0]].Url);
                axWindowsMediaPlayer1.Ctlcontrols.currentItem = axWindowsMediaPlayer1.currentPlaylist.Item[(int)a[0]];
                axWindowsMediaPlayer1.Ctlcontrols.play();
                pictureBox1.Image = Properties.Resources.pause;
                LrcDetails lrcdd = LrcReader(pl[(int)a[0]].LrcUrl);
                label9.Text = pl[(int)a[0]].SongName + " - " + pl[(int)a[0]].SingerName;
                label9.Location = new Point((424 - label9.Width) / 2, label9.Location.Y);
            }
            catch
            {
            }
        }
        public LrcDetails LrcReader(string url)
        {
            WebClient wc = new WebClient();
            Stream s = wc.OpenRead(url);
            StreamReader sr = new StreamReader(s);
            string lrc = sr.ReadToEnd();
            lrcd.url = url;
            lrcd.LrcWord = new List<LrcContent>();
            lrc.Replace("\r\n", "");
            string[] a = lrc.Split('[');
            string nlrc = "";
            for (int i = 1; i < a.Length; i++)
            {
                nlrc += "[" + a[i];
            }
            string[] c = { "\r", "\n" };
            string[] b = nlrc.Split(new char[2] { '\r', '\n' });
            foreach (string d in b)
            {
                if (d.StartsWith("[ti:"))
                {
                    lrcd.Title = SplitInfo(d);
                }
                else if (d.StartsWith("[ar:"))
                {
                    lrcd.Artist = SplitInfo(d);
                }
                else if (d.StartsWith("[al:"))
                {
                    lrcd.Album = SplitInfo(d);
                }
                else if (d.StartsWith("[by:"))
                {
                    lrcd.LrcBy = SplitInfo(d);
                }
                else if (d.StartsWith("[offset:"))
                {
                    lrcd.Offset = SplitInfo(d);
                }
                else
                {
                    try
                    {
                        Regex regexword = new Regex(@".*\](.*)");
                        Match mcw = regexword.Match(d);
                        string word = mcw.Groups[1].Value;
                        Regex regextime = new Regex(@"\[([0-9.:]*)\]", RegexOptions.Compiled);
                        MatchCollection mct = regextime.Matches(d);
                        foreach (Match item in mct)
                        {
                            double time = TimeSpan.Parse("00:" + item.Groups[1].Value).TotalSeconds;
                            LrcContent l = new LrcContent()
                            {
                                Time = time,
                                Ci = word
                            };
                            lrcd.LrcWord.Add(l);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            return lrcd;
        }
        private static string SplitInfo(string line)
        {
            return line.Substring(line.IndexOf(":") + 1).TrimEnd(']');
        }
        private void Timer3_Tick(object sender, EventArgs e)
        {
            try
            {
                if (axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying)
                {
                    for (int i = 0; i < lrcd.LrcWord.Count; i++)
                    {
                        try
                        {
                            if (((int)lrcd.LrcWord[i].Time + Timec) <= metroTrackBar1.Value && metroTrackBar1.Value <= ((int)lrcd.LrcWord[i + 1].Time + Timec))
                            {
                                label8.Text = lrcd.LrcWord[i].Ci;
                                i++;
                                label8.Location = new Point((424 - label8.Width) / 2, label8.Location.Y);
                                label9.Location = new Point((424 - label9.Width) / 2, label9.Location.Y);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                        if (i > lrcd.LrcWord.Count)
                        {
                            timer3.Enabled = false;
                        }
                    }
                }
                if (axWindowsMediaPlayer1.playState == WMPPlayState.wmppsMediaEnded)
                {
                    label8.Text = "当前无音乐播放";
                    label9.Text = "歌曲名";
                    label8.Location = new Point((424 - label8.Width) / 2, label8.Location.Y);
                    label9.Location = new Point((424 - label9.Width) / 2, label9.Location.Y);
                }
            }
            catch
            {
            }
        }
        private void AxWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying)
            {
                for (int i = 0; i < listView2.Items.Count; i++)
                {
                    if (axWindowsMediaPlayer1.currentMedia.sourceURL == pl[i].Url)
                    {
                        label9.Text = pl[i].SongName + " - " + pl[i].SingerName;
                        label9.Location = new Point((424 - label9.Width) / 2, label9.Location.Y);
                        LrcDetails lrcdd = LrcReader(pl[i].LrcUrl);
                    }
                }
            }
        }
        private void MediaEndAndChangeLrc(object i)
        {
            try
            {
                LrcDetails lrcdd = LrcReader(pl[(int)i].LrcUrl);
            }
            catch
            {
            }
        }
        public int CheckRepeat(string songname, string singername, string album)
        {
            for (int i = 0; i < listView2.Items.Count; i++)
            {
                if (songname == pl[i].SongName && singername == pl[i].SingerName && album == pl[i].Album)
                {
                    return i;
                }
            }
            return -1;
        }
        public void HotMusicList()
        {
            skinTabControl1.SelectedIndex = 0;
            Searchresult = GetMusiclistJson("3778678", 1);
            listView1.Items.Clear();
            if (Searchresult != null)
            {
                for (int i = 0; i < Searchresult.Count; i++)
                {
                    listView1.Items.Add(Searchresult[i].SongName);
                    listView1.Items[i].SubItems.Add(Searchresult[i].SingerName);
                    listView1.Items[i].SubItems.Add(Searchresult[i].Album);
                }
            }
        }
        private void ToolStripMenuItem11_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            listView1.Items.Add("获取中...");
            Thread a = new Thread(HotMusicList);
            a.Start();
        }
        private void AxWindowsMediaPlayer1_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
        {
            for (int i = 0; i < pl.Count; i++)
            {
                if (axWindowsMediaPlayer1.currentMedia.sourceURL == pl[i].Url)
                {
                    if (pl[i].LrcUrl != lrcd.url)
                    {
                        MediaEndAndChangeLrc(i);
                    }
                }
            }
        }
        private void LinkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (f2 == null)
            {
                f2 = new Form2();
                f2.Show();
            }
            else
            {
                f2.Activate();
            }
        }
        private void AddMusicDetails(string path, string title, string artists, string ablum, string picture, string dir, bool ifdownloadpic)
        {
            ID3Info info = new ID3Info(path, true);
            info.ID3v2Info.SetTextFrame("TIT2", title);
            info.ID3v2Info.SetTextFrame("TPE1", artists);
            info.ID3v2Info.SetTextFrame("TALB", ablum);
            try
            {
                if (ifdownloadpic)
                {
                    WebClient wc = new WebClient();
                    wc.DownloadFile(picture, dir + "\\" + title + " - " + artists + ".jpg");
                    AttachedPictureFrame pic = new AttachedPictureFrame(FrameFlags.Compression, "cover.jpg", TextEncodings.UTF_16, "", AttachedPictureFrame.PictureTypes.Other, new System.IO.MemoryStream(File.ReadAllBytes(dir + "\\" + title + " - " + artists + ".jpg")));
                    info.ID3v2Info.AttachedPictureFrames.Add(pic);
                }
            }
            catch
            {
            }
            finally
            {
                info.Save();
            }
            //File.Delete(dir + "\\" + title + " - " + artists + ".jpg");
        }
        private void MultiFilesDownload(List<DownloadList> dl, int n)
        {
            if (dl == null || dl.Count == 0)
            {
                return;
            }
            for (int i = 0; i < n; i++)
            {
                List<DownloadList> dl_ = new List<DownloadList>();
                for (int x = 0; x < dl.Count; x++)
                {
                    if (x % n == i)
                    {
                        DownloadList _dl = new DownloadList()
                        {
                            Album = dl[x].Album,
                            Api = dl[x].Api,
                            DownloadQuality = dl[x].DownloadQuality,
                            ID = dl[x].ID,
                            IfDownloadlrc = dl[x].IfDownloadlrc,
                            IfDownloadSong = dl[x].IfDownloadSong,
                            index = dl[x].index,
                            LrcUrl = dl[x].LrcUrl,
                            Savepath = dl[x].Savepath,
                            Singername = dl[x].Singername,
                            Songname = dl[x].Songname,
                            Url = dl[x].Url,
                            ifdownloadpic = dl[x].ifdownloadpic
                        };
                        if (dl[x].Api == 1)
                        {
                        }
                        dl_.Add(_dl);
                    }
                }
                Thread a = new Thread(new ParameterizedThreadStart(Download));
                downloadthreadlist.Add(a);
                a.Start(dl_);
            }
        }
        private void ToolStripMenuItem12_Click(object sender, EventArgs e)
        {
            try
            {
                ArrayList a = GetListViewSelectedIndices();
                Clipboard.SetText(listView1.Items[(int)a[0]].Text);
            }
            catch
            {
            }
        }
        private void ToolStripMenuItem13_Click(object sender, EventArgs e)
        {
            try
            {
                ArrayList a = GetListViewSelectedIndices();
                Clipboard.SetText(listView1.Items[(int)a[0]].SubItems[1].Text);
            }
            catch
            {
            }
        }
        private void ToolStripMenuItem14_Click(object sender, EventArgs e)
        {
            try
            {
                ArrayList a = GetListViewSelectedIndices();
                Clipboard.SetText(listView1.Items[(int)a[0]].SubItems[2].Text);
            }
            catch
            {
            }
        }
        private void ToolStripMenuItem15_Click(object sender, EventArgs e)
        {
            try
            {
                ArrayList a = GetListViewSelectedIndices_musiclist();
                Clipboard.SetText(listView2.Items[(int)a[0]].Text);
            }
            catch
            {
            }
        }
        private void ToolStripMenuItem16_Click(object sender, EventArgs e)
        {
            try
            {
                ArrayList a = GetListViewSelectedIndices_musiclist();
                Clipboard.SetText(listView2.Items[(int)a[0]].SubItems[1].Text);
            }
            catch
            {
            }
        }
        private void ToolStripMenuItem17_Click(object sender, EventArgs e)
        {
            try
            {
                ArrayList a = GetListViewSelectedIndices_musiclist();
                Clipboard.SetText(listView2.Items[(int)a[0]].SubItems[2].Text);
            }
            catch
            {
            }
        }
        private List<DownloadList> GetDownloadLists(bool ifDownloadSong, bool ifDownloadLrc, bool ifDownloadAll = false)
        {
            if (!ifDownloadAll)
            {
                List<DownloadList> dl = new List<DownloadList>();
                downloadindices = GetListViewSelectedIndices();
                if (Searchresult != null)
                {
                    for (int i = 0; i < downloadindices.Count; i++)
                    {
                        listView3.Items.Add(Searchresult[(int)downloadindices[i]].SongName);
                        listView3.Items[listView3.Items.Count - 1].SubItems.Add(Searchresult[i].SingerName);
                        listView3.Items[listView3.Items.Count - 1].SubItems.Add("准备下载");
                        dl.Add(SetDownloadMedia(GetApiCode(), Searchresult[(int)downloadindices[i]].id, ifDownloadLrc, ifDownloadSong, DownloadPathtextBox.Text, Searchresult[(int)downloadindices[i]].SongName, Searchresult[(int)downloadindices[i]].SingerName, Searchresult[(int)downloadindices[i]].url, Searchresult[(int)downloadindices[i]].lrcurl, Searchresult[(int)downloadindices[i]].Album, metroComboBox1.SelectedItem.ToString(), listView3.Items.Count - 1, checkBox3.Checked));
                    }
                    return dl;
                }
                return null;
            }
            else
            {
                downloadindices.Clear();
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    downloadindices.Add(i);
                }
                if (Searchresult != null)
                {
                    List<DownloadList> dl = new List<DownloadList>();
                    for (int i = 0; i < downloadindices.Count; i++)
                    {
                        listView3.Items.Add(Searchresult[(int)downloadindices[i]].SongName);
                        listView3.Items[listView3.Items.Count - 1].SubItems.Add(Searchresult[i].SingerName);
                        listView3.Items[listView3.Items.Count - 1].SubItems.Add("准备下载");
                        dl.Add(SetDownloadMedia(GetApiCode(), Searchresult[(int)downloadindices[i]].id, ifDownloadLrc, ifDownloadSong, DownloadPathtextBox.Text, Searchresult[(int)downloadindices[i]].SongName, Searchresult[(int)downloadindices[i]].SingerName, Searchresult[(int)downloadindices[i]].url, Searchresult[(int)downloadindices[i]].lrcurl, Searchresult[(int)downloadindices[i]].Album, metroComboBox1.SelectedItem.ToString(), listView3.Items.Count - 1, checkBox3.Checked));
                    }
                    return dl;
                }
                return null;
            }
        }
        private List<DownloadList> GetDownloadLists_musiclist(bool ifDownloadSong, bool ifDownloadLrc, bool ifDownloadAll = false)
        {
            if (!ifDownloadAll)
            {
                List<DownloadList> dl = new List<DownloadList>();
                downloadindices = GetListViewSelectedIndices_musiclist();
                if (pl != null)
                {
                    for (int i = 0; i < downloadindices.Count; i++)
                    {
                        listView3.Items.Add(pl[(int)downloadindices[i]].SongName);
                        listView3.Items[listView3.Items.Count - 1].SubItems.Add(pl[i].SingerName);
                        listView3.Items[listView3.Items.Count - 1].SubItems.Add("准备下载");
                        dl.Add(SetDownloadMedia(GetApiCode(), pl[(int)downloadindices[i]].ID, ifDownloadLrc, ifDownloadSong, DownloadPathtextBox.Text, pl[(int)downloadindices[i]].SongName, pl[(int)downloadindices[i]].SingerName, pl[(int)downloadindices[i]].Url, pl[(int)downloadindices[i]].LrcUrl, pl[(int)downloadindices[i]].Album, metroComboBox1.SelectedItem.ToString(), listView3.Items.Count - 1, checkBox3.Checked));
                    }
                    return dl;
                }
                return null;
            }
            else
            {
                downloadindices.Clear();
                for (int i = 0; i < listView2.Items.Count; i++)
                {
                    downloadindices.Add(i);
                }
                if (pl != null)
                {
                    List<DownloadList> dl = new List<DownloadList>();
                    for (int i = 0; i < downloadindices.Count; i++)
                    {
                        listView3.Items.Add(pl[(int)downloadindices[i]].SongName);
                        listView3.Items[listView3.Items.Count - 1].SubItems.Add(pl[i].SingerName);
                        listView3.Items[listView3.Items.Count - 1].SubItems.Add("准备下载");
                        dl.Add(SetDownloadMedia(GetApiCode(), pl[(int)downloadindices[i]].ID, ifDownloadLrc, ifDownloadSong, DownloadPathtextBox.Text, pl[(int)downloadindices[i]].SongName, pl[(int)downloadindices[i]].SingerName, pl[(int)downloadindices[i]].Url, pl[(int)downloadindices[i]].LrcUrl, pl[(int)downloadindices[i]].Album, metroComboBox1.SelectedItem.ToString(), listView3.Items.Count - 1, checkBox3.Checked));
                    }
                    return dl;
                }
                return null;
            }
        }
        private string GetPicUrl(string id, int api)
        {
            if (api == 1)
            {
                return "https://v1.itooi.cn/netease/pic?id=" + id;
            }
            if (api == 2)
            {
                return "https://v1.itooi.cn/kugou/pic?id=" + id;
            }
            if (api == 3)
            {
                return "https://v1.itooi.cn/tencent/pic?id=" + id;
            }
            if (api == 4)
            {
                return "https://v1.itooi.cn/kuwo/pic?id=" + id;
            }
            if (api == 5)
            {
                return "https://v1.itooi.cn/baidu/pic?id=" + id;
            }
            return "";
        }
        private void Timer2_Tick(object sender, EventArgs e)
        {
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                pictureBox1.Image = Properties.Resources.pause;
                return;
            }
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPaused)
            {
                pictureBox1.Image = Properties.Resources.play;
                return;
            }
            if (axWindowsMediaPlayer1.currentPlaylist.count != 0)
            {
                pictureBox1.Image = Properties.Resources.play;
            }
        }
        private void ToolStripMenuItem18_Click(object sender, EventArgs e)
        {
            ArrayList a = GetListViewSelectedIndices_downloadlist();
            for (int i = 0; i < a.Count; i++)
            {
                canceldownloadindex.Add(a[i]);
                listView3.Items[(int)a[i]].SubItems[2].Text = "取消下载";
            }
        }
        /// <summary>
        /// 如果取消下载返回 true
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool Ifcanceldownload(int index)
        {
            for (int a = 0; a < canceldownloadindex.Count; a++)
            {
                if ((int)canceldownloadindex[a] == index)
                {
                    canceldownloadindex.RemoveAt(a);
                    return true;
                }
            }
            return false;
        }
        private void MetroComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (metroComboBox3.SelectedIndex)
            {
                case 0:
                    this.Style = MetroFramework.MetroColorStyle.Black;
                    break;
                case 1:
                    this.Style = MetroFramework.MetroColorStyle.White;
                    break;
                case 2:
                    this.Style = MetroFramework.MetroColorStyle.Silver;
                    break;
                case 3:
                    this.Style = MetroFramework.MetroColorStyle.Blue;
                    break;
                case 4:
                    this.Style = MetroFramework.MetroColorStyle.Green;
                    break;
                case 5:
                    this.Style = MetroFramework.MetroColorStyle.Lime;
                    break;
                case 6:
                    this.Style = MetroFramework.MetroColorStyle.Teal;
                    break;
                case 7:
                    this.Style = MetroFramework.MetroColorStyle.Orange;
                    break;
                case 8:
                    this.Style = MetroFramework.MetroColorStyle.Brown;
                    break;
                case 9:
                    this.Style = MetroFramework.MetroColorStyle.Pink;
                    break;
                case 10:
                    this.Style = MetroFramework.MetroColorStyle.Magenta;
                    break;
                case 11:
                    this.Style = MetroFramework.MetroColorStyle.Purple;
                    break;
                case 12:
                    this.Style = MetroFramework.MetroColorStyle.Red;
                    break;
                case 13:
                    this.Style = MetroFramework.MetroColorStyle.Yellow;
                    break;
            }
        }
        private void PictureBox8_Click(object sender, EventArgs e)
        {
            Timec = Timec + 0.5;
        }
        private void PictureBox9_Click(object sender, EventArgs e)
        {
            Timec = Timec - 0.5;
        }
        private void ToolStripMenuItem19_Click(object sender, EventArgs e)
        {
            try
            {
                ArrayList a = GetListViewSelectedIndices();
                Clipboard.SetText(Searchresult[(int)a[0]].id);
            }
            catch
            {
            }
        }
        private void ToolStripMenuItem20_Click(object sender, EventArgs e)
        {
            try
            {
                ArrayList a = GetListViewSelectedIndices_musiclist();
                Clipboard.SetText(pl[(int)a[0]].ID);
            }
            catch
            {
            }
        }
    }
}
