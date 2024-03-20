using WoW.Launcher.Constants;

namespace WoW.Launcher.Structures;

[StructLayout(LayoutKind.Sequential)]

struct MemoryBasicInformation
{
	public nint BaseAddress;
	public nint AllocationBase;
	public MemProtection AllocationProtect;
}
