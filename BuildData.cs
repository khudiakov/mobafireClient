using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mobafireClient
{
    class BuildData
    {
        public string Title;
        public struct Item
        {
            public int id;
            public string name;
            public string url;
            public string imgUrl;

            public Item(string summonUrl, string summonImg)
            {
                string nameId = summonUrl.Split('/')[summonUrl.Split('/').Length-1];
                int whereCut = nameId.LastIndexOf("-");
                id = Convert.ToInt32(nameId.Substring(whereCut+1));
                name = nameId.Substring(0, whereCut).Replace("-", " ");
                url = summonUrl;
                imgUrl = summonImg;
            }
        }
        public class CategoryItems
        {
            public string name;
            public List<Item> Items = new List<Item>();
            public void addItem(string runeUrl, string runeImg)
            {
                Items.Add(new Item(runeUrl, runeImg));
            }
        }
        public class SkillsUpgrades
        {
            public List<Item> SkillData = new List<Item>();
            public int[] Sequence = new int[18];
            public void addSkill(string summonUrl, string summonImg)
            {
                SkillData.Add(new Item(summonUrl, summonImg));
            }
        }
        public class Rune
        {
            public Item Data;
            public int Count;
            public Rune(string runeUrl, string runeImg, int runeCount)
            {
                Data = new Item(runeUrl, runeImg);
                Count = runeCount;
            }
        }

        public List<Item> Summons = new List<Item>();
        public List<Rune> Runes = new List<Rune>();
        public List<CategoryItems> GroupOfItems = new List<CategoryItems>();
        public SkillsUpgrades Skills = new SkillsUpgrades();

        public void addSummon(string summonUrl, string summonImg)
        {
            Summons.Add(new Item(summonUrl, summonImg));
        }

        public void addRunes(string runeUrl, string runeImg, int runeCount)
        {
            Runes.Add(new Rune(runeUrl, runeImg, runeCount));
        }

        public CategoryItems createCategoryItems(string categoryName)
        {
            CategoryItems newCat = new CategoryItems();
            newCat.name = categoryName;
            GroupOfItems.Add(newCat);
            return newCat;
        }
    }
}
