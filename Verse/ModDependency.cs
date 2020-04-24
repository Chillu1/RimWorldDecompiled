using RimWorld;
using UnityEngine;

namespace Verse
{
	public class ModDependency : ModRequirement
	{
		public string downloadUrl;

		public string steamWorkshopUrl;

		public override string RequirementTypeLabel => "ModDependsOn".Translate("");

		public override bool IsSatisfied => ModLister.GetActiveModWithIdentifier(packageId) != null;

		public override Texture2D StatusIcon
		{
			get
			{
				ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(packageId, ignorePostfix: true);
				if (modWithIdentifier == null)
				{
					return ModRequirement.NotInstalled;
				}
				if (!modWithIdentifier.Active)
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
				ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(packageId, ignorePostfix: true);
				if (modWithIdentifier == null)
				{
					return base.Tooltip + "\n" + "ContentNotInstalled".Translate() + "\n\n" + "ModClickToGoToWebsite".Translate();
				}
				if (!modWithIdentifier.Active)
				{
					return base.Tooltip + "\n" + "ContentInstalledButNotActive".Translate() + "\n\n" + "ModClickToSelect".Translate();
				}
				return base.Tooltip;
			}
		}

		public string Url => steamWorkshopUrl ?? downloadUrl;

		public override void OnClicked(Page_ModsConfig window)
		{
			ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(packageId, ignorePostfix: true);
			if (modWithIdentifier == null)
			{
				if (!Url.NullOrEmpty())
				{
					SteamUtility.OpenUrl(Url);
				}
			}
			else if (!modWithIdentifier.Active)
			{
				window.SelectMod(modWithIdentifier);
			}
		}
	}
}
