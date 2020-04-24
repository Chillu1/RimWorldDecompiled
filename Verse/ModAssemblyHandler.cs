using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Verse
{
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
				ResolveEventHandler @object = (object obj, ResolveEventArgs args) => Assembly.GetExecutingAssembly();
				AppDomain.CurrentDomain.AssemblyResolve += @object.Invoke;
				globalResolverIsSet = true;
			}
			foreach (FileInfo item in from f in ModContentPack.GetAllFilesForModPreserveOrder(mod, "Assemblies/", (string e) => e.ToLower() == ".dll")
				select f.Item2)
			{
				Assembly assembly = null;
				try
				{
					byte[] rawAssembly = File.ReadAllBytes(item.FullName);
					FileInfo fileInfo = new FileInfo(Path.Combine(item.DirectoryName, Path.GetFileNameWithoutExtension(item.FullName)) + ".pdb");
					if (fileInfo.Exists)
					{
						byte[] rawSymbolStore = File.ReadAllBytes(fileInfo.FullName);
						assembly = AppDomain.CurrentDomain.Load(rawAssembly, rawSymbolStore);
					}
					else
					{
						assembly = AppDomain.CurrentDomain.Load(rawAssembly);
					}
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading " + item.Name + ": " + ex.ToString());
					return;
				}
				if (!(assembly == null) && AssemblyIsUsable(assembly))
				{
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
				stringBuilder.AppendLine("ReflectionTypeLoadException getting types in assembly " + asm.GetName().Name + ": " + ex);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Loader exceptions:");
				if (ex.LoaderExceptions != null)
				{
					Exception[] loaderExceptions = ex.LoaderExceptions;
					foreach (Exception ex2 in loaderExceptions)
					{
						stringBuilder.AppendLine("   => " + ex2.ToString());
					}
				}
				Log.Error(stringBuilder.ToString());
				return false;
			}
			catch (Exception ex3)
			{
				Log.Error("Exception getting types in assembly " + asm.GetName().Name + ": " + ex3);
				return false;
			}
			return true;
		}
	}
}
