using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Linq;
using System.IO;
using System;

namespace Essentials
{
    public class Function
    {
        #region API方法补充

        /// <summary>
        /// 格式化 JSON 字符串
        /// </summary>
        /// <param name="str">需要格式化的字符串</param>
        /// <returns>格式化的 JSON 字符串</returns>
        internal static string JsonFomat(string str)
        {
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            return str;
        }

        /// <summary>
        /// 获取玩家的 uuid
        /// </summary>
        /// <param name="name">玩家名称</param>
        /// <returns>玩家 uuid</returns>
        internal static string getUuid(string name) => JArray.Parse(Essentials.mcapi.getOnLinePlayers()).First(l => l.Value<string>("playername") == name).Value<string>("uuid");

        /// <summary>
        /// 获取玩家的 xuid
        /// </summary>
        /// <param name="name">玩家名称</param>
        /// <returns>玩家 xuid</returns>
        internal static string getXuid(string name) => JArray.Parse(Essentials.mcapi.getOnLinePlayers()).First(l => l.Value<string>("playername") == name).Value<string>("xuid");

        /// <summary>
        /// 调用 tellraw 方法
        /// </summary>
        /// <param name="name">玩家名称</param>
        /// <param name="text">发送的文本</param>
        internal static void tellraw(string name, string text) => Essentials.mcapi.runcmd("tellraw " + name + " {\"rawtext\":[{\"text\":\"" + text + "\"}]}");
        
        /// <summary>
        /// 调用 title 方法
        /// </summary>
        /// <param name="name">玩家名称</param>
        /// <param name="text">发送的文本</param>
        internal static void title(string name, string text) => Essentials.mcapi.runcmd(string.Format("title {0} title {1}", name, text));

        #endregion

        #region 经济操作

        /// <summary>
        /// 获取玩家余额
        /// </summary>
        /// <param name="name">玩家名称</param>
        /// <returns>玩家余额</returns>
        public static int getMoney(string name) => JArray.Parse(File.ReadAllText("plugins//Economic//data.json")).First(l => l.Value<string>("name") == name).Value<int>("money");

        /// <summary>
        /// 获取货币单位
        /// </summary>
        /// <returns>货币单位符号</returns>
        public static string getUnit() => JObject.Parse(File.ReadAllText("plugins//Economic//config.json")).Value<string>("unit");

        /// <summary>
        /// 修改玩家余额
        /// </summary>
        /// <param name="name">玩家名称</param>
        /// <param name="type">修改类型</param>
        /// <param name="count">修改数额</param>
        /// <returns>玩家余额是否修改成功</returns>
        public static bool ChangeMoney(string name, string type, int count)
        {
            var data = JArray.Parse(File.ReadAllText(Economic.dataFile));
            for (int i = 0; i < data.Count; i++)
            {
                if ((string)data[i]["name"] == name)
                {
                    switch (type)
                    {
                        case "Add":
                            if (count >= 0)
                            {
                                data[i]["money"] = (int)data[i]["money"] + count;
                                Console.WriteLine("Essentials >> 已从玩家 {0} 增加了 {1} 金币", name, count);
                                File.WriteAllText(Economic.dataFile, JsonFomat(data.ToString()));
                                return true;
                            }
                            return false;
                        case "Set":
                            data[i]["money"] = count;
                            Console.WriteLine("Essentials >> 已从已将玩家 {0} 的余额设置为： {1} 金币", name, count);
                            File.WriteAllText(Economic.dataFile, JsonFomat(data.ToString()));
                            return true;
                        case "Deduct":
                            data[i]["money"] = (int)data[i]["money"] - count;
                            if ((int)data[i]["money"] >= 0)
                            {
                                Console.WriteLine("Essentials >> 已从玩家 {0} 扣除了 {1} 金币", name, count);
                                File.WriteAllText(Economic.dataFile, JsonFomat(data.ToString()));
                                return true;
                            }
                            return false;
                        default:
                            break;
                    }
                    break;
                }
            }
            return false;
        }

        #endregion

        #region 计分板经济操作

        /// <summary>
        /// 获取玩家积分
        /// </summary>
        /// <param name="name">玩家名称</param>
        /// <returns>玩家积分</returns>
        public static int getScore(string name) => Essentials.mcapi.getscoreboard(getUuid(name), (string)Economic.config["scoreboard"]);

        /// <summary>
        /// 获取计分板单位
        /// </summary>
        /// <returns>货币单位符号</returns>
        public static string getScoreboard() => (string)Economic.config["scoreboard"];

        /// <summary>
        /// 修改玩家积分
        /// </summary>
        /// <param name="name">玩家名称</param>
        /// <param name="type">修改类型</param>
        /// <param name="count">修改数额</param>
        /// <returns>玩家积分是否修改成功</returns>
        public static bool ChangeScore(string name, string type, int count)
        {
            var list = JArray.Parse(Essentials.mcapi.getOnLinePlayers());
            bool online = false;
            foreach (JObject player in list)
                if ((string)player["playername"] == name)
                {
                    online = true;
                    break;
                }
            if (!online)
                return false;
            switch (type)
            {
                case "Add":
                    Essentials.mcapi.runcmd(string.Format("scoreboard players add {0} {1} {2}", name, Economic.config["scoreboard"], count));
                    return true;
                case "Set":
                    Essentials.mcapi.runcmd(string.Format("scoreboard players set {0} {1} {2}", name, Economic.config["scoreboard"], count));
                    return true;
                case "Deduct":
                    if (getScore(name) - count >= 0)
                    {
                        Essentials.mcapi.runcmd(string.Format("scoreboard players remove {0} {1} {2}", name, Economic.config["scoreboard"], count));
                        return true;
                    }
                    break;
                default:
                    break;
            }
            return false;
        }

        #endregion
    }
}
