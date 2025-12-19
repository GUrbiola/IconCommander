using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KeywordLoader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DbConx conx = new DbConx("Data Source=Main\\SQLSERVER;Initial Catalog=Northwind;Integrated Security=False;Persist Security Info=True;User ID=SqlAdmin;Password=SqlAdmin123");

            LoadIconexToDb(conx, @"D:\Icons\iconex_v5\v_collection\_iconex_system\_scripts\icon_keyword_table2.js", "v5");
            LoadIconexToDb(conx, @"D:\Icons\iconex_g2\g_collection\_iconex_system\_scripts\icon_keyword_table2.js", "g2");


        }

        public static void LoadIconexToDb(DbConx conx, string filePath, string prefix)
        {

            IconexMapper KeyWordsToIconIds = new IconexMapper("Keywords", "IconIds");
            IconexMapper IconsToKeyWordIds = new IconexMapper("Icons", "Keywords");
            IconexMapper KeyWordsToKeywordIds = new IconexMapper("Keywords", "KeywordIds");
            string fileName = filePath;// @"D:\Icons\iconex_v5\v_collection\_iconex_system\_scripts\icon_keyword_table2.js";
            string fileText = File.ReadAllText(fileName);
            int iconCount = 0;
            Console.WriteLine($"**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**");
            Console.WriteLine("********STARTED PROCESS TO REGISTER ICON NAMES AND KEYWORDS TO DB*******");
            Console.WriteLine($"**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**{prefix}**");


            var tmp = conx.GetIconCount();
            if (tmp.IsOK)
                iconCount = tmp.Result;

            JObject obj = JsonConvert.DeserializeObject<JObject>(fileText);

            if (obj != null)
            {
                foreach (JToken tok in obj["keywordsToIconNameIds"].Children())
                {
                    Mapping x = new Mapping();
                    x.Text = tok[0].ToString();
                    JArray refs = tok[1] as JArray;
                    foreach (JToken r in refs)
                        x.References.Add(int.Parse(r.ToString()) + iconCount);
                    KeyWordsToIconIds.Mappings.Add(x);
                }

                foreach (JToken tok in obj["iconNamesToKeywordIds"].Children())
                {
                    Mapping x = new Mapping();
                    x.Text = $"{prefix}_{tok[0].ToString()}";
                    JArray refs = tok[1] as JArray;
                    foreach (JToken r in refs)
                        x.References.Add(int.Parse(r.ToString()) + iconCount);
                    IconsToKeyWordIds.Mappings.Add(x);
                }

                foreach (JToken tok in obj["searchKeywordsToKeywordIds"].Children())
                {
                    Mapping x = new Mapping();
                    x.Text = tok[0].ToString();
                    JArray refs = tok[1] as JArray;
                    foreach (JToken r in refs)
                        x.References.Add(int.Parse(r.ToString()) + iconCount);
                    KeyWordsToKeywordIds.Mappings.Add(x);
                }

                Console.WriteLine($"Collection 1: {KeyWordsToIconIds.Mappings.Count}");
                Console.WriteLine($"Collection 2: {IconsToKeyWordIds.Mappings.Count}");
                Console.WriteLine($"Collection 3: {KeyWordsToKeywordIds.Mappings.Count}");

                //register keywords and their references
                int count = 0;
                foreach (Mapping m in KeyWordsToKeywordIds.Mappings)
                {
                    foreach (int r in m.References)
                    {
                        conx.Keywords_Insert(m.Text, r);
                    }
                    if (count % 100 == 0)
                        Console.WriteLine($"{prefix}: Registering Keyword to Db {count} of {KeyWordsToKeywordIds.Mappings.Count}");
                    count++;
                }
                Console.WriteLine($"{prefix}: Registered All Keywords to Db ({KeyWordsToKeywordIds.Mappings.Count})");

                Dictionary<string, List<int>> ItoK = new Dictionary<string, List<int>>();
                foreach (Mapping m in IconsToKeyWordIds.Mappings)
                    ItoK.Add(m.Text, m.References);

                //register icons
                int index = 0;
                foreach (var ItK in ItoK)
                {
                    conx.Icons_Insert(ItK.Key, index);
                    if (index % 100 == 0)
                        Console.WriteLine($"{prefix}: Registering Icon to Db {index} of {ItoK.Count}");
                    index++;
                }
                Console.WriteLine($"{prefix}: Registered All Icons to Db ({ItoK.Count})");


                Dictionary<string, List<int>> KtoI = new Dictionary<string, List<int>>();
                foreach (Mapping m in KeyWordsToIconIds.Mappings)
                    KtoI.Add(m.Text, m.References);

                int rel = 0;
                //register relationship between Icon and Keyword
                foreach (var ItK in ItoK)
                {
                    int IconId;
                    var res = conx.GetIconId(ItK.Key);
                    if (res.IsOK)
                    {
                        IconId = res.Result;
                        foreach (int keywordId in ItK.Value)
                        {
                            
                            conx.RegisterIconRelationShip(IconId, keywordId + tmp.Result);
                            if (rel % 100 == 0)
                                Console.WriteLine($"{prefix}: Registering Icon to Keyword relationship: {rel} of ?");
                            rel++;
                        }
                    }
                }
                Console.WriteLine($"{prefix}: Registered All Relationships to Db ({rel})");
            }

        }
    }
    public class IconexMapper
    {
        public string From { get; set; }
        public string To { get; set; }
        public List<Mapping> Mappings { get; set; }

        public IconexMapper()
        {
            Mappings = new List<Mapping>();
        }
        public IconexMapper(string from, string to)
        {
            this.From = from;
            this.To = to;
            Mappings = new List<Mapping>();
        }
    }
    public class Mapping
    {
        public string Text { get; set; }
        public List<int> References { get; set; }
        public Mapping()
        {
            this.Text = String.Empty;
            this.References = new List<int>();
        }
    }
}
