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

namespace TV_Thailand
{
    public partial class ProgramPivotPage : PhoneApplicationPage
    {
        // Initialize VservSDK
        VservAdControl VAC = VservAdControl.Instance;

        List<ProgramlistItem> programlistItems = new List<ProgramlistItem>();
        string program_id = "";
        ScrollViewer scrollViewer;
        bool isLoadDetail = false;
        bool isEmptyProgramlist = false;

        public ProgramPivotPage()
        {
            InitializeComponent();
            this.Loaded += ProgramPivotPage_Loaded;
            VAC.VservAdNoFill += VAC_VservAdNoFill;

            ListBox_Programlist.Loaded += new RoutedEventHandler(ListBox_Programlist_Loaded);
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
        void ListBox_Programlist_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            element.Loaded -= ListBox_Programlist_Loaded;
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
            ProgramPivotPage page = obj as ProgramPivotPage;

            if (page.isEmptyProgramlist) return;

            ScrollViewer viewer = page.scrollViewer;

            //Checks if the Scroll has reached the last item based on the ScrollableHeight
            bool atBottom = viewer.VerticalOffset >= viewer.ScrollableHeight;

            if (atBottom)
            {
                page.loadProgramlist();
            }
        }

        #endregion

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            YouTube.CancelPlay();
            base.OnNavigatedTo(e);
            string title = "";

            NavigationContext.QueryString.TryGetValue("program_id", out program_id);
            NavigationContext.QueryString.TryGetValue("title", out title);


            MainPivot.Title = title;
            loadProgramlist();
        }

        private void loadProgramlist()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Applications unable to connect to internet");
            }
            else
            {
                SystemTray.IsVisible = loadingProgressBar.IsVisible = true;
                string url = Utility.Instance.getUrlProgramlist(program_id, programlistItems.Count);
                Uri whatsNewUri = new Uri(url);
                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(programlist_DownloadStringCompleted);
                webClient.DownloadStringAsync(whatsNewUri);
            }
        }

        void programlist_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            SystemTray.IsVisible = loadingProgressBar.IsVisible = false;
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {   
                JObject json = JObject.Parse(e.Result);
                JToken jInfo = json["info"];

                if (jInfo != null)
                {
                    string thumbnail = jInfo["thumbnail"].Value<string>();
                    if (thumbnail != "")
                    {
                        Uri uri = new Uri(thumbnail, UriKind.Absolute);
                        ImgProgram.Source = new BitmapImage(uri);
                    }

                    string title = jInfo["title"].Value<string>();
                    string detail = jInfo["detail"].Value<string>();
                    string time = jInfo["description"].Value<string>();
                    string count = jInfo["view_count"].Value<string>();
                    int views = 0;
                    Int32.TryParse(count, out views);
                    string fullDetail = String.Format("{0}\n\n{1}\n\n{2}\n\n{3} Views", title, time, detail, views.ToString("#,###"));
                    txtProgramDetail.Text = fullDetail;
                }



                JToken programlists = json["episodes"];
                isEmptyProgramlist = true;
                foreach (JToken programlist in programlists)
                {
                    isEmptyProgramlist = false;
                    ProgramlistItem programlistItem = new ProgramlistItem(programlist);
                    programlistItems.Add(programlistItem);
                }

                Dispatcher.BeginInvoke(
                    () =>
                    {
                        ListBox_Programlist.ItemsSource = null;
                        ListBox_Programlist.ItemsSource = programlistItems;
                    }
                );
            }
        }

        private void ListBox_Programlist_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListBox_Programlist.SelectedIndex == -1) return;
            ProgramlistItem selectedProgram = programlistItems[ListBox_Programlist.SelectedIndex];

            string videoKeys_decode = Utility.Instance.DecodeVideoKey(selectedProgram.youtube_encrypt);

            string[] videoKeys = videoKeys_decode.Split(new string[]{","}, StringSplitOptions.None);

            if (videoKeys.Length == 0)
            {
                MessageBox.Show("No Video");
            }
            else if (videoKeys.Length == 1)
            {
                if (!Utility.isLoading) Utility.Instance.PlayVideo(selectedProgram.src_type, videoKeys[0], selectedProgram.password);
            }
            else
            {
                NavigationService.Navigate(new Uri("/EpPage.xaml?epname=" + HttpUtility.UrlEncode(selectedProgram.name)
                    + "&videoKeys_decode=" + HttpUtility.UrlEncode(videoKeys_decode) 
                    + "&src_type=" + selectedProgram.src_type
                    + "&password=" + selectedProgram.password, UriKind.Relative));
            }
            ListBox_Programlist.SelectedIndex = -1;
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainPivot.SelectedIndex == 1 && !isLoadDetail)
            {

            }
        }

    }
}