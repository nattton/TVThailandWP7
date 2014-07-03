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
using vservWindowsPhone;

namespace TV_Thailand
{
    public partial class EpPage : PhoneApplicationPage
    {
        // Initialize VservSDK
        VservAdControl VAC = VservAdControl.Instance;

        string src_type = "";
        string password = "";
        List<EpItem> epItems = new List<EpItem>();

        public EpPage()
        {
            InitializeComponent();

            this.Loaded += EpPage_Loaded;

            VAC.VservAdNoFill += VAC_VservAdNoFill;
        }

        void VAC_VservAdNoFill(object sender, EventArgs e)
        {
            adGrid.Visibility = Visibility.Collapsed;
        }

        void EpPage_Loaded(object sender, RoutedEventArgs e)
        {
            VAC.RenderAd("562db24e", adGrid);
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

            for (int i = 0, length = videoKeys.Length; i < length; i++)
            {
                EpItem epItem = new EpItem(i, length, videoKeys[i], src_type);
                epItems.Add(epItem);
            }

            ListBox_Ep.ItemsSource = epItems;
        }

        private void ListBox_Ep_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Ep.SelectedIndex == -1) return;
            string videoKey = epItems[ListBox_Ep.SelectedIndex].videoKey;
            if (!Utility.isLoading) Utility.Instance.PlayVideo(src_type, videoKey, password);
            ListBox_Ep.SelectedIndex = -1;
        }
    }
}