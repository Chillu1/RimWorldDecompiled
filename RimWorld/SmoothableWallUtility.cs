using Verse;

namespace RimWorld
{
	public static class SmoothableWallUtility
	{
		public static void Notify_SmoothedByPawn(Thing t, Pawn p)
		{
			for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
			{
				IntVec3 c = t.Position + GenAdj.CardinalDirections[i];
				if (!c.InBounds(t.Map))
				{
					continue;
				}
				Building edifice = c.GetEdifice(t.Map);
				if (edifice == null || !edifice.def.IsSmoothable)
				{
					continue;
				}
				bool flag = true;
				int num = 0;
				for (int j = 0; j < GenAdj.CardinalDirections.Length; j++)
				{
					IntVec3 intVec = edifice.Position + GenAdj.CardinalDirections[j];
					if (!IsBlocked(intVec, t.Map))
					{
						flag = false;
						break;
					}
					Building edifice2 = intVec.GetEdifice(t.Map);
					if (edifice2 != null && edifice2.def.IsSmoothed)
					{
						num++;
					}
				}
				if (!flag || num < 2)
				{
					continue;
				}
				for (int k = 0; k < GenAdj.DiagonalDirections.Length; k++)
				{
					if (!IsBlocked(edifice.Position + GenAdj.DiagonalDirections[k], t.Map))
					{
						SmoothWall(edifice, p);
						break;
					}
				}
			}
		}

		public static void Notify_BuildingDestroying(Thing t, DestroyMode mode)
		{
			if ((mode != DestroyMode.KillFinalize && mode != DestroyMode.Deconstruct) || !t.def.IsSmoothed)
			{
				return;
			}
			for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
			{
				IntVec3 c = t.Position + GenAdj.CardinalDirections[i];
				if (!c.InBounds(t.Map))
				{
					continue;
				}
				Building edifice = c.GetEdifice(t.Map);
				if (edifice == null || !edifice.def.IsSmoothed)
				{
					continue;
				}
				bool flag = true;
				for (int j = 0; j < GenAdj.CardinalDirections.Length; j++)
				{
					if (!IsBlocked(edifice.Position + GenAdj.CardinalDirections[j], t.Map))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					edifice.Destroy(DestroyMode.WillReplace);
					GenSpawn.Spawn(ThingMaker.MakeThing(edifice.def.building.unsmoothedThing, edifice.Stuff), edifice.Position, t.Map, edifice.Rotation);
				}
			}
		}

		public static Thing SmoothWall(Thing target, Pawn smoother)
		{
			Map map = target.Map;
			target.Destroy(DestroyMode.WillReplace);
			Thing thing = ThingMaker.MakeThing(target.def.building.smoothedThing, target.Stuff);
			thing.SetFaction(smoother.Faction);
			GenSpawn.Spawn(thing, target.Position, map, target.Rotation);
			map.designationManager.TryRemoveDesignation(target.Position, DesignationDefOf.SmoothWall);
			return thing;
		}

		private static bool IsBlocked(IntVec3 pos, Map map)
		{
			if (!pos.InBounds(map))
			{
				return false;
			}
			if (pos.Walkable(map))
			{
				return false;
			}
			Building edifice = pos.GetEdifice(map);
			if (edifice == null)
			{
				return false;
			}
			if (!edifice.def.IsSmoothed)
			{
				return edifice.def.building.isNaturalRock;
			}
			return true;
		}
	}
}
