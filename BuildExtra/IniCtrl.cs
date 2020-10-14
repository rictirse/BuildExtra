using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;

namespace BuildExtra
{
    internal class IniBase
    {
        string IniPath = Path.Combine(Environment.CurrentDirectory, "Config.cfg");

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);

        bool IsdefValIsNullWrite = false;

        public string Tag { get; private set; }
        public string Section { get; private set; }
        string m_Key { get; set; }
        int    m_Int { get; set; }
        float  m_Float { get; set; }
        string m_String { get; set; }
        bool   m_Bool { get; set; }
        Point  m_Point { get; set; }
        Size   m_Size { get; set; }

        public IniBase(string aKey, Size aSize = new Size(), string aTag = "Config", string aSection = "Config")
        {
            m_Key   = aKey;
            m_Size  = aSize;
            Tag     = aTag;
            Section = aSection;
        }

        public IniBase(string aKey, Point aPoint = new Point(), string aTag = "Config", string aSection = "Config")
        {
            m_Key   = aKey;
            m_Point = aPoint;
            Tag     = aTag;
            Section = aSection;
        }

        public IniBase(string aKey, int aInt = 0, string aTag = "Config", string aSection = "Config")
        {
            m_Key   = aKey;
            m_Int   = aInt;
            Tag     = aTag;
            Section = aSection;
        }

        public IniBase(string aKey, float aFloat = 0f, string aTag = "Config", string aSection = "Config")
        {
            m_Key     = aKey;
            m_Float   = aFloat;
            Tag     = aTag;
            Section = aSection;
        }

        public IniBase(string aKey, string aString = "", string aTag = "Config", string aSection = "Config")
        {
            m_Key    = aKey;
            m_String = aString;
            Tag      = aTag;
            Section  = aSection;
        }

        public IniBase(string aKey, bool aBool = false, string aTag = "Config", string aSection = "Config")
        {
            m_Key   = aKey;
            m_Bool  = aBool;
            Tag     = aTag;
            Section = aSection;
        }

        public Point GetPoint
        {
            get { return ReadPoint(Section, m_Key); }
            set { Write(Section, m_Key, value); }
        }

        public Size GetSize
        {
            get { return ReadSize(Section, m_Key); }
            set { Write(Section, m_Key, value); }
        }

        public float GetFloat
        {
            get { return ReadFloat(Section, m_Key); }
            set { Write(Section, m_Key, value); }
        }

        public int GetInt
        {
            get { return ReadInt(Section, m_Key); }
            set { Write(Section, m_Key, value); }
        }

        public string GetString
        {
            get { return ReadString(Section, m_Key); }
            set { Write(Section, m_Key, value); }
        }

        public bool GetBool
        {
            get { return ReadBool(Section, m_Key); }
            set { Write(Section, m_Key, value); }
        }

        private string ReadString(string aSection, string aKey)
        {
            var byt = new byte[500];
            string s;

            int status = GetPrivateProfileString(aSection, aKey, null, byt, 500, IniPath);

            if (status != 0)
            {
                s = Encoding.Unicode.GetString(byt).Trim('\0');
            }
            else
            {
                s = m_String;
                if (IsdefValIsNullWrite)
                { 
                    Write(aSection, aKey, s); //如果key是空的是否要寫入
                }
            }

            return s;
        }

        private bool ReadBool(string aSection, string aKey)
        {
            var byt = new byte[16];
            bool b = false;

            int status = GetPrivateProfileString(aSection, aKey, null, byt, 16, IniPath);
            var sTmp = Encoding.Unicode.GetString(byt).Trim('\0');

            if (status != 0)
            {
                b = sTmp.ToString() == "true" ? true : false;
            }
            else
            {
                b = m_Bool;
                if (IsdefValIsNullWrite)
                { 
                    Write(aSection, aKey, b); //如果key是空的是否要寫入
                }
            }

            return b;
        }

        private float ReadFloat(string aSection, string aKey)
        {
            var byt = new byte[32];
            var f = 0f;

            int status = GetPrivateProfileString(aSection, aKey, null, byt, 16, IniPath);
            var sTmp = Encoding.Unicode.GetString(byt).Trim('\0');

            if (status == 0 || !float.TryParse(sTmp, out f))
            {
                f = m_Float;
                if (IsdefValIsNullWrite)
                { 
                    Write(aSection, aKey, f);
                }
            }

            return f;
        }

        private int ReadInt(string aSection, string aKey)
        {
            var byt = new byte[16];
            int i = 0;

            int status = GetPrivateProfileString(aSection, aKey, null, byt, 16, IniPath);
            var sTmp = Encoding.Unicode.GetString(byt).Trim('\0');

            if (status == 0 || !int.TryParse(sTmp, out i))
            {
                i = m_Int;
                if (IsdefValIsNullWrite)
                {
                    Write(aSection, aKey, i);
                }
            }

            return i;
        }

        private Size ReadSize(string aSection, string aKey)
        {
            var byt = new byte[16];
            var s = new Size(0, 0);

            int status = GetPrivateProfileString(aSection, aKey, null, byt, 16, IniPath);
            var sTmp = Encoding.Unicode.GetString(byt).Trim('\0').Split(',');

            if (status != 0 && sTmp.Length == 2)
            {
                int Var;
                if (!int.TryParse(sTmp[0], out Var))
                {
                    Var = 0;
                }
                s.Width = Var;

                if (!int.TryParse(sTmp[1], out Var))
                {
                    Var = 0;
                }
                s.Height = Var;
            }
            else
            {
                s = m_Size;
                if (IsdefValIsNullWrite)
                { 
                    Write(aSection, aKey, s);
                }
            }
            return s;
        }

        private Point ReadPoint(string aSection, string aKey)
        {
            var byt = new byte[16];
            var p = new Point(0, 0);

            int status = GetPrivateProfileString(aSection, aKey, null, byt, 16, IniPath);
            var sTmp = Encoding.Unicode.GetString(byt).Trim('\0').Split(',');

            if (status != 0 && sTmp.Length == 2)
            {
                int Var;
                if (!int.TryParse(sTmp[0], out Var))
                {
                    Var = 0;
                }
                p.X = Var;

                if (!int.TryParse(sTmp[1], out Var))
                {
                    Var = 0;
                }
                p.Y = Var;
            }
            else
            {
                p = m_Point;
                if (IsdefValIsNullWrite)
                { 
                    Write(aSection, aKey, p);
                }
            }
            return p;
        }

        private void Write(string aSection, string aKey, Size aValue)
        {
            WritePrivateProfileString(aSection, aKey, string.Format("{0},{1}", aValue.Width, aValue.Height), IniPath);
        }

        private void Write(string aSection, string aKey, Point aValue)
        {
            WritePrivateProfileString(aSection, aKey, string.Format("{0},{1}", aValue.X, aValue.Y), IniPath);
        }

        private void Write(string aSection, string aKey, string aValue)
        {
            WritePrivateProfileString(aSection, aKey, aValue, IniPath);
        }

        private void Write(string aSection, string aKey, int aValue)
        {
            WritePrivateProfileString(aSection, aKey, string.Format("{0:D}", aValue), IniPath);
        }

        private void Write(string aSection, string aKey, bool aValue)
        {
            WritePrivateProfileString(aSection, aKey, aValue ? "true" : "false", IniPath);
        }

        private void Write(string aSection, string aKey, float aValue)
        {
            WritePrivateProfileString(aSection, aKey, string.Format("{0:F6}", aValue), IniPath);
        }
    }
}
