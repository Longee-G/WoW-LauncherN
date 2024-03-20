using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoW.Launcher.Structures;

// 为什么要自己定义一个结构，c#在引用c++的dll的时候没有自己定义吗？
struct StartupInfo
{
	public uint Cb;
	public string Reserved;
	public string Desktop;
	public string Title;
	public uint X;
	public uint Y;
	public uint XSize;
	public uint YSize;
	public uint XCountChars;
	public uint YCountChars;
	public uint FillAttribute;
	public uint Flags;
	public short ShowWindow;
	public short Reserved2;
	public nint ReservedHandle;
	public nint StdInputHandle;
	public nint StdOutputHandle;
	public nint StdErrorHandle;

	public static int Size => Marshal.SizeOf<StartupInfo>();
}
