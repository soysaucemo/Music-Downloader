using System;
using System.Collections.Generic;
namespace Music_Downloader
{
    public class Setting
    {
        public string SavePath { get; set; }
        public List<PlayList> PlayList { get; set; }
        public string DownloadQuality { get; set; }
        public int Volume { get; set; }
        public int MultiDownload { get; set; }
        public bool ifdownloadpic { set; get; }
        public bool ifdownloadlrc { set; get; }
        public int savenamestyle { set; get; }
    }
    public class PlayList
    {
        public string SongName { get; set; }
        public string SingerName { get; set; }
        public string Url { get; set; }
        public string ID { get; set; }
        public string LrcUrl { get; set; }
        public string Album { get; set; }
    }
}
