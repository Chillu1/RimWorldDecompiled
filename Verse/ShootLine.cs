using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
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

		public void ChangeDestToMissWild(float aimOnChance)
		{
			float num = ShootTuning.MissDistanceFromAimOnChanceCurves.Evaluate(aimOnChance, Rand.Value);
			if (num < 0f)
			{
				Log.ErrorOnce("Attempted to wild-miss less than zero tiles away", 94302089);
			}
			IntVec3 a;
			do
			{
				Vector2 unitVector = Rand.UnitVector2;
				Vector3 b = new Vector3(unitVector.x * num, 0f, unitVector.y * num);
				a = (dest.ToVector3Shifted() + b).ToIntVec3();
			}
			while (Vector3.Dot((dest - source).ToVector3(), (a - source).ToVector3()) < 0f);
			dest = a;
		}

		public IEnumerable<IntVec3> Points()
		{
			return GenSight.PointsOnLineOfSight(source, dest);
		}

		public override string ToString()
		{
			return "(" + source + "->" + dest + ")";
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
					shootLine2.ChangeDestToMissWild((float)item / 100f);
					if (shootLine2.dest.z == 0 && shootLine2.dest.x > intVec.x)
					{
						results[item, shootLine2.dest.x - intVec.x]++;
					}
				}
			}
			DebugTables.MakeTablesDialog(colValues, (int cells) => cells.ToString() + "-away\ncell\nhit%", enumerable, (int hitchance) => ((float)hitchance / 100f).ToStringPercent() + " aimon chance", delegate(int cells, int hitchance)
			{
				float num = (float)hitchance / 100f;
				return (cells == 0) ? num.ToStringPercent() : ((float)results[hitchance, cells] / 10000f * (1f - num)).ToStringPercent();
			});
		}
	}
}
