using Microsoft.AspNetCore.Http;
using Neo.IO.Json;

namespace Neo.Plugins
{
    /// <summary>
    /// RPC插件。用于添加自定义RPC指令。
    /// </summary>
    public interface IRpcPlugin
    {
        /// <summary>
        /// RPC方法处理
        /// </summary>
        /// <param name="context">Http上下文</param>
        /// <param name="method">rpc方法</param>
        /// <param name="_params">参数</param>
        /// <returns>
        /// 如果此插件能处理这个RPC请求，则返回结果。
        /// 如果此插件不能处理这个请求，则返回null。系统将继续询问下一个插件。
        /// 如果所有插件都不能处理请求时，使用系统默认的RPC处理方法。
        /// </returns>
        JObject OnProcess(HttpContext context, string method, JArray _params);
    }
}
