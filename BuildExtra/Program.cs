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

        static void Main(string[] args)
        {
            try
            {
                Init();
                LoadConfig();
                Analysis(args);
                CreateFolder();
                BackupFile();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\r\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// 參數初始化
        /// </summary>
        static void Init()
        {
            Type            = BuildType.Non;
            Version         = string.Empty;
            Di              = null;
            IsBackupDebug   = true;
            IsBackupRelease = true;
            SavePath        = Environment.CurrentDirectory;
        }

        /// <summary>
        /// 讀參數Cfg檔案
        /// </summary>
        static void LoadConfig()
        {
            if (!File.Exists("Config.cfg"))
            {
                Console.WriteLine("找不到Config.cfg檔案，套預設參數。");
                return;
            }

            IsBackupDebug   = new IniBase("BackupDebug", true).GetBool;
            IsBackupRelease = new IniBase("BackupRelease", true).GetBool;
            IsBackupRelease = new IniBase("BackupRelease", true).GetBool;
        }

        /// <summary>
        /// 分析檔案版本位置與權限
        /// </summary>
        /// <param name="args"></param>
        static void Analysis(string[] args)
        {
            if (IsUAC()) throw new Exception("權限不足，請使用UAC權限開啟。");

            if (args.Length == 0) throw new ArgumentException("請輸入引入參數。");

            Fi = new FileInfo(args[0]);

            if (!Fi.Exists) throw new FileNotFoundException("檔案不存在 : " + Fi.FullName);

            Type = GetBiildType(Fi.FullName);
            if (Type == BuildType.Non) throw new ArgumentException("無法分析Build版本為何種類型(debug或release) : " + Fi.FullName);

        }

        /// <summary>
        /// 建立備份資料夾
        /// </summary>
        static void CreateFolder()
        {
            try
            {
                Version = FileVersionInfo.GetVersionInfo(Fi.FullName).FileVersion;
                Di = new DirectoryInfo(
                    Path.Combine(
                        SavePath,
                        Path.GetFileNameWithoutExtension(Fi.Name),
                        Version));
                if (Di.Exists) Di.Create();
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
                Fi.CopyTo(
                    Path.Combine(
                        Di.FullName,
                        Fi.Name));
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
                        return BuildType.Debug;
                    case "RELEASE":
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
    }
}
