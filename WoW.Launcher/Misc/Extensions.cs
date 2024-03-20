using System;
namespace WoW.Launcher.Misc;


static class Extensions
{
	public static nint ToNint(this byte[] buffer) => (nint) BitConverter.ToInt64(buffer, 0);
	public static nint ToNint(this long value) => (nint)value;

	public static byte[] GetCopy(this byte[] data)
	{
		var copy = new byte[data.Length];
		Array.Copy(data, copy, data.Length); 
		return copy;
	}


}
