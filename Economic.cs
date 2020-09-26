using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Essentials
{
    internal class Economic
    {
        public static JObject config = null;
        private const string FolderName = "plugins/Economic";
        public const string dataFile = "plugins//Economic//data.json";
        public const string configFile = "plugins//Economic//config.json";

        #region 初始化文件

        /// <summary>
        /// 初始化文件
        /// </summary>
        public static void init()
        {
            if (!Directory.Exists(FolderName))
                Directory.CreateDirectory(FolderName);
            if (!File.Exists(dataFile))
                initData();
            if (!File.Exists(configFile))
                initConfig();
            try { config = JObject.Parse(File.ReadAllText(configFile)); }
            catch { Console.WriteLine("Economic >> 配置文件 config.json 读取失败！"); }
            Essentials.mcapi.runcmd(Function.Unicode(string.Format("scoreboard objectives add {0} dummy 玩家积分", config["scoreboard"])));
        }

        /// <summary>
        /// 初始化 data.json 文件
        /// </summary>
        private static void initData()
        {
            var data = new JArray();
            File.WriteAllText(dataFile, Function.JsonFomat(data.ToString()));
        }

        /// <summary>
        /// 初始化 config.json 文件
        /// </summary>
        public static void initConfig()
        {
            var config = new JObject(
                new JProperty("version", Essentials.Version),
                new JProperty("scoreboard", "money"),
                new JProperty("unit", "$"),
                new JProperty("default", 0)
            );
            File.WriteAllText(configFile, Function.JsonFomat(config.ToString()));
        }

        #endregion

        /// <summary>
        /// 初始化玩家经济
        /// </summary>
        /// <param name="name">玩家名称</param>
        public static void Add(string name)
        {
            if (File.Exists(dataFile))
            {
                try
                {
                    var data = JArray.Parse(File.ReadAllText(dataFile));
                    bool here = false;
                    foreach (JObject money in data)
                        if ((string)money["name"] == name)
                        {
                            here = true;
                            break;
                        }
                    if (!here)
                    {
                        var money = new JObject(
                            new JProperty("name", name),
                            new JProperty("money", config["default"])
                        );
                        data.Add(money);
                        File.WriteAllText(dataFile, Function.JsonFomat(data.ToString()));
                    }
                }
                catch { Console.WriteLine("Economic >> 配置文件 data.json 读取失败！"); }
            }
            else
            {
                initData();
                Console.WriteLine("Economic >> 未找到配置文件 data.json，正在为您生成！");
            }
        }

        public static void Choose(string name)
        {

        }
    }
}
