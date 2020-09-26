using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Essentials
{
    internal class CleanRobot
    {
        public static JObject config = null;
        private const string FolderName = "plugins/clean";
        public const string configFile = "plugins//clean//config.json";

        public static void init()
        {
            if (!Directory.Exists(FolderName))
                Directory.CreateDirectory(FolderName);
            if (!File.Exists(configFile))
                initConfig();
            try { config = JObject.Parse(File.ReadAllText(configFile)); }
            catch { Console.WriteLine("Clean >> 配置文件 config.json 读取失败！"); }
        }

        public static void initConfig()
        {
            var config = new JObject(
                new JProperty("version", Essentials.Version),
                new JProperty("time", 5 * 60000)
            );
            File.WriteAllText(configFile, Function.JsonFomat(config.ToString()));
        }

        public static void Clean()
        {
            while (true)
            {
                int Thirty = (int)config["time"] - 30;
                const int Ten = 20;
                const int Five = 5;
                const int Three = 2;
                const int One = 1;
                Thread.Sleep(Thirty);
                Function.tellraw("@a", CleanLanguage.thirty);
                Thread.Sleep(Ten);
                Function.tellraw("@a", CleanLanguage.ten);
                Thread.Sleep(Five);
                Function.title("@a", CleanLanguage.five);
                Thread.Sleep(Three);
                Function.tellraw("@a", CleanLanguage.three);
                Thread.Sleep(One);
                Function.tellraw("@a", CleanLanguage.two);
                Thread.Sleep(One);
                Function.tellraw("@a", CleanLanguage.one);
                Thread.Sleep(One);
                Essentials.mcapi.runcmd("kill @e[type=item]");
            }
        }
    }
}
