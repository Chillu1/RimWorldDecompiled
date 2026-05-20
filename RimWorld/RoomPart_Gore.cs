using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomPart_Gore : RoomPartWorker
{
	private static readonly FloatRange BloodGroupsPer100Cells = new FloatRange(1f, 2f);

	private static readonly IntRange BloodCountRange = new IntRange(6, 10);

	private static readonly IntRange BloodDistanceRange = new IntRange(5, 9);

	private static readonly IntRange SmearLengthRange = new IntRange(6, 10);

	private static readonly IntRange DeadTicksRange = new IntRange(180000, 36000000);

	public RoomPart_Gore(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		float num = (float)room.Area / 100f;
		int num2 = Mathf.Max(Mathf.RoundToInt(BloodGroupsPer100Cells.RandomInRange * num), 1);
		for (int i = 0; i < num2; i++)
		{
			if (!room.TryGetRandomCellInRoom(map, out var cell, 1))
			{
				break;
			}
			SpawnBloodPool(map, cell, room);
		}
		if (room.TryGetRandomCellInRoom(map, out var cell2, 3, 0, (IntVec3 c) => ValidCell(map, c, room)))
		{
			SpawnCorpseSmear(map, cell2, room, faction);
		}
	}

	private void SpawnBloodPool(Map map, IntVec3 cell, LayoutRoom room)
	{
		GenSpawn.SpawnIrregularLump(ThingDefOf.Filth_Blood, cell, map, BloodCountRange, BloodDistanceRange, WipeMode.Vanish, (IntVec3 c) => ValidCell(map, c, room));
	}

	private void SpawnCorpseSmear(Map map, IntVec3 cell, LayoutRoom room, Faction faction)
	{
		int num = SmearLengthRange.RandomInRange;
		IntVec3 intVec = cell;
		HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
		SpawnCorpse(map, cell, faction);
		while (num > 0 && ValidCell(map, intVec, room))
		{
			IntVec3 intVec2 = IntVec3.Invalid;
			int num2 = Rand.Range(0, 8);
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec3 = intVec + GenAdj.AdjacentCellsAround[GenMath.PositiveMod(i + num2, 8)];
				if (!hashSet.Contains(intVec3) && ValidCell(map, intVec3, room))
				{
					intVec2 = intVec3;
					break;
				}
			}
			if (!intVec2.IsValid)
			{
				break;
			}
			FilthMaker.TryMakeFilth(intVec, map, ThingDefOf.Filth_BloodSmear, out var outFilth, 1, FilthSourceFlags.None, shouldPropagate: false);
			FilthMaker.TryMakeFilth(intVec, map, ThingDefOf.Filth_BloodSmear, out var outFilth2, 1, FilthSourceFlags.None, shouldPropagate: false);
			if (outFilth != null)
			{
				outFilth.SetOverrideDrawPositionAndRotation(intVec.ToVector3().WithY(ThingDefOf.Filth_BloodSmear.Altitude), (intVec2 - intVec).AngleFlat);
				int num3 = Rand.Range(3, 5);
				for (int j = 0; j < num3; j++)
				{
					outFilth.ThickenFilth();
				}
			}
			if (outFilth2 != null)
			{
				Vector3 vector = (intVec2 - intVec).ToVector3();
				Vector3 v = intVec.ToVector3() + vector * 0.5f;
				outFilth2.SetOverrideDrawPositionAndRotation(v.WithY(ThingDefOf.Filth_BloodSmear.Altitude), vector.AngleFlat());
				int num4 = Rand.Range(3, 5);
				for (int k = 0; k < num4; k++)
				{
					outFilth2.ThickenFilth();
				}
			}
			hashSet.Add(intVec);
			intVec = intVec2;
			num--;
		}
	}

	private static void SpawnCorpse(Map map, IntVec3 cell, Faction faction)
	{
		PawnKindDef pawnKindDef = PawnKindDefOf.Villager;
		if (faction != null)
		{
			pawnKindDef = faction.RandomPawnKind();
		}
		PawnKindDef kind = pawnKindDef;
		int randomInRange = DeadTicksRange.RandomInRange;
		DamageDef executionCut = DamageDefOf.ExecutionCut;
		Tool toolUsed = ThingDefOf.MeleeWeapon_Knife.tools[1];
		BodyPartDef neck = BodyPartDefOf.Neck;
		RoomGenUtility.SpawnCorpse(cell, kind, randomInRange, map, executionCut, null, forceNoGear: false, null, toolUsed, null, neck);
	}

	private static bool ValidCell(Map map, IntVec3 cell, LayoutRoom room)
	{
		if (room.Contains(cell) && cell.GetEdifice(map) == null && cell.GetRoof(map) != null)
		{
			return cell.GetAffordances(map).Contains(TerrainAffordanceDefOf.Light);
		}
		return false;
	}
}
