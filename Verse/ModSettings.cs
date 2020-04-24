namespace Verse
{
	public abstract class ModSettings : IExposable
	{
		public Mod Mod
		{
			get;
			internal set;
		}

		public virtual void ExposeData()
		{
		}

		public void Write()
		{
			LoadedModManager.WriteModSettings(Mod.Content.FolderName, Mod.GetType().Name, this);
		}
	}
}
