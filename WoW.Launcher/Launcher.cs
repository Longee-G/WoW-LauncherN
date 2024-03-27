using System.CommandLine.Parsing;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using WoW.Launcher.IO;

namespace WoW.Launcher;



static class Launcher
{
	public static readonly CancellationTokenSource CancellationTokenSource = new();
	//public static async ValueTask<string> PrepareGameLauncher(ParseResult commandLineResult, IPFilter ipfilter)
	//{
	//	return string.Empty;
	//}

	public static bool LaunchGame(string appPath, string gameCommandLine, ParseResult commandLineResult)
	{
		return false;	// TODO:
	}

	static bool IsDevModeAllowed(IPFilter ipfilter, string portal_ip) => ipfilter.IsInRange(portal_ip);

	static long GenerateAuthSeedFunctionPatch(WinMemory memory, long modulusOffset)
	{
		return 0;	// TODO:
	}

	static void WaitForUnpack(ref ProcessInformation pi, WinMemory memory, ref MemoryBasicInformation mbi, Stream gameAppData, bool antiCrash)
	{
		// TODO:
	}

	static void PrepareAntiCrash(WinMemory memory, ref MemoryBasicInformation mbi, ref ProcessInformation pi) 
	{ 
		// TODO:
	}
}
