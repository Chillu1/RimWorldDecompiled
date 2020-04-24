using UnityEngine;

namespace Verse
{
	public abstract class Mod
	{
		private ModSettings modSettings;

		private ModContentPack intContent;

		public ModContentPack Content => intContent;

		public Mod(ModContentPack content)
		{
			intContent = content;
		}

		public T GetSettings<T>() where T : ModSettings, new()
		{
			if (modSettings != null && modSettings.GetType() != typeof(T))
			{
				Log.Error($"Mod {Content.Name} attempted to read two different settings classes (was {modSettings.GetType()}, is now {typeof(T)})");
				return null;
			}
			if (modSettings != null)
			{
				return (T)modSettings;
			}
			modSettings = LoadedModManager.ReadModSettings<T>(intContent.FolderName, GetType().Name);
			modSettings.Mod = this;
			return modSettings as T;
		}

		public virtual void WriteSettings()
		{
			if (modSettings != null)
			{
				modSettings.Write();
			}
		}

		public virtual void DoSettingsWindowContents(Rect inRect)
		{
		}

		public virtual string SettingsCategory()
		{
			return "";
		}
	}
}
