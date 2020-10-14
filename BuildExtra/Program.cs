using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BuildExtra
{
    enum BuildType
    { 
        Non = -1,
        Debug,
        Release,
    }

    class Program
    {
        static BuildType Type { get; set; }
        static FileInfo Fi { get; set; }
        static DirectoryInfo Di { get; set; }
        static bool IsBackupDebug { get; set; }
        static bool IsBackupRelease { get; set; }
        static string Version { get; set; }
        static string SavePath { get; set; }
        static string ExeCurrentPath { get; set; }

        static void Main(string[] args)
        {
            try
            {
                Init();
                LoadConfig();
                Analysis(args);
                BackupFile();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 參數初始化
        /// </summary>
        static void Init()
        {
            Console.WriteLine(@"
                 ____  __  __  ____  __    ____     ____    __    ___  _  _  __  __  ____ 
                (  _ \(  )(  )(_  _)(  )  (  _ \   (  _ \  /__\  / __)( )/ )(  )(  )(  _ \
                 ) _ < )(__)(  _)(_  )(__  )(_) )   ) _ < /(__)\( (__  )  (  )(__)(  )___/
                (____/(______)(____)(____)(____/   (____/(__)(__)\___)(_)\_)(______)(__)  ");


            ExeCurrentPath  = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName;
            Type            = BuildType.Non;
            Version         = string.Empty;
            Di              = null;
            IsBackupDebug   = true;
            IsBackupRelease = true;
            SavePath        = string.Empty;
        }

        /// <summary>
        /// 讀參數Cfg檔案
        /// </summary>
        static void LoadConfig()
        {
            if (!File.Exists(Path.Combine(ExeCurrentPath, "Config.cfg")))
            {
                Console.WriteLine("找不到Config.cfg檔案，套預設參數。");
                return;
            }

            IsBackupDebug   = new IniBase("BackupDebug", true).GetBool;
            IsBackupRelease = new IniBase("BackupRelease", true).GetBool;
            SavePath        = new IniBase("SavePath", "").GetString;
        }

        /// <summary>
        /// 分析檔案版本位置與權限
        /// </summary>
        /// <param name="args"></param>
        static void Analysis(string[] args)
        {
            string _value = string.Empty;
            var _cmds = new Dictionary<string, string>();

            if (IsUAC()) throw new Exception("權限不足，請使用UAC權限開啟。");

            if (args.Length == 0) throw new ArgumentException("請輸入引入參數。");

            _cmds = CmdToDic(args);

            if (!_cmds.TryGetValue("source", out _value)) throw new FileNotFoundException("目標檔案不存在，或指令錯誤。\r\n" + Fi.FullName);
            Fi = new FileInfo(_value);

            if (!Fi.Exists) throw new FileNotFoundException("檔案不存在 : " + Fi.FullName);

            Type = GetBiildType(Fi.FullName);
            if (Type == BuildType.Non) throw new ArgumentException("無法分析Build版本為何種類型(debug或release)。\r\n" + Fi.FullName);

            if (!_cmds.TryGetValue("target", out _value)) { _value = string.Empty; }

            CreateFolder(_value);
        }

        /// <summary>
        /// 建立備份資料夾
        /// 預設存檔目錄跟檔案相同
        /// </summary>
        static void CreateFolder(string aSavePath = "")
        {
            try
            {
                if (string.IsNullOrEmpty(aSavePath))
                {
                    aSavePath = Path.Combine(
                                    ExeCurrentPath,
                                    Path.GetFileNameWithoutExtension(Fi.Name));
                }

                Di = new DirectoryInfo(aSavePath);

                if (!Di.Exists) Di.Create();
            }
            catch (IOException) { throw; }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// 備份檔案
        /// </summary>
        static void BackupFile()
        {
            try
            {
                string targetFilePath = Path.Combine(
                        Di.FullName,
                        string.Format("{0}{1}_{2}{3}",
                            Type == BuildType.Debug ? "d_" : "",
                            Path.GetFileNameWithoutExtension(Fi.Name),
                            FileVersionInfo.GetVersionInfo(Fi.FullName).FileVersion,
                            Fi.Extension));

                Fi.CopyTo(targetFilePath);

                Console.WriteLine("Source : " + Fi.FullName +  "\r\nTarget : " + 
                    targetFilePath + "\r\n" +
                    (File.Exists(targetFilePath) ? "複製成功。" : "複製失敗。"));
                
            }
            catch (IOException) { throw; }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// 分析檔案路徑是Debug還是Release
        /// </summary>
        /// <param name="aFileFullPath"></param>
        /// <returns></returns>
        static BuildType GetBiildType(string aFileFullPath)
        {
            var sp = aFileFullPath.Split('\\');

            foreach (var _str in sp)
            {
                switch (_str.ToUpper())
                {
                    case "DEBUG":
                        if (IsBackupDebug) throw new Exception("Debug不備份，詳閱設定");
                        return BuildType.Debug;
                    case "RELEASE":
                        if (IsBackupRelease) throw new Exception("Release不備份，詳閱設定");
                        return BuildType.Release;
                }
            }

            return BuildType.Non;
        }

        /// <summary>
        /// 確認是否有UAC權限
        /// </summary>
        /// <returns></returns>
        public static bool IsUAC()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Command 轉 Dictionary
        /// </summary>
        /// <param name="aArrCmdLines"></param>
        private static Dictionary<string, string> CmdToDic(string[] aArrCmdLines)
        {
            var dicCmd = new Dictionary<string, string>();

            for (int i = 0; i < aArrCmdLines.Length; i++)
            {
                try
                {
                    var _cmdSepa = aArrCmdLines[i].Remove(1);
                    if (_cmdSepa == "-" || _cmdSepa == "/")
                    {
                        if (i + 1 < aArrCmdLines.Length)
                        {
                            var ___cmdSepa = aArrCmdLines[i + 1].Remove(1);
                            if (___cmdSepa != "-" && ___cmdSepa != "/")
                            {
                                dicCmd.Add(aArrCmdLines[i].Substring(1).ToLower(), aArrCmdLines[i + 1]);
                                continue;
                            }
                        }
                        dicCmd.Add(aArrCmdLines[i].Substring(1).ToLower(), string.Empty);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Command Error.\r\n" + e.Message);
                }
            }

            return dicCmd;
        }
    }
}
