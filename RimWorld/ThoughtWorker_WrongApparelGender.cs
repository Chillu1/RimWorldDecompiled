using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_WrongApparelGender : ThoughtWorker
{
	public override string PostProcessLabel(Pawn p, string label)
	{
		return label.Formatted(p.gender.Opposite().GetLabel().ToLower(), p.Named("PAWN"));
	}

	public override string PostProcessDescription(Pawn p, string description)
	{
		return description.Formatted(p.gender.Opposite().GetLabel().ToLower(), p.gender.GetLabel(), p.Named("PAWN"));
	}

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		List<Apparel> wornApparel = p.apparel.WornApparel;
		for (int i = 0; i < wornApparel.Count; i++)
		{
			if (!wornApparel[i].def.apparel.CorrectGenderForWearing(p.gender))
			{
				return ThoughtState.ActiveDefault;
			}
		}
		return false;
	}
}
