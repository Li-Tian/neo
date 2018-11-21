using Microsoft.AspNetCore.Http;
using Neo.IO.Json;

namespace Neo.Plugins
{
    /// <summary>
    /// RPC插件
    /// </summary>
    public interface IRpcPlugin
    {
        /// <summary>
        /// RPC方法处理
        /// </summary>
        /// <param name="context">Http上下文</param>
        /// <param name="method">rpc方法</param>
        /// <param name="_params">参数</param>
        /// <returns></returns>
        JObject OnProcess(HttpContext context, string method, JArray _params);
    }
}
