using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_ColonyXenotypeMakeup : ThoughtWorker_Precept
{
	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive || p.Faction == null)
		{
			return ThoughtState.Inactive;
		}
		if (!p.Ideo.PreferredXenotypes.Any() && !p.Ideo.PreferredCustomXenotypes.Any())
		{
			return ThoughtState.Inactive;
		}
		List<Pawn> list = p.MapHeld.mapPawns.SpawnedPawnsInFaction(p.Faction);
		int num = 0;
		int num2 = 0;
		bool flag = p.IsSlave || p.IsPrisoner;
		foreach (Pawn item in list)
		{
			bool flag2 = item.IsSlave || item.IsPrisoner;
			if (item.genes != null && flag == flag2)
			{
				num++;
				if (!p.Ideo.IsPreferredXenotype(item))
				{
					num2++;
				}
			}
		}
		if (num2 == 0)
		{
			return ThoughtState.ActiveAtStage(0);
		}
		float num3 = (float)num2 / (float)num;
		if (num3 < 0.33f)
		{
			return ThoughtState.ActiveAtStage(1);
		}
		if (num3 < 0.66f)
		{
			return ThoughtState.ActiveAtStage(2);
		}
		return ThoughtState.ActiveAtStage(3);
	}
}
