using WoW.Launcher.Constants;

namespace WoW.Launcher.Structures;

[StructLayout(LayoutKind.Sequential)]

struct MemoryBasicInformation
{
	public nint BaseAddress;
	public nint AllocationBase;
	public MemProtection AllocationProtect;
	public nint RegionSize;
	public MemState State;
	public MemProtection Protect;
	public MemType Type;

	public static int Size => Marshal.SizeOf<MemoryBasicInformation>();
}
