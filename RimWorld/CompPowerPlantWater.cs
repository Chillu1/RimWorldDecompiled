using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class CompPowerPlantWater : CompPowerPlant
{
	private float spinPosition;

	private bool cacheDirty = true;

	private bool waterUsable;

	private bool waterDoubleUsed;

	private float spinRate = 1f;

	private const float PowerFactorIfWaterDoubleUsed = 0.3f;

	private const float SpinRateFactor = 1f / 150f;

	private const float BladeOffset = 2.36f;

	private const int BladeCount = 9;

	private const int RebuildCacheIntervalTicks = 1200;

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

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		base.PostDeSpawn(map, mode);
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
			if (item.InBounds(parent.Map) && !item.GetAffordances(parent.Map).Contains(TerrainAffordanceDefOf.MovingFluid))
			{
				waterUsable = false;
				break;
			}
		}
		waterDoubleUsed = false;
		List<Building> list = parent.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.WatermillGenerator);
		foreach (IntVec3 item2 in WaterUseCells())
		{
			if (!item2.InBounds(parent.Map))
			{
				continue;
			}
			foreach (Building item3 in list)
			{
				if (item3 != parent && item3.GetComp<CompPowerPlantWater>().WaterUseRect().Contains(item2))
				{
					waterDoubleUsed = true;
					break;
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
			spinPosition = (spinPosition + 1f / 150f * spinRate + MathF.PI * 2f) % (MathF.PI * 2f);
		}
		if (parent.IsHashIntervalTick(1200))
		{
			RebuildCache();
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
		int width = (rot.IsHorizontal ? 7 : 13);
		int height = (rot.IsHorizontal ? 13 : 7);
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
	}

	public override void PostDraw()
	{
		base.PostDraw();
		Vector3 vector = parent.TrueCenter();
		vector += parent.Rotation.FacingCell.ToVector3() * 2.36f;
		for (int i = 0; i < 9; i++)
		{
			float num = spinPosition + MathF.PI * 2f * (float)i / 9f;
			float x = Mathf.Abs(4f * Mathf.Sin(num));
			bool num2 = num % (MathF.PI * 2f) < MathF.PI;
			Vector2 vector2 = new Vector2(x, 1f);
			Vector3 s = new Vector3(vector2.x, 1f, vector2.y);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(vector + Vector3.up * 0.03658537f * Mathf.Cos(num), parent.Rotation.AsQuat, s);
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
