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

            if (!string.IsNullOrWhiteSpace(tbSource.Text) && tbSource.Text != Source) Source = tbSource.Text;
            if (!string.IsNullOrWhiteSpace(tbTarget.Text) && tbTarget.Text != Target) Target = tbTarget.Text;

            var rawTargetPath = Target;

            Source = Path.GetFullPath(Source);
            Target = Path.GetFullPath(Target);

            bool SourceIsDir = false;
            if (!(SourceIsDir = Directory.Exists(Source)) && !File.Exists(Source))
            {
                lblInfo.Text = "Invalid source path";
                return;
            }

            Properties.Settings.Default.LastTarget = rawTargetPath;
            cbxTargets.UpdateTargets(rawTargetPath);
            Properties.Settings.Default.Save();

            if (SourceIsDir)
            {
                var lists = new DirectoryInfo(Source).EnumerateFiles("*.m3u?", SearchOption.AllDirectories).OrderByDescending(f => f.LastWriteTime.Year <= 1601 ? f.CreationTime : f.LastWriteTime)/*https://stackoverflow.com/a/23839158*/;
                foreach (var list in lists) ParseList(list);
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

        readonly Dictionary<string, List<Record>> TargetListContent = new();
        readonly HashSet<string> TargetListContentAdded = new();

        private void MergeInTarget()
        {
            // skip list to skip grops
            // check if exists file skipg.txt and add list of groups from there
            var skipgroupslist = SetList("skipg.txt", true);

            // blacklist to skip urls
            var skipurl = SetList("skipu.txt", true, cutUrlOptions: true);

            // rename groups list.
            var rengroupslist = SetDict("reng.txt", true);
            var movebywordlist = SetDict("rengbyw.txt", true);

            foreach (var listContent in listsContents)
            {
                string LastGroupTitle = "";
                foreach (var item in listContent.items)
                {
                    string GroupTitle = "";

                    // skip url from skipu and skip already added
                    if (TargetListContentAdded.Contains(item.Key)) continue;
                    if (skipurl.Contains(item.Key.Split('?')[0].ToUpperInvariant())) continue;

                    var tags = item.Value;
                    var source = item.Key;
                    var EXTGRP = false;

                    var isgrouptitle = false;
                    if ((!(isgrouptitle = tags.Contains("group-title")) && !(EXTGRP = tags.Contains("#EXTGRP"))))
                    {
                        tags = SetGroupTitle(tags, LastGroupTitle, out string groupName);
                        GroupTitle = groupName;
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

                        StringBuilder groupName = new();
                        foreach (var c in tags[(tags.IndexOf(tag) + tag.Length)..])
                        {
                            if (c == c1 || c == c2) break;

                            groupName.Append(c);
                        }
                        GroupTitle = groupName.ToString().Trim();

                        if (!isgrouptitle)
                        {
                            tags = SetGroupTitle(tags, GroupTitle, out string gn, EXTGRP);
                            GroupTitle = gn;
                        }
                    }

                    LastGroupTitle = GroupTitle; // set last group title for nex records if they are ith no title

                    // skip group
                    if (skipgroupslist.Contains(GroupTitle)) continue; // In this case same adress will not be ignored in other group

                    // record's title
                    string title = Regex.Match(tags, @"#EXTINF[^,]+,([^\r\n]+).*").Result("$1");

                    string WORD = null;
                    string GROUP = GroupTitle.ToUpperInvariant();
                    string TITLE = title.ToUpperInvariant();
                    bool G = false;
                    // rename group name
                    if (((WORD = TITLE.HasSkipwordFromDict(movebywordlist)) != null && movebywordlist[WORD] != GroupTitle)
                        || ((G = rengroupslist.ContainsKey(GROUP)) && rengroupslist[GROUP] != GroupTitle))
                    {
                        // in tags
                        foreach (var r in new[] { @"group-title\=\""" + GroupTitle.Replace("+", @"\+") + @"\""", "#EXTGRP:[ ]*" + GroupTitle.Replace("+", @"\+") })
                        {
                            Match m = Regex.Match(tags, r, RegexOptions.IgnoreCase);
                            if (!m.Success) continue;

                            tags = G
                                ? tags.Replace(m.Value, m.Value.Replace(GroupTitle, rengroupslist[GROUP]))
                                : tags.Replace(m.Value, m.Value.Replace(GroupTitle, movebywordlist[WORD]));
                        }

                        // group name itself
                        GroupTitle = G ? rengroupslist[GROUP] : movebywordlist[WORD];
                    }

                    // add new record
                    if (!TargetListContent.ContainsKey(GroupTitle))
                    {
                        TargetListContent.Add(GroupTitle, new List<Record>()); // add group if missing
                    }
                    TargetListContent[GroupTitle].Add(new Record(source, tags, title)); // add record to group
                    TargetListContentAdded.Add(source); // add source stream link to control duplicates

                    LastGroupTitle = "";// disabled last group
                }

                //LastGroupTitle = ""; // reset last group title after end of a playlist
            }

            // read list of skip words
            var dontskipwords = SetList("!skipw.txt", true);

            // read list of skip words
            var skipwords = SetList("skipw.txt", true);

            //create target playlist content and add header m3u info
            StringBuilder targetm3uContent = new();
            targetm3uContent.AppendLine(TargetM3UInfo);

            // check if exists file add1.txt and add lines from there right adter m3u info
            foreach (var line in SetList("add1.txt")) targetm3uContent.AppendLine(line);

            // first add groups by selected list
            // check if exists file firstg.txt and add list of groups from there
            var firstgroups = SetList("firstg.txt");
            foreach (var group in firstgroups)
            {
                if (!TargetListContent.ContainsKey(group)) continue;

                foreach (var record in TargetListContent[group].OrderBy(r => r.title))
                {
                    if (record.value.ToUpperInvariant().HasSkipwordFrom(skipwords, dontskipwords)) continue;

                    targetm3uContent.AppendLine(record.value + "\r\n" + record.address);
                }
            }

            //create sorted groups list
            string[] groupslist = new string[TargetListContent.Count];
            int i = 0;
            foreach (var group in TargetListContent.Keys) groupslist[i++] = group;

            Array.Sort(groupslist);

            // last add groups by selected list
            var lastgroups = SetList("lastg.txt");

            //add groups and records by sorted list
            foreach (var group in groupslist)
            {
                if (firstgroups.Contains(group)) continue; // first groups was already added
                if (lastgroups.Contains(group)) continue; // last groups will be added in the end
                if (skipgroupslist.Contains(group.ToUpperInvariant())) continue; // skip excluded groups

                foreach (var record in TargetListContent[group].OrderBy(r => r.title))
                {
                    if (record.value.ToUpperInvariant().HasSkipwordFrom(skipwords, dontskipwords)) continue;

                    targetm3uContent.AppendLine(record.value + "\r\n" + record.address);
                }
            }

            // check if exists file lastg.txt and add list of groups from there
            foreach (var group in lastgroups)
            {
                if (!TargetListContent.ContainsKey(group)) continue;

                foreach (var record in TargetListContent[group].OrderBy(r => r.title))
                {
                    if (record.value.ToUpperInvariant().HasSkipwordFrom(skipwords, dontskipwords)) continue;

                    targetm3uContent.AppendLine(record.value + "\r\n" + record.address);
                }
            }

            // write target playlist
            File.WriteAllText(Target, targetm3uContent.ToString());
        }

        private static HashSet<string> SetList(string filepath, bool toupper = false, bool cutUrlOptions = false)
        {
            var list = new HashSet<string>();
            if (!File.Exists(filepath)) return list;

            foreach (var line in File.ReadAllLines(filepath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue; // skip empty and commented lines

                string lineToAdd = line;
                if (cutUrlOptions && line.Length > 2 && line.Contains("?") && (line.ToUpperInvariant().StartsWith("HTTP") || line.ToUpperInvariant().StartsWith("UDP")))
                {
                    lineToAdd = line.Split('?')[0];
                }
                lineToAdd = (toupper ? lineToAdd.ToUpperInvariant() : lineToAdd);

                if (!list.Contains(lineToAdd)) list.Add(lineToAdd);
            }

            return list;
        }

        private static Dictionary<string, string> SetDict(string filepath, bool caseinsensitive = true, string splitter = "=")
        {
            var list = new Dictionary<string, string>();
            if (!File.Exists(filepath)) return list;

            foreach (var line in File.ReadAllLines(filepath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue; // skip empty and commented lines

                string[] splitted = line.Split(new[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
                if (splitted.Length != 2) continue;

                string key = caseinsensitive ? splitted[0].ToUpperInvariant() : splitted[0];
                if (!list.ContainsKey(key)) list.Add(key, splitted[1]);

            }

            return list;
        }

        private static string SetGroupTitle(string value, string lastGroup, out string groupName, bool EXTGRP = false)
        {
            var parts = value.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var reg = Regex.Match(parts[0], @"#EXTINF(:-?[0-9]{0,12})");
            Match groupm = null;
            if (!EXTGRP) groupm = Regex.Match(parts[0], @"group\=\""([^\""]+)\""");

            groupName = (EXTGRP ? lastGroup : groupm != null && groupm.Success ? groupm.Result("$1") : lastGroup.Length > 0 ? lastGroup : "Разное");
            parts[0] = parts[0].Insert(parts[0].IndexOf(reg.Result("$1")) + reg.Result("$1").Length, " group-title=\"" + groupName + "\"");

            return string.Join("\r\n", parts);
        }

        readonly List<Playlist> listsContents = new();
        private void ParseList(string list)
        {
            if (!Path.GetExtension(list).ToUpperInvariant().StartsWith(".M3U")) return;

            var pl = new Playlist();
            pl.Read(list);

            if (pl?.items.Count > 0) listsContents.Add(pl);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.TargetsList == null) Properties.Settings.Default.TargetsList = new System.Collections.Specialized.StringCollection();

            Target = Properties.Settings.Default.LastTarget;
            tbSource.Text = Source;
            tbTarget.Text = Target;
            cbxTargets.Items.AddRange(Properties.Settings.Default.TargetsList.ToArray());
        }

        private void CBXTargets_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbTarget.Text = cbxTargets.Text;
        }
    }

    public class Playlist
    {
        public List<string> Extm3uInfo = new List<string>();
        public Dictionary<string, string> items = new Dictionary<string, string>();

        public void Read(string list)
        {
            using StreamReader sr = new(list);
            string line;
            var GotExm3u = false;
            bool IstrackInfo = false;
            StringBuilder rlines = new();
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

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
                        if (endname == -1) continue; // skip when no m3u info

                        var name = line[1..endname];
                        var value = line[(endname + 1)..].Split(',');
                        var title = value.Length == 2 ? value[2] : "";
                        var attributes = value[0];

                        Extm3uInfo = new List<string>();

                        foreach (var attrib in attributes.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (string.IsNullOrWhiteSpace(attrib)) continue;

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
