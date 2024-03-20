using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoW.Launcher.Structures;

// 这个是什么用处呢？
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct ProcessBasicInformation
{
	public nint ExitStatus;
	public nint PebBaseAddress;
	public nint AffinityMask;
	public nint BasePriority;
	public nint UniqueProcessId;
	public nint InheritedFromUniqueProcessId;

	public static int Size => Marshal.SizeOf<ProcessBasicInformation>();
}
