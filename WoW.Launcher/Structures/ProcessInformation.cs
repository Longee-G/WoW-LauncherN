using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoW.Launcher.Structures;

struct ProcessInformation
{
	public nint ProcessHandle;      // nint 是一个新的类型？
	public nint ThreadHandle;
	public uint ProcessId;
	public uint ThreadId;

	public static int Size => Marshal.SizeOf<ProcessInformation>();
}
