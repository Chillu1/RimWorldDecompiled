using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class GenClamor
{
	public delegate void ClamorEffect(Thing source, Pawn hearer);

	public static void DoClamor(Thing source, float radius, ClamorDef type)
	{
		DoClamor(source, source.Position, radius, type);
	}

	public static void DoClamor(Thing source, IntVec3 position, float radius, ClamorDef type)
	{
		DoClamor(source, position, radius, delegate(Thing _, Pawn hearer)
		{
			hearer.HearClamor(source, type);
		});
	}

	public static void DoClamor(Thing source, float radius, ClamorEffect clamorEffect)
	{
		DoClamor(source, source.Position, radius, clamorEffect);
	}

	public static void DoClamor(Thing source, IntVec3 position, float radius, ClamorEffect clamorEffect)
	{
		if (source.MapHeld == null)
		{
			return;
		}
		Region region = position.GetRegion(source.MapHeld);
		if (region == null)
		{
			return;
		}
		RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.door == null || r.door.Open, delegate(Region r)
		{
			List<Thing> list = r.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i] as Pawn;
				float num = Mathf.Clamp01(pawn.health.capacities.GetLevel(PawnCapacityDefOf.Hearing));
				if (num > 0f && pawn.Position.InHorDistOf(position, radius * num))
				{
					clamorEffect(source, pawn);
				}
			}
			return false;
		}, 15);
	}
}
