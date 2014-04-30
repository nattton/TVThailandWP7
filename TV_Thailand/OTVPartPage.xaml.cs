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
using TV_Thailand.Model;
using Microsoft.Phone.Shell;

namespace TV_Thailand
{
    public partial class OTVPartPage : PhoneApplicationPage
    {
        // Initialize VservSDK
        VservAdControl VAC = VservAdControl.Instance;

        OTVEpisodeItem episode = new OTVEpisodeItem();

        public OTVPartPage()
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


            episode = (OTVEpisodeItem)PhoneApplicationService.Current.State["OTVEpisode"];


            PageTitle.Text = episode.nameTh;

            ListBox_Ep.ItemsSource = episode.parts;
        }

        private void ListBox_Ep_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Ep.SelectedIndex == -1) return;
            
            //NavigationService.Navigate(new Uri("/WebPage.xaml?url=" + HttpUtility.UrlEncode(url), UriKind.Relative));



            ListBox_Ep.SelectedIndex = -1;
        }
    }
}