using System;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CSR;

namespace Essentials
{
    internal class Essentials
    {
        /// <summary>
        /// MC相关API方法
        /// </summary>
        public static MCCSAPI mcapi = null;
        public const string Version = "1.0.0";
        public const string Author = "SnowPea8072";
        public const string PluginName = "Essentials";
        private const string BDSName = "plugins";
        private const string configFile = "plugins//Essentials.json";

        #region 初始化文件

        /// <summary>
        /// 初始化文件
        /// </summary>
        private static void initPlugin()
        {
            if (!Directory.Exists(BDSName))
                Directory.CreateDirectory(BDSName);
            if (!File.Exists(configFile))
                initConfig();

            // var config = JObject.Parse(File.ReadAllText(configFile));
            Economic.init();
            CleanRobot.init();
            /* if (config["tpa"])
                Tpa.init(mcapi);
            if (config["tpr"])
                Tpr.init(mcapi);
            if (config["back"])
                Back.init(mcapi);
            if (config["blacklist"])
                BlackList.init(mcapi);
            if (config["home"])
            {
                Home.init(mcapi);
                HomeT.init(mcapi);
            } */

            /* mcapi.setCommandDescribe("money", "打开经济管理");
            if ((bool)config["tpa"])
                mcapi.setCommandDescribe("tpa", "打开传送菜单");
            if ((bool)config["tpr"])
                mcapi.setCommandDescribe("tpr", "随机传送");
            if ((bool)config["back"])
                mcapi.setCommandDescribe("back", "返回死亡点");
            if ((bool)config["blacklist"])
                mcapi.setCommandDescribeEx("ban", "打开封禁管理", MCCSAPI.CommandPermissionLevel.Admin, (byte)MCCSAPI.CommandCheatFlag.NotCheat, (byte)MCCSAPI.CommandVisibilityFlag.Visible);
            if ((bool)config["home"])
            {
                mcapi.setCommandDescribe("home", "打开家园菜单");
                mcapi.setCommandDescribe("homet", "返回至默认家");
            } */
        }

        /// <summary>
        /// 初始化 config.json 文件
        /// </summary>
        private static void initConfig()
        {
            var config = new JObject(
                new JProperty("version", Version),
                new JProperty("tpa", true),
                new JProperty("tpr", true),
                new JProperty("back", true),
                new JProperty("home", true),
                new JProperty("clean", true),
                new JProperty("behavior", true),
                new JProperty("blacklist", true)
            );
            File.WriteAllText(configFile, Function.JsonFomat(config.ToString()), Encoding.UTF8);
        }

        #endregion

        #region 玩家信息核查

        /// <summary>
        /// 设置玩家加入游戏监听
        /// </summary>
        private static bool PlayerJoin(Events x)
        {
            var json = BaseEvent.getFrom(x) as LoadNameEvent;
            // BlackList.Check(json.playername, api);
            Economic.Add(json.playername);
            return true;
        }

        #endregion

        #region 后台命令

        private static bool ServerCmd(Events x)
        {
            var json = BaseEvent.getFrom(x) as ServerCmdEvent;
            switch (json.cmd)
            {
                case "money reload":
                    if (File.Exists(Economic.configFile))
                    {
                        try { Economic.config = JObject.Parse(File.ReadAllText(Economic.configFile)); }
                        catch { Console.WriteLine("Economic >> 配置文件 config.json 读取失败！"); }
                    }
                    else
                    {
                        Economic.initConfig();
                        Console.WriteLine("Economic >> 未找到配置文件 config.json，正在为您生成！");
                    }
                    return false;
                case "clean reload":
                    if (File.Exists(CleanRobot.configFile))
                    {
                        try { CleanRobot.config = JObject.Parse(File.ReadAllText(CleanRobot.configFile)); }
                        catch { Console.WriteLine("Economic >> 配置文件 config.json 读取失败！"); }
                    }
                    else
                    {
                        CleanRobot.initConfig();
                        Console.WriteLine("Economic >> 未找到配置文件 config.json，正在为您生成！");
                    }
                    return false;
                default:
                    break;
            }
            return true;
        }

        #endregion

        #region 玩家命令

        /*
        /// <summary>
        /// 设置玩家命令监听
        /// </summary>
        private static bool InputCommand(Events x)
        {
            var json = BaseEvent.getFrom(x) as InputCommandEvent;
            var config = JObject.Parse(File.ReadAllText(configFile));
            if (json.cmd == "/tpa" && (bool)config["tpa"])
            {
                Tpa.Choose(json.playername, api);
                return false;
            }
            else if (json.cmd == "/tpr" && (bool)config["tpr"])
            {
                Tpr.Do(json.playername, api);
                return false;
            }
            else if (json.cmd == "/back" && (bool)config["back"])
            {
                Back.Do(json.playername, api);
                return false;
            }
            else if (json.cmd == "/home" && (bool)config["home"])
            {
                Home.Choose(json.playername, api);
                return false;
            }
            else if (json.cmd == "/homet" && (bool)config["home"])
            {
                HomeT.Do(json.playername, api);
                return false;
            }
            else if (json.cmd == "/money")
            {
                Economic.Choose(json.playername);
                return false;
            } 
            else if (json.cmd == "/ban" && (bool)config["blacklist"])
            {
                BlackList.Choose(json.playername, api);
                return false;
            }
            return true;
        } */

        #endregion

        /// <summary>
        /// 初始化插件
        /// </summary>
        /// <param name="api">MC相关API方法</param>
        public static void init(MCCSAPI api)
        {
            mcapi = api;

            initPlugin();

            #region 设置事件监听器

            api.addAfterActListener(EventKey.onLoadName, PlayerJoin);
            // api.addAfterActListener(EventKey.onFormSelect, FormSelect);
            api.addBeforeActListener(EventKey.onServerCmd, ServerCmd);
            // api.addBeforeActListener(EventKey.onInputCommand, InputCommand);

            var Clean = new Thread(new ThreadStart(CleanRobot.Clean));
            Clean.Name = "扫地机器人";
            Clean.Start();

            #endregion
        }
    }
}



namespace CSR
{
    partial class Plugin
    {
        /// <summary>
        /// 静态api对象
        /// </summary>
        public static MCCSAPI api { get; private set; } = null;

        /// <summary>
        /// 插件装载时的事件
        /// </summary>
        public static int onServerStart(string pathandversion)
        {
            string[] pav = pathandversion.Split(',');
            if (pav.Length > 1)
            {
                string path = pav[0];
                string version = pav[1];
                bool commercial = (pav[pav.Length - 1] == "1");
                api = new MCCSAPI(path, version, commercial);
                if (api != null)
                {
                    onStart(api);
                    GC.KeepAlive(api);
                    return 0;
                }
            }
            Console.WriteLine("Load failed.");
            return -1;
        }

        /// <summary>
        /// 通用调用接口
        /// </summary>
        /// <param name="api">MC相关调用方法</param>
        public static void onStart(MCCSAPI api)
        {
            // TODO 此接口为必要实现
            Essentials.Essentials.init(api);
            Console.WriteLine("[{0}]{1}插件加载成功！ By：{2}", Essentials.Essentials.Version, Essentials.Essentials.PluginName, Essentials.Essentials.Author);
        }
    }
}
