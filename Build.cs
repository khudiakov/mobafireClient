using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;

namespace mobafireClient
{
    class Build
    {
        public List<BuildData> Data;
        public Build(string url, string title, string autor, int votes, int procent, bool isComplete, string descript)
        {
            Url = url;
            Title = title;
            Autor = autor;
            Votes = votes;
            Procent = procent;
            IsComplete = isComplete;
            Descript = descript;
            Data = new List<BuildData>();
        } 
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string Autor { get; private set; }
        public int Votes { get; private set; }
        public int Procent { get; private set; }
        public bool IsComplete { get; private set; }
        public string Descript { get; private set; }

        public void GetBuildInfo()
        {
            if (Data.Any()) return;
            var webGet = new HtmlWeb();
            var document = webGet.Load("http://www.mobafire.com" + Url);
            var metaTags = document.DocumentNode.SelectNodes("//div");
            int buildCounter = -1;
            if (metaTags != null)
            {
                foreach (var tag in metaTags)
                {
                    if (tag.Attributes["class"] != null)
                    {
                        //Summons
                        if (tag.Attributes["class"].Value.Equals("build-wrap"))
                        {
                            buildCounter++;
                            Data.Add(new BuildData());
                        }
                        else if (tag.Attributes["class"].Value.Equals("build-title"))
                        {
                            Data[buildCounter].Title = tag.ChildNodes["h2"].InnerText;
                        }
                        else if (tag.Attributes["class"].Value.Equals("build-spells"))
                        {
                            var spells = tag.ChildNodes;
                            foreach (var spell in spells)
                            {
                                if (spell.Name.Equals("a"))
                                {
                                    string url = spell.Attributes["href"].Value;
                                    string imgUrl = spell.ChildNodes["img"].Attributes["src"].Value;

                                    Data[buildCounter].addSummon(url, imgUrl);
                                }
                            }
                        }
                        //Runes
                        else if (tag.Attributes["class"].Value.StartsWith("rune-wrap"))
                        {
                            var spells = tag.ChildNodes;
                            foreach (var spell in spells)
                            {
                                if (spell.Name.Equals("a"))
                                {
                                    string url = spell.Attributes["href"].Value;
                                    string imgUrl = spell.ChildNodes["img"].Attributes["src"].Value;
                                    int count = Convert.ToInt32(Regex.Match(spell.ChildNodes["div"].ChildNodes[3].InnerHtml, @"\d+").Value);
                                    Data[buildCounter].addRunes(url, imgUrl, count);
                                }
                            }
                        }
                        //Items
                        else if (tag.Attributes["class"].Value.StartsWith("item-wrap"))
                        {
                            var itemsCat = Data[buildCounter].createCategoryItems(tag.ChildNodes["h2"].InnerText.Trim());
                            foreach (var item in tag.ChildNodes["div"].ChildNodes)
                            {
                                if (item.Name == "div")
                                {
                                    string url = item.ChildNodes["a"].Attributes["href"].Value;
                                    string imageUrl = item.ChildNodes["a"].ChildNodes["div"].ChildNodes["img"].Attributes["src"].Value;
                                    itemsCat.addItem(url, imageUrl);
                                }
                            }
                        }
                        //Skills
                        else if (tag.Attributes["class"].Value.StartsWith("ability-wrap"))
                        {
                            int cnt = 0;
                            foreach (var item in tag.ChildNodes)
                            {
                                if (item.ChildNodes["div"] != null)
                                {
                                    string url = item.ChildNodes["div"].ChildNodes["a"].Attributes["href"].Value;
                                    string imageUrl = item.ChildNodes["div"].ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                                    Data[buildCounter].Skills.addSkill(url, imageUrl);

                                    var skillLvls = item.ChildNodes[3].ChildNodes;
                                    foreach (var lvl in skillLvls)
                                    {
                                        if (lvl.InnerText.Trim() != "")
                                        {
                                            var index = Convert.ToInt32(lvl.InnerText);
                                            Data[buildCounter].Skills.Sequence[index - 1] = cnt;
                                        }
                                    }
                                    cnt++;
                                }
                            }
                        }
                    }
                }
            }
        }
        public StackPanel ShowBuild(int buildNum = 0)
        {
            var buildStack = new StackPanel{Orientation = Orientation.Vertical};

            buildStack.Children.Add(new TextBlock { Text = Data[buildNum].Title, FontSize = 20});

            var summons = new StackPanel{Orientation = Orientation.Horizontal};
            var stackOfSummons = new WrapPanel { Orientation = Orientation.Horizontal, Width = 500 };
            foreach (var summon in Data[buildNum].Summons)
            {
                Uri summonImgUri = new Uri(DirectoryWorker.DownloadImage("summons", summon.imgUrl, summon.name + ".png"));
                stackOfSummons.Children.Add(addTitle(summonImgUri, summon.name, 40));
            }
            summons.Children.Add(CreateContainer("Summons:", stackOfSummons));
            buildStack.Children.Add(summons);

            var runes = new StackPanel { Orientation = Orientation.Horizontal };
            var stackOfRunes = new WrapPanel {Orientation = Orientation.Horizontal, Width = 500};
            foreach (var rune in Data[buildNum].Runes)
            {
                Uri runeImgUri = new Uri(DirectoryWorker.DownloadImage("runes", rune.Data.imgUrl, rune.Data.name + ".png"));
                stackOfRunes.Children.Add(addTitle(runeImgUri, rune.Data.name.Replace("greater", ""), 50, 20));
            }
            runes.Children.Add(CreateContainer("Runes:", stackOfRunes));
            buildStack.Children.Add(runes);

            var lvls = new StackPanel { Orientation = Orientation.Horizontal };
            var stackOfSkills = new WrapPanel { Orientation = Orientation.Horizontal, Width = 500};
            var lvlCnt = 1;
            foreach (var lvl in Data[buildNum].Skills.Sequence)
            {
                var skillImgUri = new Uri(DirectoryWorker.DownloadImage("skills", Data[buildNum].Skills.SkillData[lvl].imgUrl, Data[buildNum].Skills.SkillData[lvl].name + ".png"));
                stackOfSkills.Children.Add(addTitle(skillImgUri, lvlCnt.ToString(), 30));
                lvlCnt++;
            }
            lvls.Children.Add(CreateContainer("Skills:", stackOfSkills));
            buildStack.Children.Add(lvls);

            var items = new StackPanel { Orientation = Orientation.Vertical };
            foreach (var group in Data[buildNum].GroupOfItems)
            {
                var newStack = new WrapPanel
                {
                    Orientation = Orientation.Horizontal,
                    Height = 50,
                    Margin = new Thickness(0, 3, 0, 3),
                    Width = 500
                };
                foreach (var item in group.Items)
                {
                    var itemImgUri = new Uri(DirectoryWorker.DownloadImage("items", item.imgUrl, item.name + ".png"));
                    var itemImg = new Image {Source = new BitmapImage(itemImgUri), Margin = new Thickness(3, 0, 3, 0)};
                    newStack.Children.Add(itemImg);
                }
                items.Children.Add(CreateContainer(group.name, newStack));
            }
            buildStack.Children.Add(items);
            return buildStack;
        }


        static StackPanel CreateContainer(string name, WrapPanel elements)
        {
            var panel = new StackPanel();
            var title = new TextBlock {Text = name, FontWeight = FontWeights.Bold};
            panel.Children.Add(title);
            panel.Children.Add(elements);
            return panel;
        }

        StackPanel addTitle(Uri imgAddr, string title, int imgSize = 50, int margin = 3)
        {

            var wraper = new StackPanel {VerticalAlignment = VerticalAlignment.Top};

            var imgTitle = new TextBlock();
            imgTitle.Text = title;
            imgTitle.Width = imgSize + margin * 2;
            imgTitle.TextWrapping = TextWrapping.Wrap;
            imgTitle.TextAlignment = TextAlignment.Center;

            var img = new Image
            {
                Margin = new Thickness(margin, 0, margin, 0),
                Width = imgSize,
                Source = new BitmapImage(imgAddr)
            };

            wraper.Children.Add(img);
            wraper.Children.Add(imgTitle);

            return wraper;
        }
    }
}
