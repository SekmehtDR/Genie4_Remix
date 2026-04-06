using System;
using System.Collections;
using System.Drawing;
using System.Text.RegularExpressions;

namespace GenieClient.Genie
{
    public class Highlights : Collections.SortedList
    {
        private Regex m_oRegexString = null;
        private Regex m_oRegexStringCI = null;
        private Regex m_oRegexLine = null;
        private Regex m_oRegexLineCI = null;

        public Regex RegexString
        {
            get { return m_oRegexString; }
            set { m_oRegexString = value; }
        }

        public Regex RegexStringCI
        {
            get { return m_oRegexStringCI; }
            set { m_oRegexStringCI = value; }
        }

        public Regex RegexLine
        {
            get { return m_oRegexLine; }
            set { m_oRegexLine = value; }
        }

        public Regex RegexLineCI
        {
            get { return m_oRegexLineCI; }
            set { m_oRegexLineCI = value; }
        }

        // Finds a highlight by key using case-insensitive comparison, for use after a RegexStringCI/RegexLineCI match.
        public Highlight GetCaseInsensitive(string matchedValue)
        {
            foreach (string key in base.Keys)
            {
                if (string.Equals(key, matchedValue, StringComparison.OrdinalIgnoreCase))
                    return (Highlight)base[key];
            }
            return null;
        }

        public void ToggleClass(string ClassName, bool Value)
        {
            if (AcquireReaderLock())
            {
                var al = new ArrayList();
                try
                {
                    foreach (string s in base.Keys)
                        al.Add(s);
                }
                finally
                {
                    ReleaseReaderLock();
                    foreach (string s in al)
                    {
                        Highlight hl = (Highlight)base[s];
                        if ((hl.ClassName.ToLower() ?? "") == (ClassName.ToLower() ?? ""))
                        {
                            hl.IsActive = Value;
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Unable to aquire reader lock.");
            }
        }

        public class Highlight
        {
            public Color FgColor;
            public Color BgColor;
            public string ColorName = string.Empty;
            public bool HighlightWholeRow = false;
            public bool CaseSensitive = true;
            public string ClassName = string.Empty;
            public bool IsActive = true;
            public string SoundFile = string.Empty;

            public Highlight(Color oColor, string sColorName, Color oBgColor, bool bHighlightWholeRow, bool CaseSensitive = true, string SoundFile = "", string ClassName = "", bool IsActive = true)
            {
                FgColor = oColor;
                BgColor = oBgColor;
                HighlightWholeRow = bHighlightWholeRow;
                ColorName = sColorName;
                this.CaseSensitive = CaseSensitive;
                this.SoundFile = SoundFile;
                this.ClassName = ClassName;
                this.IsActive = IsActive;
            }
        }

        public bool Add(string sKey, bool bHighlightWholeRow, string sColorName, bool bCaseSensitive = true, string SoundFile = "", string ClassName = "", bool IsActive = true)
        {
            if (sKey.Length == 0)
            {
                return false;
            }
            else
            {
                Color oColor;
                Color oBgcolor;
                if (sColorName.Contains(",") == true && sColorName.EndsWith(",") == false)
                {
                    string sColor = sColorName.Substring(0, sColorName.IndexOf(",")).Trim();
                    string sBgColor = sColorName.Substring(sColorName.IndexOf(",") + 1).Trim();
                    oColor = ColorCode.StringToColor(sColor);
                    oBgcolor = ColorCode.StringToColor(sBgColor);
                }
                else
                {
                    oColor = ColorCode.StringToColor(sColorName);
                    oBgcolor = Color.Transparent;
                }

                if (base.ContainsKey(sKey) == true)
                {
                    base[sKey] = new Highlight(oColor, sColorName, oBgcolor, bHighlightWholeRow, bCaseSensitive, SoundFile, ClassName, IsActive);
                }
                else
                {
                    object argvalue = new Highlight(oColor, sColorName, oBgcolor, bHighlightWholeRow, bCaseSensitive, SoundFile, ClassName, IsActive);
                    Add(sKey, argvalue);
                }

                return true;
            }
        }

        public int Remove(string sKey)
        {
            if (base.ContainsKey(sKey) == true)
            {
                base.Remove(sKey);
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public void RebuildStringIndex()
        {
            if (AcquireReaderLock())
            {
                try
                {
                    var alCS = new ArrayList();
                    var alCI = new ArrayList();
                    foreach (string s in base.Keys)
                    {
                        Highlight h = (Highlight)base[s];
                        if (h.IsActive && !h.HighlightWholeRow)
                        {
                            if (h.CaseSensitive)
                                alCS.Add(s);
                            else
                                alCI.Add(s);
                        }
                    }

                    alCS.Sort();
                    var sbCS = new System.Text.StringBuilder();
                    foreach (string s in alCS)
                    {
                        if (sbCS.Length > 0) sbCS.Append('|');
                        sbCS.Append(Regex.Replace(s, @"([^A-Za-z0-9\s])", @"\$1"));
                    }
                    if (sbCS.Length > 0) { sbCS.Insert(0, '('); sbCS.Append(')'); }
                    RegexString = sbCS.Length > 0 ? new Regex(sbCS.ToString(), RegexOptions.Compiled) : null;

                    alCI.Sort();
                    var sbCI = new System.Text.StringBuilder();
                    foreach (string s in alCI)
                    {
                        if (sbCI.Length > 0) sbCI.Append('|');
                        sbCI.Append(Regex.Replace(s, @"([^A-Za-z0-9\s])", @"\$1"));
                    }
                    if (sbCI.Length > 0) { sbCI.Insert(0, '('); sbCI.Append(')'); }
                    RegexStringCI = sbCI.Length > 0 ? new Regex(sbCI.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase) : null;
                }
                finally
                {
                    ReleaseReaderLock();
                }
            }
            else
            {
                throw new Exception("Unable to aquire writer lock.");
            }
        }

        public void RebuildLineIndex()
        {
            if (AcquireReaderLock())
            {
                try
                {
                    var alCS = new ArrayList();
                    var alCI = new ArrayList();
                    foreach (string s in base.Keys)
                    {
                        Highlight h = (Highlight)base[s];
                        if (h.IsActive && h.HighlightWholeRow)
                        {
                            if (h.CaseSensitive)
                                alCS.Add(s);
                            else
                                alCI.Add(s);
                        }
                    }

                    alCS.Sort();
                    var sbCS = new System.Text.StringBuilder();
                    foreach (string s in alCS)
                    {
                        if (sbCS.Length > 0) sbCS.Append('|');
                        sbCS.Append(Regex.Replace(s, @"([^A-Za-z0-9\s])", @"\$1"));
                    }
                    if (sbCS.Length > 0) { sbCS.Insert(0, '('); sbCS.Append(')'); }
                    RegexLine = sbCS.Length > 0 ? new Regex(sbCS.ToString(), RegexOptions.Compiled) : null;

                    alCI.Sort();
                    var sbCI = new System.Text.StringBuilder();
                    foreach (string s in alCI)
                    {
                        if (sbCI.Length > 0) sbCI.Append('|');
                        sbCI.Append(Regex.Replace(s, @"([^A-Za-z0-9\s])", @"\$1"));
                    }
                    if (sbCI.Length > 0) { sbCI.Insert(0, '('); sbCI.Append(')'); }
                    RegexLineCI = sbCI.Length > 0 ? new Regex(sbCI.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase) : null;
                }
                finally
                {
                    ReleaseReaderLock();
                }
            }
            else
            {
                throw new Exception("Unable to aquire writer lock.");
            }
        }
    }
}