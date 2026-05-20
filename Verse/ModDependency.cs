using RimWorld;
using UnityEngine;

namespace Verse;

public class ModDependency : ModRequirement
{
	public string downloadUrl;

	public string steamWorkshopUrl;

	public override string RequirementTypeLabel => "ModDependsOn".Translate("");

	public override bool IsSatisfied
	{
		get
		{
			if (ModLister.GetActiveModWithIdentifier(packageId, ignorePostfix: true) == null)
			{
				return alternativePackageIds?.Any((string id) => ModLister.GetActiveModWithIdentifier(id, ignorePostfix: true) != null) ?? false;
			}
			return true;
		}
	}

	private ModMetaData Mod
	{
		get
		{
			ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(packageId, ignorePostfix: true);
			if (modWithIdentifier != null)
			{
				return modWithIdentifier;
			}
			if (alternativePackageIds.NullOrEmpty())
			{
				return null;
			}
			foreach (string alternativePackageId in alternativePackageIds)
			{
				modWithIdentifier = ModLister.GetModWithIdentifier(alternativePackageId, ignorePostfix: true);
				if (modWithIdentifier != null)
				{
					return modWithIdentifier;
				}
			}
			return null;
		}
	}

	public override Texture2D StatusIcon
	{
		get
		{
			if (Mod == null)
			{
				return ModRequirement.NotInstalled;
			}
			if (!Mod.Active)
			{
				return ModRequirement.Installed;
			}
			return ModRequirement.Resolved;
		}
	}

	public override string Tooltip
	{
		get
		{
			if (Mod == null)
			{
				return base.Tooltip + "\n" + "ContentNotInstalled".Translate() + "\n\n" + "ModClickToGoToWebsite".Translate();
			}
			if (!Mod.Active)
			{
				return base.Tooltip + "\n" + "ContentInstalledButNotActive".Translate() + "\n\n" + "ModClickToSelect".Translate();
			}
			return base.Tooltip;
		}
	}

	public string Url => steamWorkshopUrl ?? downloadUrl;

	public override void OnClicked(Page_ModsConfig window)
	{
		if (Mod == null)
		{
			if (!Url.NullOrEmpty())
			{
				SteamUtility.OpenUrl(Url);
			}
		}
		else if (!Mod.Active)
		{
			window.SelectMod(Mod);
		}
	}
}
