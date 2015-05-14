#region Copyright
/*
Copyright 2014 Cluster Reply s.r.l.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Reply.Cluster.Mercury.Adapters.Helpers
{
    internal enum Macros
    {
        SOURCEFILENAME,
        DATETIME,
        DATE,
        DATETIME_TZ,
        MESSAGEID,
        MESSAGEID_NOBRA,
        MESSAGEID_NOSYM,
        TIME,
        TIME_TZ,
        LOCALTIME,
        LOCALDATE,
        LOCALDATETIME,
        LOCALDATETIME_COMPACT,
        LOCALDATE_SHORT,
        MACHINENAME,
        YEAR,
        MONTH,
        DAY,
        HOUR,
        MINUTE,
        PROP
    };

    public class NameGenerator
    {
        Regex rx = new Regex("%(?<parameter>.*?)(?<value>:.*?)*%");
            
        protected static string GetTZD(DateTime time)
        {
            TimeSpan ts = TimeZone.CurrentTimeZone.GetUtcOffset(time);
            StringBuilder tzd = new StringBuilder(4);

            if (ts.Ticks > 0)
            {
                tzd.Append("+");
            }
            else tzd.Append("-");

            tzd.Append(ts.Hours);

            if (ts.Minutes < 10)
            {
                tzd.Append("0");
            }

            tzd.Append(ts.Minutes);

            return tzd.ToString();
        }

        private string EvaluateMacro(Macros macro, string value)
        {
            switch (macro)
            {
                case Macros.MACHINENAME:
                    {
                        return Environment.MachineName.ToString();
                    }
                case Macros.YEAR:
                    {
                        return DateTime.Today.Year.ToString();
                    }
                case Macros.MONTH:
                    {
                        return DateTime.Today.Month.ToString().PadLeft(2, '0');
                    }
                case Macros.DAY:
                    {
                        return DateTime.Today.Day.ToString().PadLeft(2, '0');
                    }
                case Macros.HOUR:
                    {
                        return DateTime.Now.Hour.ToString().PadLeft(2, '0');
                    }
                case Macros.MINUTE:
                    {
                        return DateTime.Now.Minute.ToString().PadLeft(2, '0');
                    }
                case Macros.SOURCEFILENAME:
                    {
                        return SourceFileName;
                    }
                case Macros.DATE:
                    {
                        return DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddT000000");
                    }
                case Macros.DATETIME:
                    {
                        return DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHHmmss");
                    }
                case Macros.DATETIME_TZ:
                    {
                        DateTime now = DateTime.Now;
                        return string.Concat(now.ToString("yyyyMMddHHmmsss"), GetTZD(now));
                    }
                case Macros.MESSAGEID:
                    {
                        return MessageID;
                    }
                case Macros.MESSAGEID_NOBRA:
                    {
                        return MessageID.Replace("{", "").Replace("}", "");
                    }
                case Macros.MESSAGEID_NOSYM:
                    {
                        return MessageID.Replace("{", "").Replace("}", "").Replace("-", "");
                    }
                case Macros.TIME:
                    {
                        return DateTime.Now.ToUniversalTime().ToString("HHmmss");
                    }
                case Macros.TIME_TZ:
                    {
                        DateTime now = DateTime.Now;
                        return string.Concat(now.ToUniversalTime().ToString("HHmmss"), GetTZD(now));
                    }
                case Macros.LOCALDATETIME:
                    {
                        return DateTime.Now.ToString("yyyy-MM-ddTHHmmss");
                    }
                case Macros.LOCALDATE:
                    {
                        return DateTime.Now.ToString("yyyyMMdd");
                    }
                case Macros.LOCALDATE_SHORT:
                    {
                        return DateTime.Now.ToString("yyMMdd");
                    }
                case Macros.LOCALDATETIME_COMPACT:
                    {
                        return DateTime.Now.ToString("yyyyMMddHHmmsss");
                    }
                case Macros.LOCALTIME:
                    {
                        return DateTime.Now.ToString("HHmmss");
                    }
                case Macros.PROP:
                    {
                        object propValue;
                        if (innerMsg.Properties.TryGetValue(value, out propValue))
                        {
                            return propValue.ToString();
                        }
                        return string.Empty;
                    }
                default:
                    {
                        throw new ArgumentException(string.Format("Macro '{0}' not available", macro), "macro");
                    }
            }

        }

        private Message innerMsg;
        private string suggestedName;
        private string suggestedFolder;

        #region MessageContextProperties

        protected string MessageID
        {
            get
            {
                if (innerMsg == null)
                    throw new ArgumentNullException("innerMsg");
                return innerMsg.Headers.MessageId.ToString();
            }
        }

        protected string SourceFileName
        {
            get
            {
                if (innerMsg == null)
                    throw new ArgumentNullException("innerMsg");
                return System.IO.Path.GetFileName(new UriBuilder(innerMsg.Headers.Action).Uri.ToString());
            }
        }

        #endregion

        public NameGenerator(Message inmsg, string suggestedFolder, string suggestedName)
        {
            innerMsg = inmsg;
            this.suggestedFolder = suggestedFolder;
            this.suggestedName = suggestedName;
        }

        protected string ExpandSuggestedName()
        {
            return ExpandSuggestedString(this.suggestedName);
        }

        protected string ExpandSuggestedFolder()
        {
            return ExpandSuggestedString(this.suggestedFolder);
        }

        protected string ExpandSuggestedString(string nameToExpand)
        {
            string currentName = nameToExpand;

            foreach (Match match in rx.Matches(currentName))
            {
                string parameter = match.Groups["parameter"].Value;
                string value = match.Groups.Count > 1 ? match.Groups["value"].Value.ToUpper() : string.Empty;
                Macros macro;

                if (Enum.TryParse(parameter.ToUpper(), out macro))
                {
                    string substitution = EvaluateMacro(macro, value.Replace(":", ""));
                    currentName = currentName.Replace("%" + string.Concat(parameter, value) + "%", substitution);
                }
            }

            return currentName;
        }

        public string FileName
        {
            get
            {
                return ExpandSuggestedName();
            }
        }

        public string Folder
        {
            get
            {
                return ExpandSuggestedFolder();
            }
        }
    }
}
