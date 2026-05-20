using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse.AI;

public class MentalBreakWorker_WildDecree : MentalBreakWorker
{
	public override bool BreakCanOccur(Pawn pawn)
	{
		if (base.BreakCanOccur(pawn) && pawn.IsColonist && !pawn.IsPrisoner && pawn.royalty != null)
		{
			return pawn.royalty.PossibleDecreeQuests.Any();
		}
		return false;
	}

	public override float CommonalityFor(Pawn pawn, bool moodCaused = false)
	{
		if (pawn.royalty == null)
		{
			return 0f;
		}
		float num = 0f;
		List<RoyalTitle> allTitlesInEffectForReading = pawn.royalty.AllTitlesInEffectForReading;
		for (int i = 0; i < allTitlesInEffectForReading.Count; i++)
		{
			num = Mathf.Max(num, allTitlesInEffectForReading[i].def.decreeMentalBreakCommonality);
		}
		return num;
	}

	public override bool TryStart(Pawn pawn, string reason, bool causedByMood)
	{
		pawn.royalty.IssueDecree(causedByMentalBreak: true, reason);
		if (MentalStateDefOf.Wander_OwnRoom.Worker.StateCanOccur(pawn))
		{
			pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_OwnRoom, null, forced: false, forceWake: false, causedByMood, null, transitionSilently: true);
		}
		else if (MentalStateDefOf.Wander_Sad.Worker.StateCanOccur(pawn))
		{
			pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Sad, null, forced: false, forceWake: false, causedByMood, null, transitionSilently: true);
		}
		return true;
	}
}
