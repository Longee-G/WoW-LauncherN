using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoW.Launcher.Patterns;

static class Common
{
    // 从AshamaneCore的Connection_patcher移植过来的...

    public static short[] Portal = ".actual.battle.net\0".ToPattern();
    public static short[] Modulus = [0x91, 0xD5, 0x9B, 0xB7, 0xD4, 0xE1, 0x83, 0xA5];
    public static short[] BinaryVersion = [0x3C, 0x56, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x3E];
    public static string VersionsFile = "%s.patch.battle.net:1119/%s/versions";
    public static string CertBundleUrl = "http://nydus-qa.web.blizzard.net/Bnet/zxx/client/bgs-key-fingerprint";
    public static short[] CertSignatureModules = [0x85, 0xF3, 0x7B, 0x14, 0x5A, 0x9C, 0x48, 0xF6];
}
