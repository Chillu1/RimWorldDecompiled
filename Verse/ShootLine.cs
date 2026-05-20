using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;

namespace Verse;

public struct ShootLine
{
	private IntVec3 source;

	private IntVec3 dest;

	public IntVec3 Source => source;

	public IntVec3 Dest => dest;

	public ShootLine(IntVec3 source, IntVec3 dest)
	{
		this.source = source;
		this.dest = dest;
	}

	public void ChangeDestToMissWild(float aimOnChance, bool flyOverhead, Map map)
	{
		float num = ShootTuning.MissDistanceFromAimOnChanceCurves.Evaluate(aimOnChance, Rand.Value);
		if (num < 0f)
		{
			Log.ErrorOnce("Attempted to wild-miss less than zero tiles away", 94302089);
		}
		IntVec3 intVec;
		do
		{
			Vector2 unitVector = Rand.UnitVector2;
			Vector3 vector = new Vector3(unitVector.x * num, 0f, unitVector.y * num);
			intVec = (dest.ToVector3Shifted() + vector).ToIntVec3();
		}
		while (Vector3.Dot((dest - source).ToVector3(), (intVec - source).ToVector3()) < 0f);
		dest = intVec;
		if (flyOverhead || map == null || ShootLeanUtility.CellCanSeeCell(source, dest, map))
		{
			return;
		}
		foreach (IntVec3 item in Points())
		{
			IntVec3 intVec2 = (dest = item);
			if (intVec2 != source && intVec2.InBounds(map) && intVec2.Filled(map) && !source.InHorDistOf(dest, 1.5f))
			{
				Building_Door door = intVec2.GetDoor(map);
				if (door == null || !door.Open)
				{
					break;
				}
			}
		}
	}

	public IEnumerable<IntVec3> Points()
	{
		return GenSight.PointsOnLineOfSight(source, dest);
	}

	public override string ToString()
	{
		string[] obj = new string[5] { "(", null, null, null, null };
		IntVec3 intVec = source;
		obj[1] = intVec.ToString();
		obj[2] = "->";
		intVec = dest;
		obj[3] = intVec.ToString();
		obj[4] = ")";
		return string.Concat(obj);
	}

	[DebugOutput]
	public static void WildMissResults()
	{
		IntVec3 intVec = new IntVec3(100, 0, 0);
		ShootLine shootLine = new ShootLine(IntVec3.Zero, intVec);
		IEnumerable<int> enumerable = Enumerable.Range(0, 101);
		IEnumerable<int> colValues = Enumerable.Range(0, 12);
		int[,] results = new int[enumerable.Count(), colValues.Count()];
		foreach (int item in enumerable)
		{
			for (int i = 0; i < 10000; i++)
			{
				ShootLine shootLine2 = shootLine;
				shootLine2.ChangeDestToMissWild((float)item / 100f, flyOverhead: false, null);
				if (shootLine2.dest.z == 0 && shootLine2.dest.x > intVec.x)
				{
					results[item, shootLine2.dest.x - intVec.x]++;
				}
			}
		}
		DebugTables.MakeTablesDialog(colValues, (int cells) => cells + "-away\ncell\nhit%", enumerable, (int hitchance) => ((float)hitchance / 100f).ToStringPercent() + " aimon chance", delegate(int cells, int hitchance)
		{
			float num = (float)hitchance / 100f;
			return (cells == 0) ? num.ToStringPercent() : ((float)results[hitchance, cells] / 10000f * (1f - num)).ToStringPercent();
		});
	}
}
