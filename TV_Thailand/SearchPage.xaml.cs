using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Data;
using System.Net.NetworkInformation;
using System.Windows.Input;

namespace TV_Thailand
{
    public partial class SearchPage : PhoneApplicationPage
    {
        List<ProgramItem> programItems = new List<ProgramItem>();

        public SearchPage()
        {
            InitializeComponent();
            ProgressBar_Page.Visibility = Visibility.Collapsed;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox_Search.Focus();
        }

        private void TextBox_Search_KeyUp(object sender, KeyEventArgs e)
        {
            loadProgramSearch(TextBox_Search.Text, 0);
            if (e.Key == Key.Enter)
            {
                this.Focus();
            }
        }

        private void loadProgramSearch(string keyword, int start)
        {
            if (keyword.Length > 0)
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    MessageBox.Show("Applications unable to connect to internet");
                }
                else
                {
                    ProgressBar_Page.Visibility = Visibility.Visible;
                    string url = Utility.Instance.getUrlProgramSearch(keyword, start);
                    Uri searchUri = new Uri(url);
                    WebClient webClient = new WebClient();
                    webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(program_DownloadStringCompleted);
                    webClient.DownloadStringAsync(searchUri);
                }
            }
        }

        private void program_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                programItems.Clear();

                JObject json = JObject.Parse(e.Result);
                JToken programs = json["programs"];

                foreach (JToken program in programs)
                {
                    ProgramItem programItem = new ProgramItem(program);
                    programItems.Add(programItem);
                }

                Dispatcher.BeginInvoke(
                    () =>
                    {
                        ProgressBar_Page.Visibility = Visibility.Collapsed;
                        ListBox_Search.ItemsSource = null;
                        ListBox_Search.ItemsSource = programItems;
                    }
                );
            }
        }

        private void ListBox_Search_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Search.SelectedIndex == -1) return;
            ProgramItem selectedProgram = programItems[ListBox_Search.SelectedIndex];
            NavigationService.Navigate(new Uri("/ProgramPivotPage.xaml?program_id=" + selectedProgram.program_id + "&title=" + HttpUtility.UrlEncode(selectedProgram.title), UriKind.Relative));
            ListBox_Search.SelectedIndex = -1;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {

        }

        private void contextMenuPin_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem selectedListBoxItem = this.ListBox_Search.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
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

    }
}