using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_SprayLiquid : CompAbilityEffect
{
	private List<Pair<IntVec3, float>> tmpCellDots = new List<Pair<IntVec3, float>>();

	private List<IntVec3> tmpCells = new List<IntVec3>();

	private new CompProperties_AbilitySprayLiquid Props => (CompProperties_AbilitySprayLiquid)props;

	private Pawn Pawn => parent.pawn;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		foreach (IntVec3 item in AffectedCells(target))
		{
			((Projectile)GenSpawn.Spawn(Props.projectileDef, Pawn.Position, Pawn.Map)).Launch(Pawn, Pawn.DrawPos, item, item, ProjectileHitFlags.IntendedTarget, parent.verb.preventFriendlyFire);
		}
		if (Props.sprayEffecter != null)
		{
			Props.sprayEffecter.Spawn(parent.pawn.Position, target.Cell, parent.pawn.Map).Cleanup();
		}
		base.Apply(target, dest);
	}

	public override void DrawEffectPreview(LocalTargetInfo target)
	{
		GenDraw.DrawFieldEdges(AffectedCells(target));
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		if (Pawn.Faction != null)
		{
			foreach (IntVec3 item in AffectedCells(target))
			{
				List<Thing> thingList = item.GetThingList(Pawn.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i].Faction == Pawn.Faction)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private List<IntVec3> AffectedCells(LocalTargetInfo target)
	{
		tmpCellDots.Clear();
		tmpCells.Clear();
		tmpCellDots.Add(new Pair<IntVec3, float>(target.Cell, 999f));
		if (Props.numCellsToHit > 1)
		{
			Vector3 vector = Pawn.Position.ToVector3Shifted().Yto0();
			Vector3 vector2 = target.Cell.ToVector3Shifted().Yto0();
			IntVec3[] array;
			int num;
			if (Props.numCellsToHit < 10)
			{
				array = GenAdj.AdjacentCells;
				num = 8;
			}
			else
			{
				array = GenRadial.RadialPattern;
				num = Props.numCellsToHit + 5;
			}
			for (int i = 0; i < num; i++)
			{
				IntVec3 first = target.Cell + array[i];
				Vector3 vector3 = first.ToVector3Shifted().Yto0();
				float second = Vector3.Dot((vector3 - vector).normalized, (vector3 - vector2).normalized);
				tmpCellDots.Add(new Pair<IntVec3, float>(first, second));
			}
			tmpCellDots.SortBy((Pair<IntVec3, float> x) => 0f - x.Second);
		}
		Map map = Pawn.Map;
		int num2 = Mathf.Min(tmpCellDots.Count, Props.numCellsToHit);
		for (int num3 = 0; num3 < num2; num3++)
		{
			IntVec3 first2 = tmpCellDots[num3].First;
			if (!first2.InBounds(map))
			{
				continue;
			}
			if (first2.Filled(map))
			{
				Building_Door door = first2.GetDoor(map);
				if (door == null || !door.Open)
				{
					continue;
				}
			}
			if (parent.verb.TryFindShootLineFromTo(Pawn.Position, first2, out var _, ignoreRange: true))
			{
				tmpCells.Add(first2);
			}
		}
		return tmpCells;
	}
}
