using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;


namespace mobafireClient
{
    class Champion
    {
        public List<Build> BuildList;
        public Champion(int id, string name, string imgPath, string buildsUrl)
        {
            Id = id;
            Name = name;
            ImgPath = imgPath;
            BuildsUrl = buildsUrl;
            BuildList = new List<Build>();
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string ImgPath { get; private set; }
        public string BuildsUrl { get; private set; }

        public void GetBuilds()
        {
            if (BuildList.Any()) return;
            string descript = "";
            var webGet = new HtmlWeb();
            var document = webGet.Load("http://www.mobafire.com" + BuildsUrl);
            var metaTags = document.DocumentNode.SelectNodes("//tr");
            if (metaTags != null)
            {
                foreach (var tag in metaTags)
                {
                    if (tag.Attributes["class"] == null)
                    {
                        if (tag.ChildNodes["td"].Attributes["class"] != null &&
                            tag.ChildNodes["td"].Attributes["class"].Value.Equals("section"))
                        {
                            descript = tag.ChildNodes["td"].InnerText;
                        }
                        continue;
                    }
                    bool isComplete;
                    if (tag.Attributes["class"].Value.IndexOf("complete-build")>=0) isComplete = true;
                    else if (tag.Attributes["class"].Value.IndexOf("partial-build")>=0) isComplete = false;
                    else continue;
                    string url = "";
                    string title = "";
                    string autor = "";
                    int votes = 0;
                    int procent = 0;

                    var buildData = tag.ChildNodes;
                    foreach (var el in buildData.Where(el => el.Name.Equals("td")))
                    {
                        if (el.Attributes["class"].Value.Equals("desc"))
                        {
                            url = el.ChildNodes["div"].ChildNodes["a"].Attributes["href"].Value;
                            title = el.ChildNodes["div"].ChildNodes["a"].InnerText;
                            autor = el.ChildNodes["div"].ChildNodes["span"].ChildNodes["a"].InnerText;
                        }
                        else if (el.Attributes["class"].Value.Equals("c"))
                        {
                            var rankData = el.ChildNodes["div"].ChildNodes;
                            foreach (var rank in rankData.Where(rank => rank.Attributes["class"] != null && rank.Name.Equals("div")))
                            {
                                if (rank.Attributes["class"].Value.Equals("rank"))
                                {
                                    votes = Convert.ToInt32(rank.InnerText.Split(' ')[0].Replace(",",""));

                                }
                                else if (rank.Attributes["class"].Value.Equals("bar-wrap"))
                                {
                                    procent =
                                        Convert.ToInt32(rank.ChildNodes["div"].ChildNodes["div"].Attributes["style"].Value.Substring(19, 2));
                                }
                            }
                        }
                    }
                    BuildList.Add(new Build(url, title, autor, votes, procent, isComplete, descript));
                }
            }
        }

    }
}
