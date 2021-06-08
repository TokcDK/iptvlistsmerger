using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            lblInfo.Text = "";

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
                foreach (var list in new DirectoryInfo(Source).GetFiles("*.m3u?", SearchOption.AllDirectories).OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime)/*https://stackoverflow.com/a/23839158*/)
                {
                    ParseList(list);
                }
            }
            else
            {
                ParseList(Source);
            }

            MargeInTarget();

            lblInfo.Text = "Finished!" + DateTime.Now;
        }

        private void ParseList(FileInfo list)
        {
            ParseList(list.FullName);
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

                    var tags = item.Value;
                    var source = item.Key;
                    var EXTGRP = false;
                    var grouptitle = false;
                    if ((!(grouptitle = tags.Contains("group-title")) && !(EXTGRP = tags.Contains("#EXTGRP"))))
                    {
                        tags = SetGroupTitle(tags, lastGroup, out string groupName);
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
                        foreach (var c in tags.Substring(tags.IndexOf(tag) + tag.Length))
                        {
                            if (c == c1 || c == c2)
                            {
                                break;
                            }

                            groupName.Append(c);
                        }
                        lastGroup = groupName.ToString().Trim();

                        if (!grouptitle)
                        {
                            tags = SetGroupTitle(tags, lastGroup, out string gn, EXTGRP);
                        }
                    }


                    if (!TargetListContent.ContainsKey(lastGroup))
                    {
                        TargetListContent.Add(lastGroup, new List<string>()); // add group if missing
                    }
                    TargetListContent[lastGroup].Add(tags + "\r\n" + source); // add record to group
                    TargetListContentAdded.Add(source); // add source stream link to control duplicates
                }
            }

            // read list of skip words
            HashSet<string> skipwords = new HashSet<string>();
            if (File.Exists("skipw.txt"))
            {
                foreach (var line in File.ReadAllLines("skipw.txt"))
                {
                    skipwords.Add(line.ToUpperInvariant());
                }
            }

            //create target playlist content and add header m3u info
            StringBuilder targetm3uContent = new StringBuilder();
            targetm3uContent.AppendLine(TargetM3UInfo);

            // check if exists file add1.txt and add lines from there right adter m3u info
            if (File.Exists("add1.txt"))
            {
                foreach (var line in File.ReadAllLines("add1.txt"))
                {
                    targetm3uContent.AppendLine(line);
                }
            }

            // first add groups by selected list
            // check if exists file sortg.txt and add list of groups from there
            HashSet<string> groups = new HashSet<string>();
            if (File.Exists("sortg.txt"))
            {
                foreach (var line in File.ReadAllLines("sortg.txt"))
                {
                    groups.Add(line);
                }
            }
            foreach (var group in groups)
            {
                if (TargetListContent.ContainsKey(group))
                {
                    foreach (var record in TargetListContent[group])
                    {
                        if (record.HasSkipwordFrom(skipwords))
                        {
                            continue;
                        }

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

            // skip list to skip grops
            // check if exists file skipg.txt and add list of groups from there
            HashSet<string> skipgroupslist = new HashSet<string>();
            if (File.Exists("skipg.txt"))
            {
                foreach (var line in File.ReadAllLines("skipg.txt"))
                {
                    skipgroupslist.Add(line.ToUpperInvariant());
                }
            }

            //add rest of groups records by sorted list
            foreach (var group in groupslist)
            {
                if (groups.Contains(group) || skipgroupslist.Contains(group.ToUpperInvariant())) // skip already added groups and excluded groups
                {
                    continue;
                }

                foreach (var record in TargetListContent[group])
                {
                    if (record.ToUpperInvariant().HasSkipwordFrom(skipwords))
                    {
                        continue;
                    }

                    targetm3uContent.AppendLine(record);
                }
            }

            // write target playlist
            File.WriteAllText(Target, targetm3uContent.ToString());
        }

        private string SetGroupTitle(string value, string lastGroup, out string groupName, bool EXTGRP = false)
        {
            var parts = value.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var reg = Regex.Match(parts[0], @"#EXTINF:(-?[0-9]{1,12})");
            groupName = (EXTGRP || lastGroup.Length > 0 ? lastGroup : "Разное");
            parts[0] = parts[0].Insert(parts[0].IndexOf(reg.Result("$1")) + reg.Result("$1").Length, " group-title=\"" + groupName + "\"");

            return string.Join("\r\n", parts);
        }

        List<playlist> listsContents = new List<playlist>();
        private void ParseList(string list)
        {
            if (!Path.GetExtension(list).ToUpperInvariant().StartsWith(".M3U"))
            {
                return;
            }

            var pl = new playlist();
            pl.Read(list);

            if (pl?.items.Count > 0)
            {
                listsContents.Add(pl);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tbSource.Text = Source;
            tbTarget.Text = Target;
        }
    }

    public static class Extensions
    {
        public static bool HasSkipwordFrom(this string record, HashSet<string> skipwords)
        {
            foreach (var word in skipwords)
            {
                if (record.Contains(word))
                {
                    return true;
                }
            }

            return false;
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

                            var endname = line.TrimEnd().IndexOf(' ');
                            if (endname == -1) // skip when no m3u info
                            {
                                continue;
                            }
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
