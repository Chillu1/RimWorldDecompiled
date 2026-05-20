using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ExpansionDef : Def
{
	[NoTranslate]
	public string iconPath;

	[NoTranslate]
	public string notOwnedIconPath;

	[NoTranslate]
	public string backgroundPath;

	[NoTranslate]
	public string linkedMod;

	[NoTranslate]
	public string steamUrl;

	[NoTranslate]
	public string siteUrl;

	[NoTranslate]
	public string previewImagesFolderPath;

	public bool isCore;

	public Color primaryColor = Color.white;

	private Texture2D cachedIcon;

	private Texture2D cachedNotOwnedIcon;

	private Texture2D cachedBG;

	private List<Texture2D> cachedPreviewImages;

	public Texture2D IconFromStatus
	{
		get
		{
			if (Status != ExpansionStatus.NotInstalled)
			{
				return Icon;
			}
			return NotOwnedIcon;
		}
	}

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

	private Texture2D NotOwnedIcon
	{
		get
		{
			if (notOwnedIconPath.NullOrEmpty())
			{
				return Icon;
			}
			if (cachedNotOwnedIcon == null)
			{
				cachedNotOwnedIcon = ContentFinder<Texture2D>.Get(notOwnedIconPath);
			}
			return cachedNotOwnedIcon;
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

	public List<Texture2D> PreviewImages
	{
		get
		{
			if (cachedPreviewImages.NullOrEmpty())
			{
				if (previewImagesFolderPath.NullOrEmpty())
				{
					return null;
				}
				cachedPreviewImages = new List<Texture2D>(ContentFinder<Texture2D>.GetAllInFolder(previewImagesFolderPath));
			}
			return cachedPreviewImages;
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

	public string StatusDescription => Status switch
	{
		ExpansionStatus.Active => "ContentActive".Translate(), 
		ExpansionStatus.Installed => "ContentInstalledButNotActive".Translate(), 
		_ => "ContentNotInstalled".Translate(), 
	};

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
