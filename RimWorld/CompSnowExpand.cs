using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld
{
	public class CompSnowExpand : ThingComp
	{
		private float snowRadius;

		private ModuleBase snowNoise;

		private const float MaxOutdoorTemp = 10f;

		private static HashSet<IntVec3> reachableCells = new HashSet<IntVec3>();

		public CompProperties_SnowExpand Props => (CompProperties_SnowExpand)props;

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref snowRadius, "snowRadius", 0f);
		}

		public override void CompTick()
		{
			if (parent.Spawned && parent.IsHashIntervalTick(Props.expandInterval))
			{
				TryExpandSnow();
			}
		}

		private void TryExpandSnow()
		{
			if (parent.Map.mapTemperature.OutdoorTemp > 10f)
			{
				snowRadius = 0f;
				return;
			}
			if (snowNoise == null)
			{
				snowNoise = new Perlin(0.054999999701976776, 2.0, 0.5, 5, Rand.Range(0, 651431), QualityMode.Medium);
			}
			if (snowRadius < 8f)
			{
				snowRadius += 1.3f;
			}
			else if (snowRadius < 17f)
			{
				snowRadius += 0.7f;
			}
			else if (snowRadius < 30f)
			{
				snowRadius += 0.4f;
			}
			else
			{
				snowRadius += 0.1f;
			}
			snowRadius = Mathf.Min(snowRadius, Props.maxRadius);
			CellRect occupiedRect = parent.OccupiedRect();
			reachableCells.Clear();
			parent.Map.floodFiller.FloodFill(parent.Position, delegate(IntVec3 x)
			{
				if ((float)x.DistanceToSquared(parent.Position) > snowRadius * snowRadius)
				{
					return false;
				}
				return occupiedRect.Contains(x) || !x.Filled(parent.Map);
			}, delegate(IntVec3 x)
			{
				reachableCells.Add(x);
			});
			int num = GenRadial.NumCellsInRadius(snowRadius);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = parent.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(parent.Map) && reachableCells.Contains(intVec))
				{
					float value = snowNoise.GetValue(intVec);
					value += 1f;
					value *= 0.5f;
					if (value < 0.1f)
					{
						value = 0.1f;
					}
					if (!(parent.Map.snowGrid.GetDepth(intVec) > value))
					{
						float lengthHorizontal = (intVec - parent.Position).LengthHorizontal;
						float num2 = 1f - lengthHorizontal / snowRadius;
						parent.Map.snowGrid.AddDepth(intVec, num2 * Props.addAmount * value);
					}
				}
			}
		}
	}
}
