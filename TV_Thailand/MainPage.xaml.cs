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
using Microsoft.Phone.Tasks;
using vservWindowsPhone;

using TV_Thailand.Class;
using TV_Thailand.Model;

namespace TV_Thailand
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Initialize VservSDK
        VservAdControl VAC = VservAdControl.Instance;

        List<CategoryItem> categoryItems = new List<CategoryItem>();
        List<ChannelItem> channelItems = new List<ChannelItem>();
        List<RadioItem> radioItems = new List<RadioItem>();
        List<ProgramItem> programItems = new List<ProgramItem>();
        List<InHouseAdItem> inHouseAds = new List<InHouseAdItem>();

        bool isLoadRecents = false;
        bool isLoadCategory = false;
        bool isLoadChannel = false;
        bool isLoadRadio = false;

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
            ListBox_Channel.SelectionChanged += ListBox_Channel_SelectionChanged;
            ListBox_Radio.SelectionChanged += ListBox_Radio_SelectionChanged;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // VAC.DisplayAd("c84927ed", LayoutRoot);

            Visibility v = (Visibility)Resources["PhoneLightThemeVisibility"];

            string strBGPano = "Images/PanoramaDark.jpg";
            if (v == System.Windows.Visibility.Visible)
            {
                // Is light theme
                strBGPano = "Images/PanoramaLight.jpg";
            }
            else
            {
                // Is dark theme
                strBGPano = "Images/PanoramaDark.jpg";
            }

            BitmapImage bitmapImage = new BitmapImage(new Uri(strBGPano, UriKind.Relative));
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = bitmapImage;
            MainPanorama.Background = imageBrush;

            loadRecents();
            loadInHosueAds();
            loadSection();
        }

        private void panorama_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplicationBarIconButton btnRefresh = (ApplicationBarIconButton)ApplicationBar.Buttons[0];

            btnRefresh.Click -= ApplicationBarIconButton_RefreshCategory_Click;
            btnRefresh.Click -= ApplicationBarIconButton_Refresh_Click;
            btnRefresh.Click -= ApplicationBarIconButton_RefreshChannel_Click;

            if (MainPanorama.SelectedIndex == 0)
            {
                btnRefresh.Click += ApplicationBarIconButton_Refresh_Click;
                if (!isLoadRecents)
                    loadRecents();
            }
            else if (MainPanorama.SelectedIndex == 1)
            {
                btnRefresh.Click += ApplicationBarIconButton_RefreshCategory_Click;
                if (!isLoadCategory)
                    loadSection();           
            }
            else if (MainPanorama.SelectedIndex == 2)
            {
                btnRefresh.Click += ApplicationBarIconButton_RefreshChannel_Click;
                //HubTileService.FreezeGroup("LiveChannel");
                //HubTileService.UnfreezeGroup("Channel");
                if (!isLoadChannel)
                    loadSection();
            }
            else if (MainPanorama.SelectedIndex == 3)
            {
                btnRefresh.Click += ApplicationBarIconButton_RefreshCategory_Click;
                if (!isLoadRadio)
                    loadSection();
            }
        }

        private void loadRecents()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Applications unable to connect to internet");
            }
            else
            {
                SystemTray.IsVisible = loadingProgressBar.IsVisible = true;
                string url = Utility.Instance.getUrlCategory("recents", 0);
                Uri recentsUri = new Uri(url);
                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(recents_DownloadStringCompleted);
                webClient.DownloadStringAsync(recentsUri);
            }
        }

        void recents_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            SystemTray.IsVisible = loadingProgressBar.IsVisible = false;
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                isLoadRecents = true;
                JObject json = JObject.Parse(e.Result);
                programItems.Clear();
                JToken programs = json["programs"];
                foreach (JToken program in programs)
                {
                    ProgramItem programItem = new ProgramItem(program);
                    programItems.Add(programItem);

                }
                Dispatcher.BeginInvoke(
                    () =>
                    {
                        ListBox_Recent.ItemsSource = null;
                        ListBox_Recent.ItemsSource = programItems;
                    }
                );
            }
        }

        private void loadSection()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Applications unable to connect to internet");
            }
            else
            {
                SystemTray.IsVisible = loadingProgressBar.IsVisible = true;
                Uri sectionUri = new Uri(Utility.Instance.getUrlSection());
                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(section_DownloadStringCompleted);
                webClient.DownloadStringAsync(sectionUri);
            }
        }

        void section_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            SystemTray.IsVisible = loadingProgressBar.IsVisible = false;
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                JObject json = JObject.Parse(e.Result);
                JToken categories = json["categories"];
                JToken channels = json["channels"];
                JToken radios = json["radios"];

                categoriesParser(categories);
                channelsParser(channels);
                radiosParser(radios);
            }
        }

        private void categoriesParser(JToken categories)
        {
            isLoadCategory = true;
            categoryItems.Clear();
            foreach (JToken category in categories)
            {
                CategoryItem categoryItem = new CategoryItem(category);
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

        private void channelsParser(JToken channels)
        {
            isLoadChannel = true;
            channelItems.Clear();
            foreach (JToken channel in channels)
            {
                ChannelItem channelItem = new ChannelItem(channel);
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

        private void radiosParser(JToken radios)
        {
            radioItems.Clear();
            foreach (JToken radio in radios)
            {
                RadioItem radioItem = new RadioItem(radio);
                radioItems.Add(radioItem);
            }

            Dispatcher.BeginInvoke(
                () =>
                {
                    ListBox_Radio.ItemsSource = null;
                    ListBox_Radio.ItemsSource = radioItems;

                }
                );
        }


        private void ListBox_Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Category.SelectedIndex == -1) return;
            CategoryItem selectedCat = categoryItems[ListBox_Category.SelectedIndex];
            PhoneApplicationService.Current.State["CategoryItem"] = selectedCat;
            NavigationService.Navigate(new Uri("/ProgramPage.xaml?mode=cat&id=" + selectedCat.id + "&title=" + HttpUtility.UrlEncode(selectedCat.title), UriKind.Relative));
            ListBox_Category.SelectedIndex = -1;
        }

        private void ListBox_Channel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Channel.SelectedIndex == -1) return;
            ChannelItem selectedCh = channelItems[ListBox_Channel.SelectedIndex];
            if (selectedCh.url.Length > 0)
            {
                if (!selectedCh.hasShow)
                {
                    NavigateToPlayStream(selectedCh.url);
                }
                else
                {
                    CustomMessageBox messageBox = new CustomMessageBox()
                    {
                        Caption = "เลือกรายการ",
                        Message = "เลือกรายการ",
                        LeftButtonContent = "รายการย้อนหลัง",
                        RightButtonContent = "ดูสด"
                    };

                    messageBox.Dismissed += (s1, e1) =>
                        {
                            switch (e1.Result)
                            {
                                case CustomMessageBoxResult.LeftButton:
                                    NavigateToChannel(selectedCh);
                                    break;
                                case CustomMessageBoxResult.RightButton:
                                    NavigateToPlayStream(selectedCh.url);
                                    break;
                                default:
                                    break;
                            }
                        };
                    messageBox.Show();
                }
            }
            else
            {
                NavigateToChannel(selectedCh);
            }
            
            ListBox_Channel.SelectedIndex = -1;
        }

        private void NavigateToChannel(ChannelItem channel)
        {
            PhoneApplicationService.Current.State["ChannelItem"] = channel;
            NavigationService.Navigate(new Uri("/ProgramPage.xaml?mode=ch&id=" + channel.id + "&title=" + HttpUtility.UrlEncode(channel.title), UriKind.Relative));
        }

        private void NavigateToPlayStream(string url)
        {
            PhoneApplicationService.Current.State["StreamURL"] = url;
            NavigationService.Navigate(new Uri("/MediaPlayerPage.xaml", UriKind.Relative));
        }

        private void NavigateToPlayStream(string url, string thumbnail)
        {
            PhoneApplicationService.Current.State["StreamURL"] = url;
            PhoneApplicationService.Current.State["ThumbnailURL"] = thumbnail;
            NavigationService.Navigate(new Uri("/MediaPlayerPage.xaml", UriKind.Relative));
        }

        private void ListBox_Radio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Radio.SelectedIndex == -1) return;
            RadioItem selectedRadio = radioItems[ListBox_Radio.SelectedIndex];
            NavigateToPlayStream(selectedRadio.url, selectedRadio.thumbnail);
            ListBox_Radio.SelectedIndex = -1;
        }

        private void ListBox_Recent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Recent.SelectedIndex == -1) return;
            ProgramItem selectedProgram = programItems[ListBox_Recent.SelectedIndex];
            //if (selectedProgram.is_otv)
            //{
            //    NavigationService.Navigate(new Uri("/OTVShowPivotPage.xaml?title=" + HttpUtility.UrlEncode(selectedProgram.title)
            //        + "&otv_id=" + selectedProgram.otv_id
            //        + "&otv_api_name=" + selectedProgram.otv_api_name, UriKind.Relative));
            //}
            //else
            //{
            //    NavigationService.Navigate(new Uri("/ProgramPivotPage.xaml?program_id=" + selectedProgram.program_id + "&title=" + HttpUtility.UrlEncode(selectedProgram.title), UriKind.Relative));
            //}

            NavigationService.Navigate(new Uri("/ProgramPivotPage.xaml?program_id=" + selectedProgram.program_id + "&title=" + HttpUtility.UrlEncode(selectedProgram.title), UriKind.Relative));

            ListBox_Recent.SelectedIndex = -1;
        }

        private void ApplicationBarIconButton_Refresh_Click(object sender, EventArgs e)
        {
            loadRecents();
        }

        private void ApplicationBarIconButton_RefreshCategory_Click(object sender, EventArgs e)
        {
            loadSection();
        }

        private void ApplicationBarIconButton_RefreshChannel_Click(object sender, EventArgs e)
        {
            loadSection();
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
            ListBoxItem selectedListBoxItem = this.ListBox_Recent.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
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
                    foreach (JToken ad in ads)
                    {
                        InHouseAdItem adData = new InHouseAdItem(ad);
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
                if(ads.name.ToLower().Equals("kapook"))
                {
                    playKapookAds();
                }
                else
                {
                    webInHouseAds.Navigate(new Uri(ads.url, UriKind.Absolute));
                }
            }
        }

        private void playKapookAds()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
            {
                if (e.Error == null)
                {
                    try
                    {
                        JObject json = JObject.Parse(e.Result);
                        string url1x1 = json["url_1x1"].Value<string>();
                        string urlShow = json["url_show"].Value<string>();
                        webInHouseAds.Navigate(new Uri(urlShow, UriKind.Absolute));
                        web1x1.Navigate(new Uri(url1x1, UriKind.Absolute));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            };
            webClient.DownloadStringAsync(new Uri(Utility.Instance.getKapookAds()));
        }

    }
}