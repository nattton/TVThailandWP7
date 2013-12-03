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
using System.Windows.Media.Imaging;
using System.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using TV_Thailand.Class;
using Microsoft.Phone.Tasks;

namespace TV_Thailand
{
    public partial class MainPage : PhoneApplicationPage
    {
        List<CategoryItem> categoryItems = new List<CategoryItem>();
        List<ChannelItem> channelItems = new List<ChannelItem>();
        List<ProgramItem> programItems = new List<ProgramItem>();
        List<InHouseAdItem> inHouseAds = new List<InHouseAdItem>();

        bool isLoadWhatsNew = false;
        bool isLoadCategory = false;
        bool isLoadChannel = false;
        bool isLoadLiveChannel = false;

        public class HubTileData
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public string Image { get; set; }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Visibility v = (Visibility)Resources["PhoneLightThemeVisibility"];

            string strBGPano = "Images/PanoramaBackground.jpg";
            if (v == System.Windows.Visibility.Visible)
            {
                // Is light theme
                strBGPano = "Images/PanoramaBackgroundLight.jpg";
            }
            else
            {
                // Is dark theme
                strBGPano = "Images/PanoramaBackground.jpg";
            }

            BitmapImage bitmapImage = new BitmapImage(new Uri(strBGPano, UriKind.Relative));
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = bitmapImage;
            MainPanorama.Background = imageBrush;

            loadWhatsNew();
            loadInHosueAds(); 
            loadCategory();
            loadChannel();
            loadLiveChannel();
        }

        private void panorama_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplicationBarIconButton btnRefresh = (ApplicationBarIconButton)ApplicationBar.Buttons[0];

            btnRefresh.Click -= ApplicationBarIconButton_RefreshCategory_Click;
            btnRefresh.Click -= ApplicationBarIconButton_Refresh_Click;
            btnRefresh.Click -= ApplicationBarIconButton_RefreshChannel_Click;
            btnRefresh.Click -= ApplicationBarIconButton_RefreshLiveChannel_Click;

            if (MainPanorama.SelectedIndex == 0)
            {
                btnRefresh.Click += ApplicationBarIconButton_Refresh_Click;
                if (!isLoadWhatsNew)
                    loadWhatsNew();
            }
            else if (MainPanorama.SelectedIndex == 1)
            {
                btnRefresh.Click += ApplicationBarIconButton_RefreshCategory_Click;
                if (!isLoadCategory) 
                    loadCategory();           
            }
            else if (MainPanorama.SelectedIndex == 2)
            {
                btnRefresh.Click += ApplicationBarIconButton_RefreshChannel_Click;
                //HubTileService.FreezeGroup("LiveChannel");
                //HubTileService.UnfreezeGroup("Channel");
                if (!isLoadChannel)
                    loadChannel();
            }
            else if (MainPanorama.SelectedIndex == 3)
            {
                btnRefresh.Click += ApplicationBarIconButton_RefreshLiveChannel_Click;
                //HubTileService.FreezeGroup("Channel");
                //HubTileService.UnfreezeGroup("LiveChannel");
                if (!isLoadLiveChannel)
                loadLiveChannel();
            }
        }

        private void loadWhatsNew()
        {
            //var entrypoint = new ShellTileSchedule();
            //entrypoint.RemoteImageUri = new Uri("http://www.makathon.com/placeholder.png");
            //entrypoint.Interval = UpdateInterval.EveryHour;
            //entrypoint.Recurrence = UpdateRecurrence.Interval;
            //entrypoint.StartTime = DateTime.Now;
            //entrypoint.Start();

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Applications unable to connect to internet");
            }
            else
            {
                SystemTray.IsVisible = loadingProgressBar.IsVisible = true;
                string url = Utility.Instance.getUrlWhatsNew(0);
                Uri whatsNewUri = new Uri(url);
                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(whatsNew_DownloadStringCompleted);
                webClient.DownloadStringAsync(whatsNewUri);
            }
        }

        void whatsNew_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            SystemTray.IsVisible = loadingProgressBar.IsVisible = false;
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                isLoadWhatsNew = true;
                JObject json = JObject.Parse(e.Result);
                programItems.Clear();
                JToken programs = json["programs"];
                foreach (JToken program in programs)
                {
                    ProgramItem programItem = new ProgramItem();
                    programItem.program_id = program["id"].Value<string>();
                    programItem.title = program["title"].Value<string>();
                    programItem.thumbnail = program["thumbnail"].Value<string>();
                    programItem.description = program["description"].Value<string>();
                    programItems.Add(programItem);

                }
                Dispatcher.BeginInvoke(
                    () =>
                    {
                        ListBox_WhatsNew.ItemsSource = null;
                        ListBox_WhatsNew.ItemsSource = programItems;
                        //listboxDataBinding.ItemsSource = null;
                        //listboxDataBinding.ItemsSource = programItems;
                    }
                );
            }
        }

        private void loadCategory()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Applications unable to connect to internet");
            }
            else
            {
                SystemTray.IsVisible = loadingProgressBar.IsVisible = true;
                string url = Utility.Instance.getUrlCategory();
                Uri whatsNewUri = new Uri(url);
                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(category_DownloadStringCompleted);
                webClient.DownloadStringAsync(whatsNewUri);
            }
        }

        void category_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            SystemTray.IsVisible = loadingProgressBar.IsVisible = false;
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                isLoadCategory = true;
                JObject json = JObject.Parse(e.Result);
                categoryItems.Clear();
                JToken categories = json["categories"];
                foreach (JToken category in categories)
                {
                    CategoryItem categoryItem = new CategoryItem();
                    categoryItem.id = category["id"].Value<string>();
                    categoryItem.title = category["title"].Value<string>();
                    categoryItem.description = category["description"].Value<string>();
                    categoryItem.thumbnail = category["thumbnail"].Value<string>();
                    categoryItems.Add(categoryItem);
                }
                Dispatcher.BeginInvoke(
                    () =>
                    {
                        ListBox_Category.ItemsSource = null;
                        ListBox_Category.ItemsSource = categoryItems;
                    }
                );
            }
        }

        private void loadChannel()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Applications unable to connect to internet");
            }
            else
            {
                SystemTray.IsVisible = loadingProgressBar.IsVisible = true;
                string url = Utility.Instance.getUrlChannel();
                Uri channelUri = new Uri(url);
                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(channel_DownloadStringCompleted);
                webClient.DownloadStringAsync(channelUri);
            }
        }

        void channel_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            SystemTray.IsVisible = loadingProgressBar.IsVisible = false;
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                isLoadChannel = true;
                JObject json = JObject.Parse(e.Result);
                channelItems.Clear();
                JToken channels = json["categories"];
                foreach (JToken channel in channels)
                {
                    ChannelItem channelItem = new ChannelItem();
                    channelItem.id = channel["id"].Value<string>();
                    channelItem.title = channel["title"].Value<string>();
                    channelItem.description = channel["description"].Value<string>();
                    channelItem.thumbnail = channel["thumbnail"].Value<string>();
                    channelItems.Add(channelItem);
                }
                Dispatcher.BeginInvoke(
                    () =>
                    {
                        ListBox_Channel.ItemsSource = null;
                        ListBox_Channel.ItemsSource = channelItems;
                    }
                );
            }
        }

        private void loadLiveChannel()
        {

        }

        private void ListBox_Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Category.SelectedIndex == -1) return;
            CategoryItem selectedCat = categoryItems[ListBox_Category.SelectedIndex];
            NavigationService.Navigate(new Uri("/ProgramPage.xaml?cat_id=" + selectedCat.id + "&cat_name=" + HttpUtility.UrlEncode(selectedCat.title), UriKind.Relative));
            ListBox_Category.SelectedIndex = -1;
        }

        private void ListBox_Channel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Channel.SelectedIndex == -1) return;
            ChannelItem selectedCh = channelItems[ListBox_Channel.SelectedIndex];
            NavigationService.Navigate(new Uri("/ProgramPage.xaml?cat_id=" + ( int.Parse(selectedCh.id) + 100) + "&cat_name=" + HttpUtility.UrlEncode(selectedCh.title), UriKind.Relative));
            ListBox_Channel.SelectedIndex = -1;
        }

        private void ListBox_WhatsNew_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_WhatsNew.SelectedIndex == -1) return;
            ProgramItem selectedProgram = programItems[ListBox_WhatsNew.SelectedIndex];
            NavigationService.Navigate(new Uri("/ProgramPivotPage.xaml?program_id=" + selectedProgram.program_id + "&title=" + HttpUtility.UrlEncode(selectedProgram.title), UriKind.Relative));
            ListBox_WhatsNew.SelectedIndex = -1;
        }

        private void ApplicationBarIconButton_Refresh_Click(object sender, EventArgs e)
        {
            loadWhatsNew();
        }

        private void ApplicationBarIconButton_RefreshCategory_Click(object sender, EventArgs e)
        {
            loadCategory();
        }

        private void ApplicationBarIconButton_RefreshChannel_Click(object sender, EventArgs e)
        {
            loadChannel();
        }

        private void ApplicationBarIconButton_RefreshLiveChannel_Click(object sender, EventArgs e)
        {
            loadLiveChannel();
        }

        private void ApplicationBarIconButton_Search_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SearchPage.xaml", UriKind.Relative));
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {

        }

        private void contextMenuPin_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem selectedListBoxItem = this.ListBox_WhatsNew.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            ProgramItem selectedProgram = selectedListBoxItem.Content as ProgramItem;

            // check if secondary tile is already made and pinned
            ShellTile Tile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("Title=" + selectedProgram.title));
            if (Tile == null)
            {
                // create a new secondary tile
                StandardTileData tileData = new StandardTileData();
                // tile foreground data
                tileData.Title = selectedProgram.title;
                tileData.BackgroundImage = new Uri(selectedProgram.thumbnail);
                tileData.BackTitle = selectedProgram.title;
                tileData.BackContent = selectedProgram.description;
                // create a new tile for this Second Page
                ShellTile.Create(new Uri("/ProgramPivotPage.xaml?program_id=" + selectedProgram.program_id + "&title=" + HttpUtility.UrlEncode(selectedProgram.title), UriKind.Relative), tileData);
            }
        }

        private void loadInHosueAds()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += webClient_DownloadStringCompleted;
            webClient.DownloadStringAsync(new Uri(Utility.Instance.getInHouseAd()));
        }

        void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                try
                {
                    JObject json = JObject.Parse(e.Result);
                    inHouseAds.Clear();
                    JToken ads = json["ads"];
                    foreach (JToken adv in ads)
                    {
                        InHouseAdItem adData = new InHouseAdItem();
                        adData.name = adv["name"].Value<string>();
                        adData.url = adv["url"].Value<string>();
                        adData.time = adv["time"].Value<string>();

                        inHouseAds.Add(adData);
                    }
                    playInHouseAd();
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }

            }
        }

        private void playInHouseAd()
        {
            foreach (InHouseAdItem ads in inHouseAds)
            {
                webInHouseAds.Navigate(new Uri(ads.url, UriKind.Absolute));
            }
        }

    }
}