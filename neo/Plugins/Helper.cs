using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;

namespace Neo.Plugins
{
    // <summary>
    // 插件辅助方法
    // </summary>
    /// <summary>
    /// Plugins helper class
    /// </summary>
    public static class Helper
    {
        // <summary>
        // 获取插件配置
        // </summary>
        // <param name="assembly">程序集，加载config.json配置文件</param>
        // <returns>加载的配置</returns>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IConfigurationSection GetConfiguration(this Assembly assembly)
        {
            string path = Path.Combine("Plugins", assembly.GetName().Name, "config.json");
            return new ConfigurationBuilder().AddJsonFile(path, optional: true).Build().GetSection("PluginConfiguration");
        }
    }
}
