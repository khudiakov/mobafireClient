using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace mobafireClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        private const string ChampionsUrl = "http://www.mobafire.com/league-of-legends/champions";
        private int _champId;
        private int _buildId;
        private List<Champion> Champions = new List<Champion>();

        public MainWindow()
        {
            InitializeComponent();
            GetChamps();
            ShowChamps();
        }

        void GetChamps()
        {
            var webGet = new HtmlWeb();
            var document = webGet.Load(ChampionsUrl);
            var metaTags = document.DocumentNode.SelectNodes("//a");
            if (metaTags != null)
            {
                foreach (var tag in metaTags)
                {
                    if (tag.Attributes["class"] != null)
                    {
                        if (tag.Attributes["class"].Value.StartsWith("champ-box"))
                        {
                            string url = tag.Attributes["href"].Value;
                            string imgUrl = tag.ChildNodes["img"].Attributes["src"].Value;
                            string nameId = url.Split('/')[url.Split('/').Count() - 1];

                            string name = nameId.Substring(0, nameId.LastIndexOf('-'));
                            int id = Convert.ToInt32(nameId.Substring(nameId.LastIndexOf('-')+1));

                            string imgPath = DirectoryWorker.DownloadImage("champs", imgUrl, name+".png");

                            Champions.Add(new Champion(id, name, imgPath, url));
                        }
                    }
                }
            }
        }

        void ShowChamps(string filtr = null)
        {
            int i = -1;
            ChampsPanel.Children.Clear();
            foreach (var champ in Champions)
            {
                i++;
                if (filtr != null)
                {
                    if (!champ.Name.StartsWith(filtr)) continue;
                }
                var button = new Button();
                var itemImg = new Image();
                itemImg.Height = 50;
                itemImg.Source = new BitmapImage(new Uri(champ.ImgPath));
                button.Content = itemImg;
                button.Tag = i;
                button.Click += ShowBuild;
                ChampsPanel.Children.Add(button);
            }
        }

        void ShowBuild(Object sender, EventArgs e)
        {
            SearchChamp.Text = "";
            BuildsBox.SelectedIndex = -1;
            BuildViewer.Content = null;
            BuildPages.Children.Clear();
            _champId = Convert.ToInt32((sender as Button).Tag);
            Champions[_champId].GetBuilds();
            Console.Text = "";
            BuildsBox.Items.Clear();
            var descript = "";
            var i = 0;
            foreach (var build in Champions[_champId].BuildList)
            {
                var stack = new StackPanel {Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Left};
                var textColor = build.IsComplete ? Brushes.White : Brushes.Silver;
                var text = new TextBlock { Text = build.Title, Width = 180, TextWrapping = TextWrapping.Wrap, Foreground = textColor };
                var autor = new TextBlock { Text = "by " + build.Autor, Width = 180, FontSize = 10, Foreground = textColor, TextAlignment = TextAlignment.Right };
                var progress = new ProgressBar {Height = 2, Width = 180, Value = build.Procent*10};

                if (!descript.Equals(build.Descript))
                {
                    descript = build.Descript;
                    BuildsBox.Items.Add(new ListBoxItem { Content = new TextBlock{ Text = descript, FontSize = 20, Foreground = Brushes.LightSteelBlue}, IsEnabled = false});
                }

                stack.Children.Add(text);
                stack.Children.Add(autor);
                stack.Children.Add(progress);
                var newItem = new ListBoxItem {Content = stack, Tag = i++};
                newItem.Selected += BuildPick;
                BuildsBox.Items.Add(newItem);
            }

        }

        private void BuildPick(object sender,  EventArgs e)
        {
            _buildId = Convert.ToInt32((sender as ListBoxItem).Tag);
            if (_buildId < 0) return;
            BuildViewer.Content = null;
            BuildPages.Children.Clear();
            Champions[_champId].BuildList[_buildId].GetBuildInfo();
            for (var i = 0; i < Champions[_champId].BuildList[_buildId].Data.Count(); i++)
            {
                var newButton = new Button { Height = 18, Width = 60, Content = i.ToString() };
                newButton.Click += delegate 
                {
                    BuildViewer.Content = Champions[_champId].BuildList[_buildId].ShowBuild(Convert.ToInt32(newButton.Content.ToString()));
                    Console.Text = newButton.Content.ToString();
                };
                BuildPages.Children.Add(newButton);
            }
            BuildViewer.Content = Champions[_champId].BuildList[_buildId].ShowBuild();
        }

        private void SearchChamp_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ShowChamps((sender as TextBox).Text.ToLower());
        }
    }
}
