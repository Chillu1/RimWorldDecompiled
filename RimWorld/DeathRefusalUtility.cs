using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class DeathRefusalUtility
{
	public static bool PlayerHasCorpseWithDeathRefusal()
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			List<Thing> list = maps[i].listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] is Corpse corpse && HasPlayerControlledDeathRefusal(corpse.InnerPawn))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HasPlayerControlledDeathRefusal(Pawn pawn)
	{
		return pawn.health.hediffSet.GetFirstHediff<Hediff_DeathRefusal>()?.PlayerControlled ?? false;
	}
}
