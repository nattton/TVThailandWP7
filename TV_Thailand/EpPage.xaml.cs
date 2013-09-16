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
using Microsoft.Phone.Tasks;
using MyToolkit.Multimedia;

namespace TV_Thailand
{
    public partial class EpPage : PhoneApplicationPage
    {
        string src_type = "";
        string password = "";
        List<EpItem> epItems = new List<EpItem>();

        public EpPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            YouTube.CancelPlay();
            base.OnNavigatedTo(e);
            string epname = "";
            string videoKeys_decode = "";

            NavigationContext.QueryString.TryGetValue("epname", out epname);
            NavigationContext.QueryString.TryGetValue("src_type", out src_type);
            NavigationContext.QueryString.TryGetValue("videoKeys_decode", out videoKeys_decode);
            NavigationContext.QueryString.TryGetValue("password", out password);

            PageTitle.Text = epname;

            InitialVideoKeys(src_type, videoKeys_decode);
        }

        private void InitialVideoKeys(string src_type, string videoKeys_decode)
        {
            string[] videoKeys = videoKeys_decode.Split(new string[] { "," }, StringSplitOptions.None);

            for (int i = 0; i < videoKeys.Length; i++)
            {
                EpItem epItem = new EpItem();
                epItem.epname = "ตอนที่ " + (i + 1).ToString() + " / " + videoKeys.Length.ToString();
                epItem.videoKey = videoKeys[i];
                epItem.thumbnail = Utility.Instance.videoThumbnail(epItem.videoKey, src_type);
                epItems.Add(epItem);
            }

            ListBox_Ep.ItemsSource = epItems;
        }

        private void ListBox_Ep_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Ep.SelectedIndex == -1) return;
            string videoKey = epItems[ListBox_Ep.SelectedIndex].videoKey;
            Utility.Instance.PlayVideo(src_type, videoKey, password);
            //NavigationService.Navigate(new Uri("/WebPage.xaml?url=" + HttpUtility.UrlEncode(url), UriKind.Relative));
            ListBox_Ep.SelectedIndex = -1;
        }

        #region InMobi

        //Invoked when an exception is raised from IMAdView
        private void AdView1_AdRequestFailed(InMobi.WpSdk.IMAdView IMAdView, InMobi.WpSdk.IMAdViewErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.ErrorCode.ToString() + e.ErrorDescription.ToString());
        }

        //Invoked when Ad is loaded
        private void AdView1_AdRequestLoaded(InMobi.WpSdk.IMAdView IMAdView, InMobi.WpSdk.IMAdViewSuccessEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.ResponseCode.ToString() + e.ResponseDescription.ToString());
        }

        //Invoked when full screen Ad displayed is closed
        private void AdView1_DismissFullAdScreen(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Full screen closed");
        }

        //Invoked when the navigating away from current page as Click To Action on IMAdView 
        private void AdView1_LeaveApplication(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Moving out of application");
        }

        //Invoked when the full screen ad has been opened, but not yet fully loaded
        private void AdView1_ShowFullAdScreen(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Displaying full screen");
        }

        #endregion
    }
}