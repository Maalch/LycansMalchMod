using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using HarmonyLib.Tools;
using Mono.Cecil;

namespace LycansHarmonyLogPatcher;

public static class HarmonyLogPathPatcher
{
	public static IEnumerable<string> TargetDLLs => new[] { "Assembly-CSharp.dll" };

	public static void Patch(AssemblyDefinition assembly)
	{
		HarmonyFileLog.FileWriterPath = Path.Combine(Environment.CurrentDirectory, "HarmonyLog." + Process.GetCurrentProcess().Id + ".txt");
	}
}