﻿using System;
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
        readonly string TargetM3UInfo = "#EXTM3U url-tvg=\"http://epg.it999.ru/edem.xml.gz\" deinterlace=1 cache=1000";

        private void BtnMerge_Click(object sender, EventArgs e)
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

            MergeInTarget();

            lblInfo.Text = "Finished!" + DateTime.Now;
        }

        private void ParseList(FileInfo list)
        {
            ParseList(list.FullName);
        }

        readonly Dictionary<string, List<Record>> TargetListContent = new Dictionary<string, List<Record>>();
        readonly HashSet<string> TargetListContentAdded = new HashSet<string>();

        private void MergeInTarget()
        {
            // skip list to skip grops
            // check if exists file skipg.txt and add list of groups from there
            HashSet<string> skipgroupslist = SetList("skipg.txt", true);

            // rename groups list.
            Dictionary<string, string> rengroupslist = SetDict("reng.txt");

            foreach (var listContent in listsContents)
            {
                string Group = "";
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
                        tags = SetGroupTitle(tags, Group, out string groupName);
                        Group = groupName;
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
                        Group = groupName.ToString().Trim();

                        if (!grouptitle)
                        {
                            tags = SetGroupTitle(tags, Group, out string gn, EXTGRP);
                        }
                    }

                    // skip group
                    if (skipgroupslist.Contains(Group))
                    {
                        continue; // In this case same adress will not be ignored in other group
                    }

                    // rename group name
                    if (rengroupslist.ContainsKey(Group.ToUpperInvariant()))
                    {
                        string GROUP = Group.ToUpperInvariant();
                        // in tags
                        foreach (var r in new[] { @"group-title\=\""" + Group.Replace("+", @"\+") + @"\""", "#EXTGRP:[ ]*" + Group.Replace("+", @"\+") })
                        {
                            Match m = Regex.Match(tags, r);
                            if (m.Success)
                            {
                                tags = tags.Replace(m.Value, m.Value.Replace(Group, rengroupslist[GROUP]));
                            }
                        }

                        // group name itself
                        Group = rengroupslist[GROUP];
                    }

                    // add new record
                    if (!TargetListContent.ContainsKey(Group))
                    {
                        TargetListContent.Add(Group, new List<Record>()); // add group if missing
                    }
                    string title = Regex.Match(tags, @"#EXTINF[^,]+,([^\r\n]+).*").Result("$1");
                    TargetListContent[Group].Add(new Record(source, tags, title)); // add record to group
                    TargetListContentAdded.Add(source); // add source stream link to control duplicates

                    Group = "";
                }
            }

            // read list of skip words
            HashSet<string> dontskipwords = SetList("!skipw.txt", true);

            // read list of skip words
            HashSet<string> skipwords = SetList("skipw.txt", true);

            //create target playlist content and add header m3u info
            StringBuilder targetm3uContent = new StringBuilder();
            targetm3uContent.AppendLine(TargetM3UInfo);

            // check if exists file add1.txt and add lines from there right adter m3u info
            foreach (var line in SetList("add1.txt"))
            {
                targetm3uContent.AppendLine(line);
            }

            // first add groups by selected list
            // check if exists file firstg.txt and add list of groups from there
            HashSet<string> firstgroups = SetList("firstg.txt");
            foreach (var group in firstgroups)
            {
                if (TargetListContent.ContainsKey(group))
                {
                    foreach (var record in TargetListContent[group].OrderBy(r => r.title))
                    {
                        if (record.value.ToUpperInvariant().HasSkipwordFrom(skipwords, dontskipwords))
                        {
                            continue;
                        }

                        targetm3uContent.AppendLine(record.value + "\r\n" + record.address);
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

            // last add groups by selected list
            HashSet<string> lastgroups = SetList("lastg.txt");

            //add groups and records by sorted list
            foreach (var group in groupslist)
            {
                if (firstgroups.Contains(group)/*first groups was already added*/ || lastgroups.Contains(group)/*last groups will be added in the end*/ || skipgroupslist.Contains(group.ToUpperInvariant())/*skip excluded groups*/)
                {
                    continue;
                }

                foreach (var record in TargetListContent[group].OrderBy(r => r.title))
                {
                    if (record.value.ToUpperInvariant().HasSkipwordFrom(skipwords, dontskipwords))
                    {
                        continue;
                    }

                    targetm3uContent.AppendLine(record.value + "\r\n" + record.address);
                }
            }

            // check if exists file lastg.txt and add list of groups from there
            foreach (var group in lastgroups)
            {
                if (TargetListContent.ContainsKey(group))
                {
                    foreach (var record in TargetListContent[group].OrderBy(r => r.title))
                    {
                        if (record.value.ToUpperInvariant().HasSkipwordFrom(skipwords, dontskipwords))
                        {
                            continue;
                        }

                        targetm3uContent.AppendLine(record.value + "\r\n" + record.address);
                    }
                }
            }

            // write target playlist
            File.WriteAllText(Target, targetm3uContent.ToString());
        }

        private HashSet<string> SetList(string filepath, bool toupper = false)
        {
            var list = new HashSet<string>();
            if (File.Exists(filepath))
            {
                foreach (var line in File.ReadAllLines(filepath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    {
                        continue; // skip empty and commented lines
                    }

                    list.Add(toupper ? line.ToUpperInvariant() : line);
                }
            }

            return list;
        }

        private Dictionary<string, string> SetDict(string filepath, string splitter = "=", bool caseinsensitive = true)
        {
            var list = new Dictionary<string, string>();
            if (File.Exists(filepath))
            {
                foreach (var line in File.ReadAllLines(filepath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    {
                        continue; // skip empty and commented lines
                    }

                    string[] splitted = line.Split(new[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitted.Length != 2)
                    {
                        continue;
                    }

                    string key = caseinsensitive ? splitted[0].ToUpperInvariant() : splitted[0];
                    if (!list.ContainsKey(key))
                        list.Add(key, splitted[1]);
                }
            }

            return list;
        }

        private string SetGroupTitle(string value, string lastGroup, out string groupName, bool EXTGRP = false)
        {
            var parts = value.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var reg = Regex.Match(parts[0], @"#EXTINF(:-?[0-9]{0,12})");
            Match groupm = null;
            if (!EXTGRP)
            {
                groupm = Regex.Match(parts[0], @"group\=\""([^\""]+)\""");
            }
            groupName = (EXTGRP ? lastGroup : groupm != null && groupm.Success ? groupm.Result("$1") : lastGroup.Length > 0 ? lastGroup : "Разное");
            parts[0] = parts[0].Insert(parts[0].IndexOf(reg.Result("$1")) + reg.Result("$1").Length, " group-title=\"" + groupName + "\"");

            return string.Join("\r\n", parts);
        }

        readonly List<Playlist> listsContents = new List<Playlist>();
        private void ParseList(string list)
        {
            if (!Path.GetExtension(list).ToUpperInvariant().StartsWith(".M3U"))
            {
                return;
            }

            var pl = new Playlist();
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
        public static bool HasSkipwordFrom(this string record, HashSet<string> skipwords, HashSet<string> dontskipwords = null)
        {
            foreach (var word in skipwords)
            {
                if (record.Contains(word) && (dontskipwords == null || (dontskipwords != null && !record.HasSkipwordFrom(dontskipwords))))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class Playlist
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

        public Playlist()
        {
            Extm3uInfo = new List<string>();
            items = new Dictionary<string, string>();
        }
    }
    public class Record
    {
        public string address;
        public string value;
        public string title;

        public Record(string address, string value, string title)
        {
            this.address = address;
            this.value = value;
            this.title = title;
        }
    }
}
