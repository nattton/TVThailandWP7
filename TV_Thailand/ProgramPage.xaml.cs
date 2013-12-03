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
using System.Windows.Data;
using System.Net.NetworkInformation;
using Microsoft.Phone.Shell;

namespace TV_Thailand
{
    public partial class ProgramPage : PhoneApplicationPage
    {
        List<ProgramItem> programItems = new List<ProgramItem>();
        string cat_id = "";
        ScrollViewer scrollViewer;
        bool isEmptyProgram = false;

        public ProgramPage()
        {
            InitializeComponent();
            ListBox_Program.Loaded += new RoutedEventHandler(ListBox_Program_Loaded);
        }


        #region Load More
        void ListBox_Program_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            element.Loaded -= ListBox_Program_Loaded;
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

        public readonly DependencyProperty ListVerticalOffsetProperty = DependencyProperty.Register("ListVerticalOffset", typeof(double), typeof(ProgramPage),
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
            ProgramPage page = obj as ProgramPage;

            if (page.isEmptyProgram) return;

            ScrollViewer viewer = page.scrollViewer;

            //Checks if the Scroll has reached the last item based on the ScrollableHeight
            bool atBottom = viewer.VerticalOffset >= viewer.ScrollableHeight;

            if (atBottom)
            {
                page.loadProgram();
            }
        }

        #endregion

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string cat_name = "";

            NavigationContext.QueryString.TryGetValue("cat_id", out cat_id);
            NavigationContext.QueryString.TryGetValue("cat_name", out cat_name);

            PageTitle.Text = cat_name;
            loadProgram();
        }

        private void loadProgram()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Applications unable to connect to internet");
            }
            else
            {
                SystemTray.IsVisible = loadingProgressBar.IsVisible = true;
                string url = Utility.Instance.getUrlProgram(cat_id, programItems.Count);
                //string url = Utility.Domain + "/api/getProgram/" + cat_id + "/" + programItems.Count.ToString();
                Uri whatsNewUri = new Uri(url);
                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(program_DownloadStringCompleted);
                webClient.DownloadStringAsync(whatsNewUri);
            }
        }

        void program_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            SystemTray.IsVisible = loadingProgressBar.IsVisible = false;
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                JObject json = JObject.Parse(e.Result);
                JToken programs = json["programs"];

                isEmptyProgram = true;
                foreach (JToken program in programs)
                {
                    isEmptyProgram = false;
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
                        ListBox_Program.ItemsSource = null;
                        ListBox_Program.ItemsSource = programItems;
                    }
                );
            }
        }

        private void ListBox_Program_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Program.SelectedIndex == -1) return;
            ProgramItem selectedProgram = programItems[ListBox_Program.SelectedIndex];
            NavigationService.Navigate(new Uri("/ProgramPivotPage.xaml?program_id=" + selectedProgram.program_id + "&title=" + HttpUtility.UrlEncode(selectedProgram.title), UriKind.Relative));
            ListBox_Program.SelectedIndex = -1;

        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {

        }

        private void contextMenuPin_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem selectedListBoxItem = this.ListBox_Program.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
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

        private void ApplicationBarIconButton_Refresh_Click(object sender, EventArgs e)
        {
            loadProgram();
        }
    }
}
