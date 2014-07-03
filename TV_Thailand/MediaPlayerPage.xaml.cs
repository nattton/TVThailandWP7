using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TV_Thailand.Model;
using System.Windows.Media.Imaging;

namespace TV_Thailand
{
    public partial class MediaPlayerPage : PhoneApplicationPage
    {
        OTVPartItem partItem;
        public MediaPlayerPage()
        {
            InitializeComponent();
            if (PhoneApplicationService.Current.State.ContainsKey("StreamURL"))
            {
                string streamURL = (string)PhoneApplicationService.Current.State["StreamURL"];
                mediaPlayer.Source = new Uri(streamURL);
            }
            else if (PhoneApplicationService.Current.State.ContainsKey("OTVPart"))
            {
                partItem = (OTVPartItem)PhoneApplicationService.Current.State["OTVPart"];
                mediaPlayer.Source = new Uri(partItem.streamURL);
            }

            ImageThumbnail.Visibility = Visibility.Collapsed;
            if (PhoneApplicationService.Current.State.ContainsKey("ThumbnailURL"))
            {
                string thumbnailURL = (string)PhoneApplicationService.Current.State["ThumbnailURL"];
                ImageThumbnail.Source = new BitmapImage(new Uri(thumbnailURL, UriKind.Absolute));
                ImageThumbnail.Visibility = Visibility.Visible;
                PhoneApplicationService.Current.State.Remove("ThumbnailURL");
            }
        }
    }
}