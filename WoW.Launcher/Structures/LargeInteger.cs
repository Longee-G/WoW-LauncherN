namespace WoW.Launcher.Structures;

// 这个是什么用法？映射一个c++的结构吗？

[StructLayout(LayoutKind.Explicit, Size = 8)]
struct LargeInteger
{
	[FieldOffset(0)]	public long Quad;
	[FieldOffset(0)]	public uint Low;
	[FieldOffset(4)]	public int High;

	public static int Size=> Marshal.SizeOf<LargeInteger>();
}
