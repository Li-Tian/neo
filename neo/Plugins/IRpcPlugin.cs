using Microsoft.AspNetCore.Http;
using Neo.IO.Json;

namespace Neo.Plugins
{
    // <summary>
    // RPC插件。用于添加自定义RPC指令。
    // </summary>
    /// <summary>
    /// RPC plugin. Used to add custom RPC instructions.
    /// </summary>
    public interface IRpcPlugin
    {
        // <summary>
        // RPC方法处理
        // </summary>
        // <param name="context">Http上下文</param>
        // <param name="method">rpc方法</param>
        // <param name="_params">参数</param>
        // <returns>
        // 如果此插件能处理这个RPC请求，则返回结果。
        // 如果此插件不能处理这个请求，则返回null。系统将继续询问下一个插件。
        // 如果所有插件都不能处理请求时，使用系统默认的RPC处理方法。
        // </returns>
        /// <summary>
        /// Processing RPC method
        /// </summary>
        /// <param name="context">Http context</param>
        /// <param name="method">RPC method</param>
        /// <param name="_params">params</param>
        /// <returns>
        /// If this plugin can handle this RPC request, it returns the result. <br/>
        /// Returns null if this plugin cannot process this request. The system will continue to ask for the next plugin. <br/>
        /// If all plugins can't process the request, use the system default RPC processing method.
        /// </returns>
        JObject OnProcess(HttpContext context, string method, JArray _params);
    }
}
