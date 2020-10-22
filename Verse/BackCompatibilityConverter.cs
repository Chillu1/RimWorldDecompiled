using System;
using System.Xml;
using RimWorld;

namespace Verse
{
	public abstract class BackCompatibilityConverter
	{
		public abstract bool AppliesToVersion(int majorVer, int minorVer);

		public abstract string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null);

		public abstract Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node);

		public virtual int GetBackCompatibleBodyPartIndex(BodyDef body, int index)
		{
			return index;
		}

		public abstract void PostExposeData(object obj);

		public virtual void PostCouldntLoadDef(string defName)
		{
		}

		public virtual void PreLoadSavegame(string loadingVersion)
		{
		}

		public virtual void PostLoadSavegame(string loadingVersion)
		{
		}

		public bool AppliesToLoadedGameVersion(bool allowInactiveScribe = false)
		{
			if (ScribeMetaHeaderUtility.loadedGameVersion.NullOrEmpty())
			{
				return false;
			}
			if (!allowInactiveScribe && Scribe.mode == LoadSaveMode.Inactive)
			{
				return false;
			}
			return AppliesToVersion(VersionControl.MajorFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion), VersionControl.MinorFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion));
		}
	}
}
