using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompPowerPlantWater : CompPowerPlant
	{
		private float spinPosition;

		private bool cacheDirty = true;

		private bool waterUsable;

		private bool waterDoubleUsed;

		private float spinRate = 1f;

		private const float PowerFactorIfWaterDoubleUsed = 0.3f;

		private const float SpinRateFactor = 0.006666667f;

		private const float BladeOffset = 2.36f;

		private const int BladeCount = 9;

		public static readonly Material BladesMat = MaterialPool.MatFrom("Things/Building/Power/WatermillGenerator/WatermillGeneratorBlades");

		protected override float DesiredPowerOutput
		{
			get
			{
				if (cacheDirty)
				{
					RebuildCache();
				}
				if (!waterUsable)
				{
					return 0f;
				}
				if (waterDoubleUsed)
				{
					return base.DesiredPowerOutput * 0.3f;
				}
				return base.DesiredPowerOutput;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			spinPosition = Rand.Range(0f, 15f);
			RebuildCache();
			ForceOthersToRebuildCache(parent.Map);
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			ForceOthersToRebuildCache(map);
		}

		private void ClearCache()
		{
			cacheDirty = true;
		}

		private void RebuildCache()
		{
			waterUsable = true;
			foreach (IntVec3 item in WaterCells())
			{
				if (item.InBounds(parent.Map) && !parent.Map.terrainGrid.TerrainAt(item).affordances.Contains(TerrainAffordanceDefOf.MovingFluid))
				{
					waterUsable = false;
					break;
				}
			}
			waterDoubleUsed = false;
			IEnumerable<Building> enumerable = parent.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.WatermillGenerator);
			foreach (IntVec3 item2 in WaterUseCells())
			{
				if (item2.InBounds(parent.Map))
				{
					foreach (Building item3 in enumerable)
					{
						if (item3 != parent && item3.GetComp<CompPowerPlantWater>().WaterUseRect().Contains(item2))
						{
							waterDoubleUsed = true;
							break;
						}
					}
				}
			}
			if (!waterUsable)
			{
				spinRate = 0f;
				return;
			}
			Vector3 zero = Vector3.zero;
			foreach (IntVec3 item4 in WaterCells())
			{
				zero += parent.Map.waterInfo.GetWaterMovement(item4.ToVector3Shifted());
			}
			spinRate = Mathf.Sign(Vector3.Dot(zero, parent.Rotation.Rotated(RotationDirection.Clockwise).FacingCell.ToVector3()));
			spinRate *= Rand.RangeSeeded(0.9f, 1.1f, parent.thingIDNumber * 60509 + 33151);
			if (waterDoubleUsed)
			{
				spinRate *= 0.5f;
			}
			cacheDirty = false;
		}

		private void ForceOthersToRebuildCache(Map map)
		{
			foreach (Building item in map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.WatermillGenerator))
			{
				item.GetComp<CompPowerPlantWater>().ClearCache();
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if (base.PowerOutput > 0.01f)
			{
				spinPosition = (spinPosition + 0.006666667f * spinRate + (float)Math.PI * 2f) % ((float)Math.PI * 2f);
			}
		}

		public IEnumerable<IntVec3> WaterCells()
		{
			return WaterCells(parent.Position, parent.Rotation);
		}

		public static IEnumerable<IntVec3> WaterCells(IntVec3 loc, Rot4 rot)
		{
			IntVec3 perpOffset = rot.Rotated(RotationDirection.Counterclockwise).FacingCell;
			yield return loc + rot.FacingCell * 3;
			yield return loc + rot.FacingCell * 3 - perpOffset;
			yield return loc + rot.FacingCell * 3 - perpOffset * 2;
			yield return loc + rot.FacingCell * 3 + perpOffset;
			yield return loc + rot.FacingCell * 3 + perpOffset * 2;
		}

		public CellRect WaterUseRect()
		{
			return WaterUseRect(parent.Position, parent.Rotation);
		}

		public static CellRect WaterUseRect(IntVec3 loc, Rot4 rot)
		{
			int width = rot.IsHorizontal ? 7 : 13;
			int height = rot.IsHorizontal ? 13 : 7;
			return CellRect.CenteredOn(loc + rot.FacingCell * 4, width, height);
		}

		public IEnumerable<IntVec3> WaterUseCells()
		{
			return WaterUseCells(parent.Position, parent.Rotation);
		}

		public static IEnumerable<IntVec3> WaterUseCells(IntVec3 loc, Rot4 rot)
		{
			foreach (IntVec3 item in WaterUseRect(loc, rot))
			{
				yield return item;
			}
		}

		public IEnumerable<IntVec3> GroundCells()
		{
			return GroundCells(parent.Position, parent.Rotation);
		}

		public static IEnumerable<IntVec3> GroundCells(IntVec3 loc, Rot4 rot)
		{
			IntVec3 perpOffset = rot.Rotated(RotationDirection.Counterclockwise).FacingCell;
			yield return loc - rot.FacingCell;
			yield return loc - rot.FacingCell - perpOffset;
			yield return loc - rot.FacingCell + perpOffset;
			yield return loc;
			yield return loc - perpOffset;
			yield return loc + perpOffset;
			yield return loc + rot.FacingCell;
			yield return loc + rot.FacingCell - perpOffset;
			yield return loc + rot.FacingCell + perpOffset;
		}

		public override void PostDraw()
		{
			base.PostDraw();
			Vector3 a = parent.TrueCenter();
			a += parent.Rotation.FacingCell.ToVector3() * 2.36f;
			for (int i = 0; i < 9; i++)
			{
				float num = spinPosition + (float)Math.PI * 2f * (float)i / 9f;
				float x = Mathf.Abs(4f * Mathf.Sin(num));
				bool num2 = num % ((float)Math.PI * 2f) < (float)Math.PI;
				Vector2 vector = new Vector2(x, 1f);
				Vector3 s = new Vector3(vector.x, 1f, vector.y);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(a + Vector3.up * 0.0454545468f * Mathf.Cos(num), parent.Rotation.AsQuat, s);
				Graphics.DrawMesh(num2 ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, BladesMat, 0);
			}
		}

		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			if (waterUsable && waterDoubleUsed)
			{
				text += "\n" + "Watermill_WaterUsedTwice".Translate();
			}
			return text;
		}
	}
}
