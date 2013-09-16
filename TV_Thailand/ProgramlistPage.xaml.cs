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

namespace TV_Thailand
{
    public partial class ProgramlistPage : PhoneApplicationPage
    {
        List<ProgramlistItem> programlistItems = new List<ProgramlistItem>();
        string program_id = "";
        ScrollViewer scrollViewer;
        bool isEmptyProgramlist = false;

        public ProgramlistPage()
        {
            InitializeComponent();
            ListBox_Programlist.Loaded += new RoutedEventHandler(ListBox_Programlist_Loaded);
            ProgressBar_Page.Visibility = Visibility.Collapsed;
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

        public readonly DependencyProperty ListVerticalOffsetProperty = DependencyProperty.Register("ListVerticalOffset", typeof(double), typeof(ProgramlistPage),
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
            ProgramlistPage page = obj as ProgramlistPage;

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

            PageTitle.Text = title;
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
                ProgressBar_Page.Visibility = Visibility.Visible;
                string url = Utility.Instance.getUrlProgramlist(program_id, programlistItems.Count);
                Uri whatsNewUri = new Uri(url);
                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(programlist_DownloadStringCompleted);
                webClient.DownloadStringAsync(whatsNewUri);
            }
        }

        void programlist_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                ProgressBar_Page.Visibility = Visibility.Collapsed;
                JObject json = JObject.Parse(e.Result);
                JToken programlists = json["programlists"];
                isEmptyProgramlist = true;
                foreach (JToken programlist in programlists)
                {
                    isEmptyProgramlist = false;
                    ProgramlistItem programlistItem = new ProgramlistItem();
                    programlistItem.programlist_id = programlist["programlist_id"].Value<string>();
                    string epname = (programlist["epname"].Value<string>().Equals("")) ? "" : " - " + programlist["epname"].Value<string>();
                    programlistItem.name = "ตอนที่ " + programlist["ep"].Value<string>() + epname;
                    programlistItem.youtube_encrypt = programlist["youtube_encrypt"].Value<string>();
                    programlistItem.src_type = programlist["src_type"].Value<string>();
                    programlistItem.date = "ออกอากาศ " + programlist["date"].Value<string>();
                    int count = Convert.ToInt32(programlist["count"].Value<string>());
                    programlistItem.count = ((count == 0) ? "0" : count.ToString("#,###")) + " Views";
                    programlistItem.password = programlist["pwd"].Value<string>();
                    programlistItems.Add(programlistItem);
                }

                Dispatcher.BeginInvoke(
                    () =>
                    {
                        ProgressBar_Page.Visibility = Visibility.Collapsed;
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
                Utility.Instance.PlayVideo(selectedProgram.src_type, videoKeys[0], selectedProgram.password);
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
    }
}
