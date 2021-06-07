﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace iptvlistsmerger
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string Source = @".\Sources\";
        string Target = @".\Target.m3u";
        string TargetM3UInfo = "#EXTM3U url-tvg=\"http://epg.it999.ru/edem.xml.gz\" deinterlace=1 cache=1000";

        private void btnMerge_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbSource.Text) && tbSource.Text != Source)
            {
                Source = tbSource.Text;
            }
            if (!string.IsNullOrWhiteSpace(tbTarget.Text) && tbTarget.Text != Target)
            {
                Target = tbTarget.Text;
            }

            Source = Path.GetFullPath(Source);
            Target = Path.GetFullPath(Target);

            bool SourceIsDir = false;
            if (!(SourceIsDir = Directory.Exists(Source)) && !File.Exists(Source))
            {
                return;
            }

            if (SourceIsDir)
            {
                foreach (var list in Directory.GetFiles(Source, "*.m3u", SearchOption.AllDirectories))
                {
                    ParseList(list);
                }
            }
            else
            {
                ParseList(Source);
            }

            MargeInTarget();
        }

        Dictionary<string, List<string>> TargetListContent = new Dictionary<string, List<string>>();
        HashSet<string> TargetListContentAdded = new HashSet<string>();

        private void MargeInTarget()
        {
            foreach (var listContent in listsContents)
            {
                string lastGroup = "";
                foreach (var item in listContent.items)
                {
                    if (TargetListContentAdded.Contains(item.Key))
                    {
                        continue;
                    }

                    var value = item.Value;
                    var EXTGRP = false;
                    var grouptitle = false;
                    if (!(grouptitle = value.Contains("group-title")) && !(EXTGRP = value.Contains("#EXTGRP")))
                    {
                        var parts = value.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        var reg = Regex.Match(parts[0], @"#EXTINF:(-?[0-9]{1,12})");
                        var groupName = (lastGroup.Length == 0 ? "group-title=\"Разное\"" : lastGroup);
                        parts[0] = parts[0].Insert(parts[0].IndexOf(reg.Result("$1")) + reg.Result("$1").Length, " " + groupName);
                        value = string.Join("\r\n", parts);
                        lastGroup = groupName;
                    }
                    else
                    {
                        string tag = "group-title=\"";
                        char c1 = '"';
                        char c2 = ',';
                        if (EXTGRP)
                        {
                            tag = "#EXTGRP:";
                            c1 = '\r';
                            c2 = '\n';
                        }

                        StringBuilder groupName = new StringBuilder();
                        foreach (var c in value.Substring(value.IndexOf(tag) + tag.Length))
                        {
                            if (c == c1 || c == c2)
                            {
                                break;
                            }

                            groupName.Append(c);
                        }
                        lastGroup = groupName.ToString().Trim();
                    }


                    if (!TargetListContent.ContainsKey(lastGroup))
                    {
                        TargetListContent.Add(lastGroup, new List<string>()); // add group if missing
                    }
                    TargetListContent[lastGroup].Add(item.Value + "\r\n" + item.Key); // add record to group
                    TargetListContentAdded.Add(item.Key); // add source stream link to control duplicates
                }
            }

            //create target playlist content and add header m3u info
            StringBuilder targetm3uContent = new StringBuilder();
            targetm3uContent.AppendLine(TargetM3UInfo);

            //first add groups by selected list
            HashSet<string> groups = new HashSet<string>
            {
                "Российские",
                "Федеральные",
                "Россия",
                "Кино",
                "Спорт",
                "Познавательные"
            };
            foreach (var group in groups)
            {
                if (TargetListContent.ContainsKey(group))
                {
                    foreach (var record in TargetListContent[group])
                    {
                        targetm3uContent.AppendLine(record);
                    }
                }
            }

            //create sorted groups list
            string[] groupslist = new string[TargetListContent.Count];
            int i = 0;
            foreach (var group in TargetListContent.Keys)
            {
                groupslist[i] = group;
                i++;
            }
            Array.Sort(groupslist);

            //skip list to skip grops
            HashSet<string> skipgroupslist = new HashSet<string>
            {
                "Голландия",
                "Израиль",
                "Казахстан",
                "Армения",
                "Азербайджан",
                "Молдова",
                "Киргизия",
                "Украина",
                "Белорусcия",
                "Беларусь"
            };

            //add rest of groups records by sorted list
            foreach (var group in groupslist)
            {
                if (groups.Contains(group)) // skip already added groups
                {
                    continue;
                }

                foreach (var record in TargetListContent[group])
                {
                    targetm3uContent.AppendLine(record);
                }
            }

            // write target playlist
            File.WriteAllText(Target, targetm3uContent.ToString());
        }

        List<playlist> listsContents = new List<playlist>();
        private void ParseList(string list)
        {
            if (Path.GetExtension(list) != ".m3u")
            {
                return;
            }

            var pl = new playlist();
            pl.Read(list);

            listsContents.Add(pl);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tbSource.Text = Source;
            tbTarget.Text = Target;
        }
    }

    public class playlist
    {
        public List<string> Extm3uInfo;
        public Dictionary<string, string> items;

        public void Read(string list)
        {
            using (StreamReader sr = new StreamReader(list))
            {
                string line;
                var GotExm3u = false;
                bool IstrackInfo = false;
                StringBuilder rlines = new StringBuilder();
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (IstrackInfo && (line.StartsWith("udp") || line.StartsWith("http")))
                    {
                        if (!items.ContainsKey(line))
                            items.Add(line, rlines.ToString());

                        IstrackInfo = false;
                    }
                    else if (line.StartsWith("#EXT"))
                    {
                        if (!GotExm3u && line.StartsWith("#EXTM3U"))
                        {
                            GotExm3u = true;

                            var endname = line.IndexOf(' ');
                            var name = line.Substring(1, endname - 1);
                            var value = line.Substring(endname + 1).Split(',');
                            var title = value.Length == 2 ? value[2] : "";
                            var attributes = value[0];

                            Extm3uInfo = new List<string>();

                            foreach (var attrib in attributes.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (string.IsNullOrWhiteSpace(attrib))
                                {
                                    continue;
                                }
                                Extm3uInfo.Add(attrib);
                            }
                        }
                        else if (line.StartsWith("#EXTINF"))
                        {
                            IstrackInfo = true;
                            rlines.Clear();
                            rlines.Append(line);
                            //var data = line.Split(':');
                            //var name = data[0];
                            //var value = data[1].Split(',');
                            //var title = value.Length == 2 ? value[2] : "";
                            //var attributes = value[0];

                            //foreach (var att in attributes.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries))
                            //{
                            //    r.line = line;
                            //}

                        }
                        else if (IstrackInfo)
                        {
                            rlines.Append("\r\n" + line);
                        }
                    }

                }
            }
        }

        public playlist()
        {
            Extm3uInfo = new List<string>();
            items = new Dictionary<string, string>();
        }
    }
    public class record
    {
        Dictionary<string, string> Attributes;
        Dictionary<string, string> Extras;
        public string line;

        public record()
        {
        }

        void parseline(string line)
        {
        }
    }
}