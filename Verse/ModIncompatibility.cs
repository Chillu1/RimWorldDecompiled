using RimWorld;

namespace Verse
{
	public class ModIncompatibility : ModRequirement
	{
		public override string RequirementTypeLabel => "ModIncompatibleWith".Translate("");

		public override bool IsSatisfied => ModLister.GetActiveModWithIdentifier(packageId) == null;

		public override string Tooltip
		{
			get
			{
				ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(packageId, ignorePostfix: true);
				if (modWithIdentifier != null && modWithIdentifier.Active)
				{
					return base.Tooltip + "\n" + "ContentActive".Translate() + "\n\n" + "ModClickToSelect".Translate();
				}
				return base.Tooltip;
			}
		}

		public override void OnClicked(Page_ModsConfig window)
		{
			ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(packageId, ignorePostfix: true);
			if (modWithIdentifier != null && modWithIdentifier.Active)
			{
				window.SelectMod(modWithIdentifier);
			}
		}
	}
}
