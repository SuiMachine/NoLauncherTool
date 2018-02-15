using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;


namespace NoLauncherTools
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Check if config file exists - if it does, we load it and start process if it doesn't, we write down parameters and create it
            if(File.Exists(Config.GetSameDirectoryAsExeConfig()))
            {
                if (Config.Init())
                {
                    Process proc = new Process();
                    proc.StartInfo.WorkingDirectory = Config.P_workDir != "" ? Config.P_workDir : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    proc.StartInfo.FileName = Config.P_exe;
                    string joinParams = Config.P_params;
                    if(Config.TranferParameters)
                        joinParams += " " + string.Join(" ", args);
                    proc.StartInfo.Arguments = joinParams;

                    proc.Start();
                    System.Threading.Thread.Sleep(1000);
                }
                else
                    MessageBox.Show("Config file doesn't have required properties.");
            }
            else
            {
                Config.GenerateConfigFile(args);
            }
        }
    }

    static class Config
    {
        public const string CONFIGFILE = "SuiLauncher.cfg";
        public static string P_exe { get; set; }
        public static string P_params { get; set;  }
        public static string P_workDir { get; set; }
        public static bool TranferParameters { get; set; }

        public static string GetSameDirectoryAsExeConfig()
        {
            return Path.Combine((new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName), CONFIGFILE);
        }


        public static bool Init()
        {
            if(File.Exists(CONFIGFILE))              
                return LoadConfig();
            else
                return false;
        }

        public static void GenerateConfigFile(string[] args)
        {
            //These directories are not always the same!
            string AbsoluteCurrentDir = (new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName);
            string AbsoluteWorkDir = Directory.GetCurrentDirectory();
            string AbsoluteExePath = Assembly.GetExecutingAssembly().Location;

            P_exe = AbsoluteExePath.Remove(0, AbsoluteWorkDir.Length + 1);
            P_workDir = AbsoluteWorkDir;
            P_params = string.Join(" ", args);
            TranferParameters = true;

            File.WriteAllText(GetSameDirectoryAsExeConfig(), "EXE:" + P_exe + "\n" +
                "WorkDir:" + P_workDir + "\n" +
                "Parameters:" + P_params + "\n" +
                "TranferParameters:" + TranferParameters.ToString()
                );


        }

        private static bool LoadConfig()
        {
            string[] lines = File.ReadAllLines(GetSameDirectoryAsExeConfig());
            char[] splitCharArray = new char[] { ':' };
            foreach(string line in lines)
            {
                if(line.StartsWith("EXE:"))
                {
                    P_exe = line.Split(splitCharArray, 2)[1];
                }
                else if (line.StartsWith("WorkDir:"))
                {
                    P_workDir = line.Split(splitCharArray, 2)[1];
                }
                else if(line.StartsWith("Parameters:"))
                {
                    P_params = line.Split(splitCharArray, 2)[1];
                }
                else if(line.StartsWith("TranferParameters:"))
                {
                    if(bool.TryParse(line.Split(splitCharArray, 2)[1], out bool temp))
                        TranferParameters = temp;
                }
            }

            if (P_exe == null || !P_exe.ToLower().EndsWith(".exe") || P_params == null || P_workDir == null)
                return false;
            else
                return true;
        }
    }

}
