using static WoW.Launcher.Misc.NativeWindows;
using static WoW.Launcher.Misc.Helpers;

namespace WoW.Launcher.IO;




class WinMemory
{
	public byte[] Data	{ get; set; }
	public nint BaseAddress { get; }

	ProcessBasicInformation _peb;   // ?? 

	readonly nint _processHandle;
	readonly Dictionary<string, (long Address, byte[] Data)> _patchList;

	public WinMemory(ProcessInformation pe, long binaryLength)
	{
		_processHandle = pe.ProcessHandle;
		if (pe.ProcessHandle == IntPtr.Zero)
			throw new InvalidOperationException("No valid process found.");

		BaseAddress = ReadImageBaseFromPEB(_processHandle);
		if (BaseAddress == 0)
			throw new InvalidOperationException("Error while reading PEB data.");

		Data = Read(BaseAddress, (int)binaryLength);

		_patchList = [];
	}

	// 这个是清除内存数据的接口？
	public void RefreshMemoryData(int size)
	{
		// Reset previous memory data.
		Data = null;
		while(Data == null) 
		{
			Console.WriteLine("Refresh client data...");
			Data = Read(BaseAddress, size);
		}
	}

	public nint Read(nint address)
	{
		try
		{
			var buffer = new byte[8];
			if (ReadProcessMemory(_processHandle, address, buffer, buffer.Length, out var dummy))
				return buffer.ToNint();
			
		}
		catch(Exception ex)
		{
			Console.WriteLine(ex.Message);
		}

		return 0;
	}

	public nint Read(long address) => Read((nint)address);
	public byte[] Read(nint address, int size)
	{
		try
		{
			var buffer = new byte[size];
			if (ReadProcessMemory(_processHandle, address, buffer, size, out var dummy))
				return buffer;
		}
		catch(Exception ex)
		{
			Console.WriteLine(ex.Message);
		}

		return null;
	}
	public byte[] Read(long address, int size) => Read((nint)address, size);

	public int ReadDataLength(nint address, string separator)
	{
		var length = 0L;
		var seperatorBytes = Encoding.UTF8.GetBytes(separator).Select(b => (short)b).ToArray();
		var dataLength = 1000;

		// Read in batches here.
		while(length == 0)
		{
			length = Read(address, dataLength)?.FindPattern(seperatorBytes) ?? 0;
			dataLength += 1000;

			// Not found!
			if (dataLength >= 100_000)	// 这个是什么写法？
				return -1;
		}

		return (int)length;
	}

	// 在内存中写入数据？
	public void Write(nint address, byte[] data, MemProtection newProtection = MemProtection.ReadWrite)
	{
		try
		{
			// invoke Win32 API ...
			VirtualProtectEx(_processHandle, address, (uint)data.Length, (uint)newProtection, out var oldProtect);

			WriteProcessMemory(_processHandle, address, data, data.Length, out var written);
			FlushInstructionCache(_processHandle, address, (uint)data.Length);

			VirtualProtectEx(_processHandle, address, (uint)data.Length, oldProtect, out oldProtect);
		}
		catch(Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}
	public void Write(long address, byte[] data, MemProtection newProtection = MemProtection.ReadWrite) => Write((nint)address, data, newProtection);

	// 在内存中打补丁 ... 
	public Task PatchMemory(short[] pattern, byte[] patch, string patchName, bool ? printInfo = null)
	{
		printInfo ??= IsDebugBuild();

		if (printInfo.Value)
			Console.WriteLine($"[{patchName}] Patching...");


	
		return Task.CompletedTask;
	}

	public Task QueuePatch(long patchOffset, byte[] patch, string patchName, bool ? printInfo = null)
	{
		// TODO:
		return Task.CompletedTask;
	}

	public Task QueuePatch(short[] pattern, byte[] patch, string patchName, int offsetBase = 0, bool? printInfo = null)
	{
		// TODO:
		return Task.CompletedTask; 
	}

	bool RemapAndPatch(nint viewAddress, int viewSize)
	{
		// TODO:
		return false;
	}

	void ApplyPatches(bool remap)
	{
		// TODO:
	}

	public bool RemapAndPatch(bool remap)
	{
		// TODO:
		return false;
	}

	nint ReadImageBaseFromPEB(nint processHandle)
	{
		// TODO:
		return 0;
	}

	// 以下的函数的作用是什么呢？

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsShortJump(byte[] instructions, int startIndex = 0)
	{
		// 为什么只判断1个字节？ 完全不需要传入一个数组...
		// 是为了和 IsJump函数统一参数 ...
		return instructions[startIndex] >= 0x70 && instructions[startIndex] <= 0x7F;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsJump(byte[] instructions, int startIndex = 0)
	{
		return (instructions[startIndex] >= 0x0F && instructions[startIndex + 1] >= 0x80 && instructions[startIndex + 1] < 0x8F);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsUnconditionalJump(byte[] instructions, int startIndex = 0)
	{
		return instructions[startIndex] == 0xE9 || instructions[startIndex] == 0xEB;
	}
}
