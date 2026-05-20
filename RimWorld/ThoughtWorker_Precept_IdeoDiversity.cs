using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_IdeoDiversity : ThoughtWorker_Precept
{
	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (p.Faction == null || !p.IsColonist)
		{
			return false;
		}
		int num = 0;
		int num2 = 0;
		List<Pawn> list = p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction);
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i].IsQuestLodger() && !list[i].IsGhoul && list[i].RaceProps.Humanlike && !list[i].IsSlave && !list[i].IsPrisoner && !list[i].DevelopmentalStage.Baby() && !list[i].IsSubhuman)
			{
				num2++;
				if (list[i] != p && list[i].Ideo != p.Ideo)
				{
					num++;
				}
			}
		}
		if (num == 0)
		{
			return ThoughtState.Inactive;
		}
		return ThoughtState.ActiveAtStage(Mathf.RoundToInt((float)num / (float)(num2 - 1) * (float)(def.stages.Count - 1)));
	}
}
