using HtmlAgilityPack;
using Microsoft.Phone.Tasks;
using MyToolkit.Multimedia;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TV_Thailand.Class;

namespace TV_Thailand
{
    public class Utility
    {
        private static Utility instance;

        private Utility() {}

        public static Utility Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Utility();
                }
                return instance;
            }
        }

        public static string Domain = "http://tv.makathon.com/api2";
        public static string UserAgent_iOS = "Mozilla/5.0 (iPhone; CPU iPhone OS 6_1 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10B143 Safari/8536.25";


        public static String GetTimestamp()
        {
            return DateTime.Now.ToString("yyyyMMddHHmm");
        }

        public string getUrlMessage()
        {
            return String.Format(@"{0}/getMessageWP/?device=wp", Domain);
        }

        public string getInHouseAd()
        {
            return String.Format(@"{0}/advertise?device=wp", Domain);
        }

        public string getUrlCategory()
        {
            return String.Format(@"{0}/category?device=wp", Domain);
        }

        public string getUrlChannel()
        {
            return String.Format(@"{0}/channel?device=wp", Domain);
        }

        public string getUrlProgram(string cat_id, int start)
        {
            return String.Format(@"{0}/category/{1}/{2}?device=wp&time={3}", Domain, cat_id, start, GetTimestamp());
        }

        public string getUrlWhatsNew(int start)
        {
            return String.Format(@"{0}/whatsnew/{1}?device=wp&time={2}", Domain, start, GetTimestamp());
        }

        public string getUrlProgramSearch(string keyword, int start)
        {
            return String.Format(@"{0}/getProgramSearch/{1}?device=wp&keyword={2}&time={3}", Domain, start, keyword, GetTimestamp());
        }

        public string getUrlProgramlist(string program_id, int start)
        {
            return String.Format(@"{0}/episode/{1}/{2}?device=wp&time={3}", Domain, program_id, start, GetTimestamp());
        }

        public string getUrlViewProgramlist(string programlist_id, int start)
        {
            return String.Format(@"{0}/view_epiode/{1}?device=wp", Domain, programlist_id);
        }

        public string getUrlProgramDetail(string program_id)
        {
            return String.Format(@"{0}/getProgramDetail/{1}/?device=wp", Domain, program_id);
        }

        public string videoThumbnail(string video_id, string src_type)
        {
            if(src_type.Equals("0"))
            {
                 return "http://i.ytimg.com/vi/" + video_id + "/1.jpg";
            }
            else if (src_type.Equals("1"))
            {
                return "http://www.dailymotion.com/thumbnail/160x120/video/" + video_id;
             }
            else if (src_type.Equals("13") || src_type.Equals("14") || src_type.Equals("15"))
            {
                return "http://video.mthai.com/thumbnail/" + video_id + ".jpg";
            }
            return "";
        }

        public string DecodeFrom64(string base64String)
        {
            String OriginalStringText = string.Empty;
            byte[] binaryData;
            try
            {
                binaryData = System.Convert.FromBase64String(base64String);
                OriginalStringText = Encoding.UTF8.GetString(binaryData, 0, binaryData.Length);
            }
            catch (System.ArgumentNullException)
            {
                System.Console.WriteLine("Base 64 string is null.");
            }
            catch (System.FormatException)
            {
                System.Console.WriteLine("Base 64 string length is not " +
                   "4 or is not an even multiple of 4.");
            }

            return OriginalStringText;
        }

        public string DecodeVideoKey(string videoKeyEncrypt)
        {
            StringBuilder stringBuilder = new StringBuilder(videoKeyEncrypt);
            stringBuilder.Replace("-", "+")
                    .Replace("_", "/").Replace(",", "=").Replace("!", "a")
                    .Replace("@", "b").Replace("#", "c").Replace("$", "d")
                    .Replace("%", "e").Replace("^", "f").Replace("&", "g")
                    .Replace("*", "h").Replace("(", "i").Replace(")", "j")
                    .Replace("{", "k").Replace("}", "l").Replace("[", "m")
                    .Replace("]", "n").Replace(":", "o").Replace(";", "p")
                    .Replace("<", "q").Replace(">", "r").Replace("?", "s");
            return DecodeFrom64(stringBuilder.ToString());
        }

        public void PlayVideo(string sourceType, string videoId, string password)
        {
            if (sourceType.Equals("0"))
            {
                YouTube.Play(videoId, true, YouTubeQuality.Quality480P);

                //url = "http://www.youtube.com/watch?v=" + videoKeys[0];
                //string url = "http://www.youtube.com/embed/" + videoId + "?autoplay=1";

                //WebBrowserTask webBrowserTask = new WebBrowserTask();
                //webBrowserTask.URL = "vnd.youtube:" + videoId + "?vndapp=youtube_mobile";
                //webBrowserTask.Show();
            }
            else if (sourceType.Equals("1"))
            {
                //url = "http://www.dailymotion.com/video/" + videoKeys[0];
                string url = "http://www.dailymotion.com/embed/video/" + videoId + "?autoPlay=1";
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                webBrowserTask.Uri = new Uri(url);
                webBrowserTask.Show();
            }
            else if (sourceType.Equals("11") || sourceType.Equals("12"))
            {
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                webBrowserTask.Uri = new Uri(videoId);
                webBrowserTask.Show();
            }
            else if (sourceType.Equals("13"))
            {
                playMThaiVideoFromAPI(videoId);
            }
            else if (sourceType.Equals("14"))
            {
                playMThaiVideoFromWeb(videoId);
            }
            else if (sourceType.Equals("15"))
            {
                playMThaiVideoWithPassword(videoId, password);
            }
            else
            {
                MessageBox.Show("Video Not Support");
            }
        }

        private void playMThaiVideoFromAPI(string videoId)
        {
            Uri mthaiUri = new Uri("http://video.mthai.com/get_config_event.php?id=" + videoId);
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler((sender, e) =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show(e.Error.Message);
                    }
                    else
                    {
                        String jString = e.Result.Replace('\'', '"');
                        JObject json = JObject.Parse(jString);
                        JArray playlist = (JArray)json["playlist"];
                        JToken pl = playlist[1];
                        string src = pl["url"].Value<string>();
                        string[] sUri = src.Split('/');
                        if (sUri.Length > 0)
                        {
                            if (sUri[sUri.Length - 1].StartsWith(videoId))
                            {
                                MediaPlayerLauncher launcher = new MediaPlayerLauncher
                                {
                                    Controls = MediaPlaybackControls.All,
                                    Media = new Uri(src)
                                };
                                launcher.Show();
                            }
                            else
                            {
                                MessageBox.Show("Sorry, video missing please try again.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Sorry, video have problem.");
                        }

                    }
                });
            webClient.DownloadStringAsync(mthaiUri);
        }


        private void playMThaiVideoFromWeb(string videoId)
        {
            Uri mthaiUri = new Uri("http://video.mthai.com/cool/player/" + videoId + ".html");
            WebClient webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.UserAgent] = UserAgent_iOS;
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler((sender, e) =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show(e.Error.Message);
                    }
                    else
                    {
                        ExtractMthaiVideo(videoId, e.Result);
                    }
                });
            webClient.DownloadStringAsync(mthaiUri);
        }

        private void playMThaiVideoWithPassword(string videoId, string password)
        {
            Uri mthaiUri = new Uri("http://video.mthai.com/cool/player/" + videoId + ".html");
            WebClient webClient = new WebClient();

            webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            webClient.Headers[HttpRequestHeader.UserAgent] = UserAgent_iOS;
            webClient.Encoding = Encoding.UTF8;
            webClient.UploadStringCompleted += new UploadStringCompletedEventHandler((sender, e) =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show(e.Error.Message);
                    }
                    else
                    {
                        ExtractMthaiVideo(videoId, e.Result);
                    }
                });
            StringBuilder postData = new StringBuilder();
            postData.AppendFormat("{0}={1}", "clip_password", HttpUtility.UrlEncode(password));
            webClient.Headers[HttpRequestHeader.ContentLength] = postData.Length.ToString();
            webClient.UploadStringAsync(mthaiUri, "POST", postData.ToString());
        }

        void ExtractMthaiVideo(string videoId, string content)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//source"))
            {
                string src = link.GetAttributeValue("src", "");
                string[] sUri = src.Split('/');
                if (sUri.Length > 0)
                {
                    if (sUri[sUri.Length - 1].StartsWith(videoId))
                    {
                        MediaPlayerLauncher launcher = new MediaPlayerLauncher
                        {
                            Controls = MediaPlaybackControls.All,
                            Media = new Uri(src)
                        };
                        launcher.Show();
                    }
                    else
                    {
                        MessageBox.Show("Sorry, video missing please try again.");
                    }
                }
                else
                {
                    MessageBox.Show("Sorry, video have problem.");
                }
                
            }
        }
    }
}
