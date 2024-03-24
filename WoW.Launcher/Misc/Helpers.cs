// Copyright (c) Arctium.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace WoW.Launcher.Misc;

static class Helpers
{
    public static bool IsDebugBuild()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }


    // 从文件中获取版本信息
    public static (int Major, int Minor, int Revision, int Build) GetVersionValueFromClient(string filename)
    {
        if (string.IsNullOrEmpty(filename))
        {
            throw new ArgumentNullException("invalid filename");
        }

        var fvi = FileVersionInfo.GetVersionInfo(filename);
        return (fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
    }

    public static void PrintHeader(string serverName)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Operating System: {RuntimeInformation.OSDescription}");
    }

    // 解析config.wtf中的portal数据 `SET portal "127.0.0.1:3367"`
    public static (string IPAddress, string HostName, int Port) ParsePortal(string config)
    {
        const string portalKey = "SET portal";
        var portalIndex = config.IndexOf(portalKey, StringComparison.Ordinal);

        if (portalIndex == -1)
            throw new ArgumentException("portal node `SET portal` NOT FOUND in config.wtf!");

        var startQuoteIndex = config.IndexOf('"', portalIndex);
        if (startQuoteIndex == -1)
            throw new ArgumentException("Invalid format for the `SET portal` variable.");

        var endQuoteIndex = config.IndexOf('"', startQuoteIndex + 1);
        if (endQuoteIndex == -1)
            throw new ArgumentException("Invalid format for the `SET portal` variable.");

        var portalLength = endQuoteIndex - startQuoteIndex - 1;
        var portalSpan = config.AsSpan(startQuoteIndex + 1, portalLength);  // 解析出引号中的ip地址
        var colonIndex = portalSpan.IndexOf(':');
        var ipSpan = colonIndex != -1 ? portalSpan[..colonIndex] : portalSpan;
        var port = colonIndex != -1 ? int.Parse(portalSpan[(colonIndex + 1)..]) : 1119;
        var portalString = ipSpan.ToString().Trim();    // hostname string

        try
        {
            if (IPAddress.TryParse(portalString, out var ipaddr))
                return (ipaddr.ToString(), portalString, port);

            var ipv4addr = Dns.GetHostAddresses(portalString).FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                ?? throw new Exception("No IPv4 address found for the provided hostname.");

            return (ipv4addr.ToString(), portalString, port);
        }
        catch (SocketException)
        {
            Console.WriteLine("No valid portal found. Dev (Local) mode disabled.");
            return (string.Empty, string.Empty, port);
        }
    }

    // 定义一个可以异步运行的函数？是在线程中运行的吗？
    public static async Task<bool> CheckUrl(string url, string fallbackUrl)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);

        try
        {
            var result = await httpClient.GetAsync(url);
            if (!result.IsSuccessStatusCode)
                Console.WriteLine($"{url} not reachable. Falling back to {fallbackUrl}");
            return result.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            Console.WriteLine($"{url} not reachable. Falling back to {fallbackUrl}");

            return false;
        }
    }
}
