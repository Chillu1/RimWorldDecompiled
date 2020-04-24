using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class GenClamor
	{
		public static void DoClamor(Thing source, float radius, ClamorDef type)
		{
			DoClamor(source, source.Position, radius, type);
		}

		public static void DoClamor(Thing source, IntVec3 position, float radius, ClamorDef type)
		{
			if (source.MapHeld != null)
			{
				Region region = position.GetRegion(source.MapHeld);
				if (region != null)
				{
					RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.door == null || r.door.Open, delegate(Region r)
					{
						List<Thing> list = r.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
						for (int i = 0; i < list.Count; i++)
						{
							Pawn pawn = list[i] as Pawn;
							float num = Mathf.Clamp01(pawn.health.capacities.GetLevel(PawnCapacityDefOf.Hearing));
							if (num > 0f && pawn.Position.InHorDistOf(position, radius * num))
							{
								pawn.HearClamor(source, type);
							}
						}
						return false;
					}, 15);
				}
			}
		}
	}
}
