using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using Microsoft.Phone.Tasks;
using System.Windows.Data;
using MyToolkit.Multimedia;
using System.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using vservWindowsPhone;
using TV_Thailand.Model;

namespace TV_Thailand
{
    public partial class OTVShowPivotPage : PhoneApplicationPage
    {
        // Initialize VservSDK
        VservAdControl VAC = VservAdControl.Instance;

        List<OTVEpisodeItem> episodeItems = new List<OTVEpisodeItem>();
        string otvId = "";
        string otvApiName = "";

        ScrollViewer scrollViewer;
        bool isLoadDetail = false;
        bool isEmptyProgramlist = false;

        public OTVShowPivotPage()
        {
            InitializeComponent();
            this.Loaded += ProgramPivotPage_Loaded;
            VAC.VservAdNoFill += VAC_VservAdNoFill;

            //ListBox_Episode.Loaded += new RoutedEventHandler(ListBox_Episode_Loaded);
        }

        void VAC_VservAdNoFill(object sender, EventArgs e)
        {
            adGrid.Visibility = Visibility.Collapsed;
        }

        void ProgramPivotPage_Loaded(object sender, RoutedEventArgs e)
        {
            VAC.RenderAd("562db24e", adGrid);
        }

        #region Load More
        void ListBox_Episode_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            element.Loaded -= ListBox_Episode_Loaded;
            scrollViewer = FindChildOfType<ScrollViewer>(element);
            if (scrollViewer == null)
            {
                throw new InvalidOperationException("ScrollViewer not found.");
            }

            Binding binding = new Binding();
            binding.Source = scrollViewer;
            binding.Path = new PropertyPath("VerticalOffset");
            binding.Mode = BindingMode.OneWay;
            this.SetBinding(ListVerticalOffsetProperty, binding);
        }

        public readonly DependencyProperty ListVerticalOffsetProperty = DependencyProperty.Register("ListVerticalOffset", typeof(double), typeof(ProgramPivotPage),
            new PropertyMetadata(new PropertyChangedCallback(OnListVerticalOffsetChanged)));

        public double ListVerticalOffset
        {
            get { return (double)this.GetValue(ListVerticalOffsetProperty); }
            set { this.SetValue(ListVerticalOffsetProperty, value); }
        }

        /// <summary>
        /// Finding the ScrollViewer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <returns></returns>
        static T FindChildOfType<T>(DependencyObject root) where T : class
        {
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                DependencyObject current = queue.Dequeue();
                for (int i = VisualTreeHelper.GetChildrenCount(current) - 1; 0 <= i; i--)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (typedChild != null)
                    {
                        return typedChild;
                    }
                    queue.Enqueue(child);
                }
            }
            return null;
        }

        private static void OnListVerticalOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            OTVShowPivotPage page = obj as OTVShowPivotPage;

            if (page.isEmptyProgramlist) return;

            ScrollViewer viewer = page.scrollViewer;

            //Checks if the Scroll has reached the last item based on the ScrollableHeight
            bool atBottom = viewer.VerticalOffset >= viewer.ScrollableHeight;

            if (atBottom)
            {
                page.loadOTVEpisode();
            }
        }

        #endregion

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            YouTube.CancelPlay();
            base.OnNavigatedTo(e);
            string title = "";

            NavigationContext.QueryString.TryGetValue("otv_id", out otvId);
            NavigationContext.QueryString.TryGetValue("otv_api_name", out otvApiName);
            NavigationContext.QueryString.TryGetValue("title", out title);


            MainPivot.Title = title;
            loadOTVEpisode();
        }

        private void loadOTVEpisode()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Applications unable to connect to internet");
            }
            else
            {
                SystemTray.IsVisible = loadingProgressBar.IsVisible = true;
                string url = Utility.Instance.getUrlOTVEpisode(otvApiName, otvId);
                Uri episodeUri = new Uri(url);
                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += otvEpisodeClient_DownloadStringCompleted;
                webClient.DownloadStringAsync(episodeUri);
            }
        }

        void otvEpisodeClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            SystemTray.IsVisible = loadingProgressBar.IsVisible = false;
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                JObject json = JObject.Parse(e.Result);

                string thumbnail = json["thumbnail"].Value<string>();
                if (thumbnail != "")
                {
                    Uri uri = new Uri(thumbnail, UriKind.Absolute);
                        ImgProgram.Source = new BitmapImage(uri);
                }

                string nameTh = json["name_th"].Value<string>();
                string detail = json["detail"].Value<string>();


                string fullDetail = String.Format("{0}\n\n{1} Views", nameTh, detail);
                txtProgramDetail.Text = fullDetail;

                isEmptyProgramlist = true;

                JToken contentList = json["contentList"];
                foreach (JToken content in contentList)
                {
                    isEmptyProgramlist = false;

                    OTVEpisodeItem episode = new OTVEpisodeItem();
                    episode.id = (content["id"] != null) ? content["id"].Value<string>() : "";
                    episode.thumbnail = (content["thumbnail"] != null) ? content["thumbnail"].Value<string>() : "";
                    episode.nameTh = (content["name_th"] != null) ? content["name_th"].Value<string>() : "";
                    episode.detail = (content["detail"] != null) ? content["detail"].Value<string>() : "";
                    episode.date = (content["date"] != null) ? "ออกอากาศ " + content["date"].Value<string>() : "";


                    episode.parts = new List<OTVPartItem>();
                    JToken items = content["item"];
                    OTVPartItem partItem = null;
                    foreach (JToken item in items)
                    {
                        if (partItem == null) 
                        {
                            partItem = new OTVPartItem();
                        }

                        string mediaCode = (item["media_code"] != null) ? item["media_code"].Value<string>() : "";
                        if ("1001".Equals(mediaCode)) 
                        {
                            partItem.vastURL = item["stream_url"].Value<string>();
                        }
                        else if ("1000".Equals(mediaCode) || "1002".Equals(mediaCode))
                        {
                            partItem.partId = item["id"].Value<string>();
                            partItem.nameTh = item["name_th"].Value<string>();
                            partItem.thumbnail = item["thumbnail"].Value<string>();
                            partItem.streamURL = item["stream_url"].Value<string>();
                            partItem.mediaCode = mediaCode;

                            episode.parts.Add(partItem);
                            partItem = null;
                        }
                    }

                    episodeItems.Add(episode);
                }

                Dispatcher.BeginInvoke(
                    () =>
                    {
                        ListBox_Episode.ItemsSource = null;
                        ListBox_Episode.ItemsSource = episodeItems;
                    }
                );
            }
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainPivot.SelectedIndex == 1 && !isLoadDetail)
            {

            }
        }

        private void ListBox_Episode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Episode.SelectedIndex == -1) return;
            OTVEpisodeItem selectedEpisode = episodeItems[ListBox_Episode.SelectedIndex];

            PhoneApplicationService.Current.State["OTVEpisode"] = selectedEpisode;
            NavigationService.Navigate(new Uri("/OTVPartPage.xaml", UriKind.Relative));
            ListBox_Episode.SelectedIndex = -1;
        }

    }
}