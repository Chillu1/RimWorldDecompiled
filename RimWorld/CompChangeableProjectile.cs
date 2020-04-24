using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompChangeableProjectile : ThingComp, IStoreSettingsParent
	{
		private ThingDef loadedShell;

		public int loadedCount;

		public StorageSettings allowedShellsSettings;

		public CompProperties_ChangeableProjectile Props => (CompProperties_ChangeableProjectile)props;

		public ThingDef LoadedShell
		{
			get
			{
				if (loadedCount <= 0)
				{
					return null;
				}
				return loadedShell;
			}
		}

		public ThingDef Projectile
		{
			get
			{
				if (!Loaded)
				{
					return null;
				}
				return LoadedShell.projectileWhenLoaded;
			}
		}

		public bool Loaded => LoadedShell != null;

		public bool StorageTabVisible => true;

		public override void PostExposeData()
		{
			Scribe_Defs.Look(ref loadedShell, "loadedShell");
			Scribe_Values.Look(ref loadedCount, "loadedCount", 0);
			Scribe_Deep.Look(ref allowedShellsSettings, "allowedShellsSettings");
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			allowedShellsSettings = new StorageSettings(this);
			if (parent.def.building.defaultStorageSettings != null)
			{
				allowedShellsSettings.CopyFrom(parent.def.building.defaultStorageSettings);
			}
		}

		public virtual void Notify_ProjectileLaunched()
		{
			if (loadedCount > 0)
			{
				loadedCount--;
			}
			if (loadedCount <= 0)
			{
				loadedShell = null;
			}
		}

		public void LoadShell(ThingDef shell, int count)
		{
			loadedCount = Mathf.Max(count, 0);
			loadedShell = ((count > 0) ? shell : null);
		}

		public Thing RemoveShell()
		{
			Thing thing = ThingMaker.MakeThing(loadedShell);
			thing.stackCount = loadedCount;
			loadedCount = 0;
			loadedShell = null;
			return thing;
		}

		public StorageSettings GetStoreSettings()
		{
			return allowedShellsSettings;
		}

		public StorageSettings GetParentStoreSettings()
		{
			return parent.def.building.fixedStorageSettings;
		}
	}
}
