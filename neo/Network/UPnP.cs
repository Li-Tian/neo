﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;

namespace Neo.Network
{
    // <summary>
    // UPnP（Universal Plug and Play）即插即用协议的实现
    // </summary>
    /// <summary>
    /// UPnP (Universal Plug and Play) implementation
    /// </summary>
    public class UPnP
    {
        private static string _serviceUrl;

        // <summary>
        // Timeout的时间, 默认为3秒
        // </summary>
        /// <summary>
        /// Timeout time, default value is 3 second
        /// </summary>
        public static TimeSpan TimeOut { get; set; } = TimeSpan.FromSeconds(3);

        // <summary>
        // 发送查找消息. 根据Upnp协议多播消息来通知控制点, 然后通过响应的消息找出根设备的url 
        // 通过此URL就可以找到根设备的描述信息，从根设备的描述信息中又可以得到设备的控制URL
        // </summary>
        // <returns>找到设备的控制URL则返回<c>true</c>, 如果超时没找到就返回<c>false</c></returns>
        /// <summary>
        /// Send a lookup message. Notify the control point according to the Upnp protocol multicast message, 
        /// and then find the url of the root device by responding to the message.
        /// You can find the description information of the root device through this URL, 
        /// and you can get the control URL of the device from the description information of the root device.
        /// </summary>
        /// <returns>Find the control URL of the device and return <c>true</c>. 
        /// If it is not found within a timeout time, return <c>false</c></returns>
        public static bool Discover()
        {
            using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                s.ReceiveTimeout = (int)TimeOut.TotalMilliseconds;
                s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                string req = "M-SEARCH * HTTP/1.1\r\n" +
                "HOST: 239.255.255.250:1900\r\n" +
                "ST:upnp:rootdevice\r\n" +
                "MAN:\"ssdp:discover\"\r\n" +
                "MX:3\r\n\r\n";
                byte[] data = Encoding.ASCII.GetBytes(req);
                IPEndPoint ipe = new IPEndPoint(IPAddress.Broadcast, 1900);

                DateTime start = DateTime.Now;

                try
                {
                    s.SendTo(data, ipe);
                    s.SendTo(data, ipe);
                    s.SendTo(data, ipe);
                }
                catch
                {
                    return false;
                }

                byte[] buffer = new byte[0x1000];

                do
                {
                    int length;
                    try
                    {
                        length = s.Receive(buffer);

                        string resp = Encoding.ASCII.GetString(buffer, 0, length).ToLower();
                        if (resp.Contains("upnp:rootdevice"))
                        {
                            resp = resp.Substring(resp.ToLower().IndexOf("location:") + 9);
                            resp = resp.Substring(0, resp.IndexOf("\r")).Trim();
                            if (!string.IsNullOrEmpty(_serviceUrl = GetServiceUrl(resp)))
                            {
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                while (DateTime.Now - start < TimeOut);

                return false;
            }
        }

        private static string GetServiceUrl(string resp)
        {
            try
            {
                XmlDocument desc = new XmlDocument();
                HttpWebRequest request = WebRequest.CreateHttp(resp);
                using (WebResponse response = request.GetResponse())
                {
                    desc.Load(response.GetResponseStream());
                }
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(desc.NameTable);
                nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
                XmlNode typen = desc.SelectSingleNode("//tns:device/tns:deviceType/text()", nsMgr);
                if (!typen.Value.Contains("InternetGatewayDevice"))
                    return null;
                XmlNode node = desc.SelectSingleNode("//tns:service[contains(tns:serviceType,\"WANIPConnection\")]/tns:controlURL/text()", nsMgr);
                if (node == null)
                    return null;
                XmlNode eventnode = desc.SelectSingleNode("//tns:service[contains(tns:serviceType,\"WANIPConnection\")]/tns:eventSubURL/text()", nsMgr);
                return CombineUrls(resp, node.Value);
            }
            catch { return null; }
        }

        private static string CombineUrls(string resp, string p)
        {
            int n = resp.IndexOf("://");
            n = resp.IndexOf('/', n + 3);
            return resp.Substring(0, n) + p;
        }

        // <summary>
        //通过SOAP协议发送指令进行端口映射
        // </summary>
        // <param name="port">端口号</param>
        // <param name="protocol">协议类型</param>
        // <param name="description">对该设备端口映射的描述</param>
        /// <summary>
        /// Sending instructions through the SOAP protocol for port mapping
        /// </summary>
        /// <param name="port">port</param>
        /// <param name="protocol">protocol</param>
        /// <param name="description">description</param>
        public static void ForwardPort(int port, ProtocolType protocol, string description)
        {
            if (string.IsNullOrEmpty(_serviceUrl))
                throw new Exception("No UPnP service available or Discover() has not been called");
            XmlDocument xdoc = SOAPRequest(_serviceUrl, "<u:AddPortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
                "<NewRemoteHost></NewRemoteHost><NewExternalPort>" + port.ToString() + "</NewExternalPort><NewProtocol>" + protocol.ToString().ToUpper() + "</NewProtocol>" +
                "<NewInternalPort>" + port.ToString() + "</NewInternalPort><NewInternalClient>" + Dns.GetHostAddresses(Dns.GetHostName()).First(p => p.AddressFamily == AddressFamily.InterNetwork).ToString() +
                "</NewInternalClient><NewEnabled>1</NewEnabled><NewPortMappingDescription>" + description +
            "</NewPortMappingDescription><NewLeaseDuration>0</NewLeaseDuration></u:AddPortMapping>", "AddPortMapping");
        }

        // <summary>
        // 通过SOAP协议发送指令删除端口映射
        // </summary>
        // <param name="port">端口号</param>
        // <param name="protocol">协议类型</param>
        /// <summary>
        /// Sending instructions through the SOAP protocol to delete port mapping
        /// </summary>
        /// <param name="port">port</param>
        /// <param name="protocol">protocol</param>
        public static void DeleteForwardingRule(int port, ProtocolType protocol)
        {
            if (string.IsNullOrEmpty(_serviceUrl))
                throw new Exception("No UPnP service available or Discover() has not been called");
            XmlDocument xdoc = SOAPRequest(_serviceUrl,
            "<u:DeletePortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
            "<NewRemoteHost>" +
            "</NewRemoteHost>" +
            "<NewExternalPort>" + port + "</NewExternalPort>" +
            "<NewProtocol>" + protocol.ToString().ToUpper() + "</NewProtocol>" +
            "</u:DeletePortMapping>", "DeletePortMapping");
        }

        // <summary>
        // 获取到映射的公网地址
        // </summary>
        // <returns>获取到的公网地址</returns>
        /// <summary>
        /// Get the mapped public network address
        /// </summary>
        /// <returns>public network address</returns>
        public static IPAddress GetExternalIP()
        {
            if (string.IsNullOrEmpty(_serviceUrl))
                throw new Exception("No UPnP service available or Discover() has not been called");
            XmlDocument xdoc = SOAPRequest(_serviceUrl, "<u:GetExternalIPAddress xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
            "</u:GetExternalIPAddress>", "GetExternalIPAddress");
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
            nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
            string IP = xdoc.SelectSingleNode("//NewExternalIPAddress/text()", nsMgr).Value;
            return IPAddress.Parse(IP);
        }

        private static XmlDocument SOAPRequest(string url, string soap, string function)
        {
            string req = "<?xml version=\"1.0\"?>" +
            "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
            "<s:Body>" +
            soap +
            "</s:Body>" +
            "</s:Envelope>";
            HttpWebRequest r = WebRequest.CreateHttp(url);
            r.Method = "POST";
            byte[] b = Encoding.UTF8.GetBytes(req);
            r.Headers["SOAPACTION"] = "\"urn:schemas-upnp-org:service:WANIPConnection:1#" + function + "\"";
            r.ContentType = "text/xml; charset=\"utf-8\"";
            using (Stream reqs = r.GetRequestStream())
            {
                reqs.Write(b, 0, b.Length);
                XmlDocument resp = new XmlDocument();
                WebResponse wres = r.GetResponse();
                using (Stream ress = wres.GetResponseStream())
                {
                    resp.Load(ress);
                    return resp;
                }
            }
        }
    }
}
