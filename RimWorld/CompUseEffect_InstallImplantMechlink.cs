using Verse;

namespace RimWorld;

public class CompUseEffect_InstallImplantMechlink : CompUseEffect_InstallImplant
{
	public override AcceptanceReport CanBeUsedBy(Pawn p)
	{
		if (!ModLister.CheckBiotech("install implant mechlink"))
		{
			return false;
		}
		return base.CanBeUsedBy(p);
	}

	public override TaggedString ConfirmMessage(Pawn p)
	{
		if (p.WorkTypeIsDisabled(WorkTypeDefOf.Smithing))
		{
			return "ConfirmInstallMechlink_Smithing".Translate();
		}
		if (p.WorkTagIsDisabled(WorkTags.Intellectual))
		{
			return "ConfirmInstallMechlink_Intellectual".Translate();
		}
		return null;
	}
}
