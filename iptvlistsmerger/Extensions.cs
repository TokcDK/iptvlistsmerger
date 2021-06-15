using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iptvlistsmerger
{
    public static class Extensions
    {
        /// <summary>
        /// check if string contains any substring from skipwords.
        /// when dontskipwords is true, ignore skipword if it is in dontskipwors list.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="skipwords"></param>
        /// <param name="dontskipwords"></param>
        /// <returns></returns>
        public static bool HasSkipwordFrom(this string record, HashSet<string> skipwords, HashSet<string> dontskipwords = null)
        {
            foreach (var word in skipwords)
            {
                if (record.Contains(word) && (dontskipwords == null || (dontskipwords != null && !word.HasSkipwordFrom(dontskipwords))))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// check if string contains any substring from skipwords.
        /// when dontskipwords is true, ignore skipword if it is in dontskipwors list.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="skipwords"></param>
        /// <param name="dontskipwords"></param>
        /// <returns>skipword if found else null</returns>
        public static string HasSkipwordFromDict(this string record, Dictionary<string, string> skipwords)
        {
            foreach (var word in skipwords.Keys)
            {
                if (record.Contains(word))
                {
                    return word;
                }
            }

            return null;
        }

        /// <summary>
        /// get all elements to string array.
        /// </summary>
        /// <param name="stringcollection"></param>
        /// <returns>string array of strings</returns>
        public static string[] ToArray(this System.Collections.Specialized.StringCollection stringcollection)
        {
            if (stringcollection == null)
            {
                stringcollection = new System.Collections.Specialized.StringCollection();
            }

            var list = new string[stringcollection.Count];
            stringcollection.CopyTo(list, 0);

            return list;
        }

        /// <summary>
        /// update combobox items
        /// </summary>
        /// <param name="combobox"></param>
        /// <param name="record"></param>
        public static void UpdateTargets(this ComboBox combobox, string record)
        {
            if (!Properties.Settings.Default.TargetsList.Contains(record))
            {
                Properties.Settings.Default.TargetsList.Add(record);
                combobox.Items.Clear();
                combobox.Items.AddRange(Properties.Settings.Default.TargetsList.ToArray());
                Properties.Settings.Default.Save();
            }
        }
    }
}

