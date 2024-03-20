// See https://aka.ms/new-console-template for more information


using System.CommandLine.Parsing;
using WoW.Launcher.Misc;

//Console.WriteLine("Hello, World!");
Helpers.PrintHeader("AshamaneCore");

string str = "abcdefg";
string subStr = str[^3..^0];    // 从倒数第3个字符到倒数第1个字符，共3个字符

Console.WriteLine(subStr);


Console.WriteLine("1234567890"[0..5]);
Console.WriteLine("1234567890"[..5]);	// 起始下标可以省略 ...

Console.WriteLine("1234567890"[..^2]);
Console.WriteLine("1234567890"[^2..]);
