using System;
using System.Text;
using Network;
using Newtonsoft.Json;
using SapphireEngine;
using UnityEngine;

namespace SapphireEmu.Data.Base
{
    public partial class ConsoleNetwork
    {
        public class Arg
    {

        public string FullString = "";

        public string Command;
        public string[] Args;

        public Connection Connection { get; private set; }

        public bool Invalid { get; private set; }
        public string Reply { get; private set; }
        
        public bool IsAdmin => this.IsConnectionAdmin;

        public bool IsConnectionAdmin
        {
            get
            {
                if (this.Connection == null || !this.Connection.connected)
                {
                    return false;
                }
                return this.Connection.authLevel != 0;
            }
        }

        internal Arg(string command, Connection connection)
            : this(command)
        {
            this.Connection = connection;
        }
        internal Arg(string command)
        {
            command = RemoveInvalidCharacters(command);
            this.BuildCommand(command);
        }

        internal void BuildCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                this.Invalid = true;
                return;
            }

            string str = command;
            if (str.Length < 1)
            {
                return;
            }

            int num1 = str.IndexOf(' ');
            if (num1 > 0)
            {
                this.FullString = str.Substring(num1 + 1);
                this.FullString = this.FullString.Trim();
                this.Args = this.FullString.SplitQuotesStrings();
                str = str.Substring(0, num1);
            }

            str = str.Trim().ToLower();
            this.Command = str;
        }


        public bool GetBool(int iArg, bool def = false)
        {
            string str = this.GetString(iArg, null);
            if (str == null)
            {
                return def;
            }
            if (str == string.Empty || str == "0")
            {
                return false;
            }
            if (str.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (str.Equals("no", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (str.Equals("none", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (str.Equals("null", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            return true;
        }

        public Color GetColor(int iArg, Color def)
        {
            string str = this.GetString(iArg, null);
            if (str == null)
            {
                return def;
            }
            return str.ToColor();
        }

        public float GetFloat(int iArg, float def = 0f)
        {
            float single;
            string str = this.GetString(iArg, null);
            if (str == null)
            {
                return def;
            }
            if (float.TryParse(str, out single))
            {
                return single;
            }
            return def;
        }

        public int GetInt(int iArg, int def = 0)
        {
            int num;
            string str = this.GetString(iArg, null);
            if (str == null)
            {
                return def;
            }
            if (int.TryParse(str, out num))
            {
                return num;
            }
            return def;
        }

        public string GetString(int iArg, string def = "")
        {
            if (!this.HasArgs(iArg + 1))
            {
                return def;
            }
            return this.Args[iArg];
        }

        public TimeSpan GetTimeSpan(int iArg)
        {
            TimeSpan timeSpan;
            string str = this.GetString(iArg, null);
            if (str == null)
            {
                return TimeSpan.FromSeconds(0);
            }
            if (TimeSpan.TryParse(str, out timeSpan))
            {
                return timeSpan;
            }
            return TimeSpan.FromSeconds(0);
        }

        public uint GetUInt(int iArg, uint def = 0)
        {
            uint num;
            string str = this.GetString(iArg, null);
            if (str == null)
            {
                return def;
            }
            if (uint.TryParse(str, out num))
            {
                return num;
            }
            return def;
        }

        public ulong GetUInt64(int iArg, ulong def = 0L)
        {
            ulong num;
            string str = this.GetString(iArg, null);
            if (str == null)
            {
                return def;
            }
            if (ulong.TryParse(str, out num))
            {
                return num;
            }
            return def;
        }

        public ulong GetULong(int iArg, ulong def = 0L)
        {
            ulong num;
            string str = this.GetString(iArg, null);
            if (str == null)
            {
                return def;
            }
            if (ulong.TryParse(str, out num))
            {
                return num;
            }
            return def;
        }

        public Vector3 GetVector3(int iArg, Vector3 def )
        {
            string str = this.GetString(iArg, null);
            if (str == null)
            {
                return def;
            }
            return str.ToVector3();
        }

        public bool HasArgs(int iMinimum = 1)
        {
            if (this.Args == null)
            {
                return false;
            }
            return (int)this.Args.Length >= iMinimum;
        }
        
        internal static string RemoveInvalidCharacters(string str)
        {
            if (str == null)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                char chr = str[i];
                if (char.IsLetterOrDigit(chr) || char.IsPunctuation(chr) || char.IsSeparator(chr) || char.IsSymbol(chr) || chr == '\n' || chr == '\t')
                {
                    stringBuilder.Append(chr);
                }
            }
            return stringBuilder.ToString();
        }

        public void ReplyWith(string strValue)
        {
            this.Reply = strValue;
        }

        public void ReplyWithObject(object rval)
        {
            if (rval == null)
            {
                return;
            }
            if (rval is string)
            {
                this.ReplyWith((string)rval);
                return;
            }
            this.ReplyWith(JsonConvert.SerializeObject(rval));
        }
    }
    }
}