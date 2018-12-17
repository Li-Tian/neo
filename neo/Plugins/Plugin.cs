using Microsoft.Extensions.Configuration;
using Neo.Network.P2P.Payloads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Neo.Plugins
{
    /// <summary>
    /// 抽象类插件
    /// </summary>
    public abstract class Plugin
    {
        /// <summary>
        /// 插件集合
        /// </summary>
        public static readonly List<Plugin> Plugins = new List<Plugin>();
        private static readonly List<ILogPlugin> Loggers = new List<ILogPlugin>();
        internal static readonly List<IPolicyPlugin> Policies = new List<IPolicyPlugin>();
        internal static readonly List<IRpcPlugin> RpcPlugins = new List<IRpcPlugin>();
        internal static readonly List<IPersistencePlugin> PersistencePlugins = new List<IPersistencePlugin>();

        private static readonly string pluginsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Plugins");
        private static readonly FileSystemWatcher configWatcher;
        /// <summary>
        /// NeoSystem对象
        /// </summary>
        protected static NeoSystem System { get; private set; }

        /// <summary>
        /// 插件名字
        /// </summary>
        public virtual string Name => GetType().Name;
        /// <summary>
        /// 插件版本
        /// </summary>
        public virtual Version Version => GetType().Assembly.GetName().Version;
        /// <summary>
        /// 获取配置文件的路径
        /// </summary>
        public virtual string ConfigFile => Path.Combine(pluginsPath, GetType().Assembly.GetName().Name, "config.json");
        /// <summary>
        /// 静态的初始化模块
        /// </summary>
        static Plugin()
        {
            if (Directory.Exists(pluginsPath))
            {
                configWatcher = new FileSystemWatcher(pluginsPath, "*.json")
                {
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.Size,
                };
                configWatcher.Changed += ConfigWatcher_Changed;
                configWatcher.Created += ConfigWatcher_Changed;
            }
        }

        /// <summary>
        /// 构造函数：创建插件。
        /// 每生成一个插件的实例，就会将其添加到对应的插件列表里。
        /// </summary>
        protected Plugin()
        {
            Plugins.Add(this);

            if (this is ILogPlugin logger) Loggers.Add(logger);
            if (this is IPolicyPlugin policy) Policies.Add(policy);
            if (this is IRpcPlugin rpc) RpcPlugins.Add(rpc);
            if (this is IPersistencePlugin persistence) PersistencePlugins.Add(persistence);

            Configure();
        }


        /// <summary>
        /// 交易过滤策略。
        /// </summary>
        /// <param name="tx">交易</param>
        /// <returns>返回true则将交易添加到内存池并转发，返回false则抛弃交易</returns>
        public static bool CheckPolicy(Transaction tx)
        {
            foreach (IPolicyPlugin plugin in Policies)
                if (!plugin.FilterForMemoryPool(tx))
                    return false;
            return true;
        }
        /// <summary>
        /// 初始化配置
        /// </summary>
        public abstract void Configure();

        private static void ConfigWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            foreach (var plugin in Plugins)
            {
                if (plugin.ConfigFile == e.FullPath)
                {
                    plugin.Configure();
                    plugin.Log($"Reloaded config for {plugin.Name}");
                    break;
                }
            }
        }

        /// <summary>
        /// 获取插件配置
        /// </summary>
        /// <returns>插件的配置</returns>
        protected IConfigurationSection GetConfiguration()
        {
            return new ConfigurationBuilder().AddJsonFile(ConfigFile, optional: true).Build().GetSection("PluginConfiguration");
        }

        internal static void LoadPlugins(NeoSystem system)
        {
            System = system;
            if (!Directory.Exists(pluginsPath)) return;
            foreach (string filename in Directory.EnumerateFiles(pluginsPath, "*.dll", SearchOption.TopDirectoryOnly))
            {
                Assembly assembly = Assembly.LoadFile(filename);
                foreach (Type type in assembly.ExportedTypes)
                {
                    if (!type.IsSubclassOf(typeof(Plugin))) continue;
                    if (type.IsAbstract) continue;

                    ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                    try
                    {
                        constructor?.Invoke(null);
                    }
                    catch { }
                }
            }
        }
        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="level">日志级别</param>
        protected void Log(string message, LogLevel level = LogLevel.Info)
        {
            Log($"{nameof(Plugin)}:{Name}", level, message);
        }

        /// <summary>
        /// 日志记录
        /// </summary>
        /// <param name="source">日志源</param>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        public static void Log(string source, LogLevel level, string message)
        {
            foreach (ILogPlugin plugin in Loggers)
                plugin.Log(source, level, message);
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>默认返回false</returns>
        protected virtual bool OnMessage(object message) => false;

        /// <summary>
        /// 发送消息给插件
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public static bool SendMessage(object message)
        {
            foreach (Plugin plugin in Plugins)
                if (plugin.OnMessage(message))
                    return true;
            return false;
        }
    }
}
