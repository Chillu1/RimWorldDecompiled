using System;
using System.IO;
using Verse;

namespace RimWorld
{
	public static class DevModePermanentlyDisabledUtility
	{
		private static bool initialized;

		private static bool disabled;

		public static bool Disabled
		{
			get
			{
				if (!initialized)
				{
					initialized = true;
					disabled = File.Exists(GenFilePaths.DevModePermanentlyDisabledFilePath);
				}
				return disabled;
			}
		}

		public static void Disable()
		{
			try
			{
				File.Create(GenFilePaths.DevModePermanentlyDisabledFilePath).Dispose();
			}
			catch (Exception arg)
			{
				Log.Error("Could not permanently disable dev mode: " + arg);
				return;
			}
			disabled = true;
			Prefs.DevMode = false;
		}
	}
}
