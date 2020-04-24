using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ExpansionDef : Def
	{
		[NoTranslate]
		public string iconPath;

		[NoTranslate]
		public string backgroundPath;

		[NoTranslate]
		public string linkedMod;

		[NoTranslate]
		public string steamUrl;

		[NoTranslate]
		public string siteUrl;

		public bool isCore;

		private Texture2D cachedIcon;

		private Texture2D cachedBG;

		public Texture2D Icon
		{
			get
			{
				if (cachedIcon == null)
				{
					cachedIcon = ContentFinder<Texture2D>.Get(iconPath);
				}
				return cachedIcon;
			}
		}

		public Texture2D BackgroundImage
		{
			get
			{
				if (cachedBG == null)
				{
					cachedBG = ContentFinder<Texture2D>.Get(backgroundPath);
				}
				return cachedBG;
			}
		}

		public string StoreURL
		{
			get
			{
				if (!steamUrl.NullOrEmpty())
				{
					return steamUrl;
				}
				return siteUrl;
			}
		}

		public ExpansionStatus Status
		{
			get
			{
				if (ModsConfig.IsActive(linkedMod))
				{
					return ExpansionStatus.Active;
				}
				if (ModLister.AllInstalledMods.Any((ModMetaData m) => m.SamePackageId(linkedMod)))
				{
					return ExpansionStatus.Installed;
				}
				return ExpansionStatus.NotInstalled;
			}
		}

		public string StatusDescription
		{
			get
			{
				switch (Status)
				{
				case ExpansionStatus.Active:
					return "ContentActive".Translate();
				case ExpansionStatus.Installed:
					return "ContentInstalledButNotActive".Translate();
				default:
					return "ContentNotInstalled".Translate();
				}
			}
		}

		public override void PostLoad()
		{
			base.PostLoad();
			linkedMod = linkedMod.ToLower();
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(linkedMod);
			if (modWithIdentifier != null && !modWithIdentifier.Official)
			{
				yield return modWithIdentifier.Name + " - ExpansionDefs are used for official content. For mods, you should define ModMetaData in About.xml.";
			}
		}
	}
}
