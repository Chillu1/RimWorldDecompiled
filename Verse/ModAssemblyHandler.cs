using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Verse;

public class ModAssemblyHandler
{
	private ModContentPack mod;

	public List<Assembly> loadedAssemblies = new List<Assembly>();

	private static bool globalResolverIsSet;

	public ModAssemblyHandler(ModContentPack mod)
	{
		this.mod = mod;
	}

	public void ReloadAll()
	{
		if (!globalResolverIsSet)
		{
			ResolveEventHandler resolveEventHandler = (object obj, ResolveEventArgs args) => Assembly.GetExecutingAssembly();
			AppDomain.CurrentDomain.AssemblyResolve += resolveEventHandler.Invoke;
			globalResolverIsSet = true;
		}
		foreach (FileInfo item in from f in ModContentPack.GetAllFilesForModPreserveOrder(mod, "Assemblies/", (string e) => e.ToLower() == ".dll")
			select f.Item2)
		{
			Assembly assembly;
			try
			{
				assembly = Assembly.LoadFrom(item.FullName);
			}
			catch (Exception arg)
			{
				Log.Error($"Exception loading {item.Name}: {arg}");
				break;
			}
			if (AssemblyIsUsable(assembly))
			{
				GenTypes.ClearCache();
				loadedAssemblies.Add(assembly);
			}
		}
	}

	private bool AssemblyIsUsable(Assembly asm)
	{
		try
		{
			asm.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"ReflectionTypeLoadException getting types in assembly {asm.GetName().Name}: {ex}");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Loader exceptions:");
			if (ex.LoaderExceptions != null)
			{
				Exception[] loaderExceptions = ex.LoaderExceptions;
				foreach (Exception arg in loaderExceptions)
				{
					stringBuilder.AppendLine($"   => {arg}");
				}
			}
			Log.Error(stringBuilder.ToString());
			return false;
		}
		catch (Exception ex2)
		{
			Log.Error("Exception getting types in assembly " + asm.GetName().Name + ": " + ex2);
			return false;
		}
		return true;
	}
}
