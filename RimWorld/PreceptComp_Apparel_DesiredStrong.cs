using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PreceptComp_Apparel_DesiredStrong : PreceptComp_Apparel
{
	public override void Notify_MemberGenerated(Pawn pawn, Precept precept, bool newborn, bool ignoreApparel = false)
	{
		if (newborn || !AppliesToPawn(pawn, precept) || ignoreApparel)
		{
			return;
		}
		Precept_Apparel precept_Apparel = (Precept_Apparel)precept;
		foreach (Apparel item in pawn.apparel.WornApparel)
		{
			if (!ApparelUtility.CanWearTogether(item.def, precept_Apparel.apparelDef, pawn.RaceProps.body))
			{
				List<ThingDef> apparelRequired = pawn.kindDef.apparelRequired;
				if (apparelRequired != null && apparelRequired.Contains(item.def))
				{
					return;
				}
			}
		}
		GiveApparelToPawn(pawn, precept_Apparel);
	}
}
