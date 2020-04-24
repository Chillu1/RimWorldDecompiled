using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.SketchGen
{
	public class SketchResolver_DamageBuildings : SketchResolver
	{
		private const float MaxPctOfTotalDestroyed = 0.65f;

		private const float HpRandomFactor = 1.2f;

		private const float DestroyChanceExp = 1.32f;

		protected override bool CanResolveInt(ResolveParams parms)
		{
			return true;
		}

		protected override void ResolveInt(ResolveParams parms)
		{
			CellRect occupiedRect = parms.sketch.OccupiedRect;
			Rot4 random = Rot4.Random;
			int num = 0;
			int num2 = parms.sketch.Buildables.Count();
			foreach (SketchBuildable item in parms.sketch.Buildables.InRandomOrder().ToList())
			{
				Damage(item, occupiedRect, random, parms.sketch, out bool destroyed);
				if (destroyed)
				{
					num++;
					if ((float)num > (float)num2 * 0.65f)
					{
						break;
					}
				}
			}
		}

		private void Damage(SketchBuildable buildable, CellRect rect, Rot4 dir, Sketch sketch, out bool destroyed)
		{
			float num = (!dir.IsHorizontal) ? ((float)(buildable.pos.z - rect.minZ) / (float)rect.Height) : ((float)(buildable.pos.x - rect.minX) / (float)rect.Width);
			if (dir == Rot4.East || dir == Rot4.South)
			{
				num = 1f - num;
			}
			if (Rand.Chance(Mathf.Pow(num, 1.32f)))
			{
				sketch.Remove(buildable);
				destroyed = true;
				SketchTerrain sketchTerrain = buildable as SketchTerrain;
				if (sketchTerrain != null && sketchTerrain.def.burnedDef != null)
				{
					sketch.AddTerrain(sketchTerrain.def.burnedDef, sketchTerrain.pos);
				}
			}
			else
			{
				destroyed = false;
				SketchThing sketchThing = buildable as SketchThing;
				if (sketchThing != null)
				{
					sketchThing.hitPoints = Mathf.Clamp(Mathf.RoundToInt((float)sketchThing.MaxHitPoints * (1f - num) * Rand.Range(1f, 1.2f)), 1, sketchThing.MaxHitPoints);
				}
			}
		}
	}
}
