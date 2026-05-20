using Verse;

namespace RimWorld;

public class PreceptComp_Apparel : PreceptComp
{
	private IdeoApparelGender gender;

	public Gender AffectedGender(Ideo ideo)
	{
		switch (gender)
		{
		case IdeoApparelGender.Any:
			return Gender.None;
		case IdeoApparelGender.SupremeGender:
			return ideo.SupremeGender;
		case IdeoApparelGender.SubordinateGender:
			return ideo.SupremeGender.Opposite();
		default:
			Log.Error("Unimplemented gender: " + gender);
			return Gender.None;
		}
	}

	public virtual bool CanApplyToApparel(ThingDef apparelDef)
	{
		return true;
	}

	protected bool AppliesToPawn(Pawn pawn, Precept precept)
	{
		if (!ModsConfig.IdeologyActive || pawn.Ideo == null || !(precept is Precept_Apparel { TargetGender: var targetGender } precept_Apparel))
		{
			return false;
		}
		if (targetGender != Gender.None && pawn.gender != targetGender)
		{
			return false;
		}
		if (pawn.royalty != null)
		{
			foreach (RoyalTitle item in pawn.royalty.AllTitlesForReading)
			{
				if (item.def.requiredApparel.NullOrEmpty())
				{
					continue;
				}
				foreach (ApparelRequirement item2 in item.def.requiredApparel)
				{
					if (ApparelUtility.IsRequirementActive(item2, ApparelRequirementSource.Title, pawn, out var _))
					{
						return false;
					}
				}
			}
		}
		foreach (Apparel item3 in pawn.apparel.WornApparel)
		{
			if (item3.def == precept_Apparel.apparelDef)
			{
				return false;
			}
		}
		return true;
	}

	protected void GiveApparelToPawn(Pawn pawn, Precept_Apparel precept)
	{
		Apparel apparel = PawnApparelGenerator.GenerateApparelOfDefFor(pawn, precept.apparelDef);
		if (apparel != null && apparel.PawnCanWear(pawn))
		{
			PawnApparelGenerator.PostProcessApparel(apparel, pawn);
			PawnGenerator.PostProcessGeneratedGear(apparel, pawn);
			if (!pawn.kindDef.ignoreIdeoApparelColors)
			{
				apparel.SetColor(pawn.Ideo.ApparelColor, reportFailure: false);
			}
			pawn.apparel.Wear(apparel, dropReplacedApparel: false);
		}
	}
}
