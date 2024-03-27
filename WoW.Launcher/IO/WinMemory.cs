﻿using static WoW.Launcher.Misc.NativeWindows;
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

		// 查找内存位置
		long patchOffset = Data.FindPattern(pattern, BaseAddress);
		if (patchOffset == 0)	// 为什么用0来表示无效位置 ...
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine($"[{patchName}] No result found.");
			Console.ReadKey();

			Launcher.CancellationTokenSource.Cancel();
		}

		// ?. 的用法是什么呢？ 这个代码的意思是什么？
		while (Read(patchOffset, patch.Length)?.SequenceEqual(patch) == false)
			Write(patchOffset, patch);

		if(printInfo.Value)
		{
			Console.Write($"[{patchName}]");
			Console.ForegroundColor= ConsoleColor.Green;
			Console.WriteLine(" Done.");
			Console.ResetColor();
			Console.WriteLine();
		}
	
		return Task.CompletedTask;
	}

	public Task QueuePatch(long patchOffset, byte[] patch, string patchName, bool ? printInfo = null)
	{
		Launcher.CancellationTokenSource.Token.ThrowIfCancellationRequested();

		printInfo ??= IsDebugBuild();

        if (printInfo.Value)
        {
			Console.WriteLine($"[{patchName}] Adding...");
			_patchList[patchName] = (patchOffset, patch);

			Console.Write($"[{patchName}]");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(" Done.");
			Console.ResetColor();
			Console.WriteLine();
        }
		else
			_patchList[patchName] = ( patchOffset, patch);

		return Task.CompletedTask;
	}

	public Task QueuePatch(short[] pattern, byte[] patch, string patchName, int offsetBase = 0, bool? printInfo = null)
	{
		long patchOffset = Data.FindPattern(pattern);

		// No result for the given pattern.
		if(patchOffset == 0)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"[{patchName}] No result found.");
			Console.ResetColor();
			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();

			Launcher.CancellationTokenSource.Cancel();
			return Task.CompletedTask;
		}
		
		return QueuePatch(patchOffset + offsetBase, patch, patchName, printInfo);
	}

	// 这个函数的作用是啥？
	// viewAddress 和 viewSize来自哪里？

	bool RemapAndPatch(nint viewAddress, int viewSize)
	{
		// Suspend before remapping to prevent crashes.
		NtSuspendProcess(_processHandle);
		Data = Read(viewAddress, viewSize);

		if (Data != null)
		{
			nint newViewHandle = 0;
			var maxSize = new LargeInteger { Quad = viewSize };
			try
			{
				// 这个函数起啥作用？参数的含义是啥呢
				if(NtCreateSection(ref newViewHandle, 0xF001F, 0, ref maxSize, 0x40u, 0x8000000 | 0x400000, 0) == NtStatus.Success &&
					NtUnmapViewOfSection(_processHandle, viewAddress) == NtStatus.Success)
				{
					var viewBase = viewAddress;

					// Map the view with original protections.
					// 感觉是不是修改exe的运行权限，让patch可以不出现问题，等修改完了再将权限恢复...
					var result = NtMapViewOfSection(newViewHandle, _processHandle, ref viewBase, 0, (ulong)viewSize, out var viewOffset,
						out var newViewSize, 2, 0, (int)MemProtection.ExecuteRead);

					if (result == NtStatus.Success)
					{
						// Apply our patches.
						ApplyPatches(true);

						nint viewBase2 = 0;
						// Create a writable view to write our patches through to preserve the original protections.
						result = NtMapViewOfSection(newViewHandle, _processHandle, ref viewBase2, 0, (uint)viewSize, out var viewOffset2,
											   out var newViewSize2, 2, 0, (int)MemProtection.ReadWrite);
						if (result == NtStatus.Success)
						{
							// Write our patched data trough the writable view to the memory.
							if (WriteProcessMemory(_processHandle, viewBase2, Data, viewSize, out var dummy))
							{
								// Unmap them writeable view, it's not longer needed.
								NtUnmapViewOfSection(_processHandle, viewBase2);

								// Check if the allocation protections is the right one.
								if (VirtualQueryEx(_processHandle, BaseAddress, out MemoryBasicInformation mbi, MemoryBasicInformation.Size) != 0
									&& mbi.AllocationProtect == MemProtection.ExecuteRead)
								{
									// Also check if we can change the page protection.
									if (!VirtualProtectEx(_processHandle, BaseAddress, 0x4000, (uint)MemProtection.ReadWrite, out var oldProtect))
										NtResumeProcess(_processHandle);

									return true;
								}
							}
						}
					}

					Console.WriteLine("Error while mapping the view with the given protection.");
				}
			}
			finally
			{
				NtClose(newViewHandle);
			}
		}
		else
			Console.WriteLine("Error while creating the view backup.");

		NtResumeProcess(_processHandle);

		return false;
	}

	void ApplyPatches(bool remap)
	{
		foreach( var p in _patchList )
		{
			var address = p.Value.Address;
			if (address == 0)
				continue;
			var patch = p.Value.Data;

			// 这个判断是啥呢？
			// We are in a different section here.
			if(address > Data.Length)
			{
				if (address < BaseAddress)
					address += BaseAddress;

				Write(address, patch);
				continue;
			}

			// 这个到底是啥玩意？
			if(remap)
			{
				for(var i = 0; i< patch.Length; i++)
					Data[address + i] = patch[i];
			}
		}
	}

	public bool RemapAndPatch(bool remap)
	{
		if(!remap)
		{
			ApplyPatches(false);
			return true;
		}

		// 这个到底是啥？

		if (VirtualQueryEx(_processHandle, BaseAddress, out var mbi, MemoryBasicInformation.Size) != 0)
			return RemapAndPatch(mbi.BaseAddress, (int)mbi.RegionSize);
	

		return false;
	}

	nint ReadImageBaseFromPEB(nint processHandle)
	{
		try
		{
			if (NtQueryInformationProcess(processHandle, 0, ref _peb, ProcessBasicInformation.Size, out _) == NtStatus.Success)
				return Read(_peb.PebBaseAddress + 0x10);	// 这个到底是啥，地址加16个字节是啥意思
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
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
