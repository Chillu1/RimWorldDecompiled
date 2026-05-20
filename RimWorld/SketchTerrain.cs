using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class SketchTerrain : SketchBuildable
{
	public TerrainDef def;

	public ThingDef stuffForComparingSimilar;

	public bool treatSimilarAsSame;

	private CellRect cachedOccupiedRect = CellRect.Empty;

	private IntVec3 cachedOccupiedRectPos = IntVec3.Invalid;

	public override BuildableDef Buildable => def;

	public override ThingDef Stuff => stuffForComparingSimilar;

	public override float SpawnOrder => 1f;

	public override CellRect OccupiedRect
	{
		get
		{
			if (cachedOccupiedRectPos != pos)
			{
				cachedOccupiedRect = CellRect.SingleCell(pos);
				cachedOccupiedRectPos = pos;
			}
			return cachedOccupiedRect;
		}
	}

	public override string Label
	{
		get
		{
			if (def.designatorDropdown == null || def.designatorDropdown.label.NullOrEmpty() || !treatSimilarAsSame)
			{
				return base.Label;
			}
			return def.designatorDropdown.label;
		}
	}

	public override string LabelCap => Label.CapitalizeFirst();

	public override void DrawGhost(IntVec3 at, Color color)
	{
		ThingDef blueprintDef = def.blueprintDef;
		GraphicDatabase.Get(blueprintDef.graphic.GetType(), blueprintDef.graphic.path, blueprintDef.graphic.Shader, blueprintDef.graphic.drawSize, color, Color.white, blueprintDef.graphicData, null).DrawFromDef(at.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint), Rot4.North, def.blueprintDef);
	}

	public override bool IsSameSpawned(IntVec3 at, Map map)
	{
		if (!at.InBounds(map))
		{
			return false;
		}
		return IsSameOrSimilar(at.GetTerrain(map));
	}

	public bool IsSameOrSimilar(BuildableDef other)
	{
		if (other == null)
		{
			return false;
		}
		if (!treatSimilarAsSame)
		{
			return other == def;
		}
		if (def.designatorDropdown == null && other.designatorDropdown == null && other.BuildableByPlayer)
		{
			return true;
		}
		if (def.designatorDropdown == null || other.designatorDropdown == null)
		{
			return other == def;
		}
		return other.designatorDropdown == def.designatorDropdown;
	}

	public override Thing GetSpawnedBlueprintOrFrame(IntVec3 at, Map map)
	{
		if (!at.InBounds(map))
		{
			return null;
		}
		List<Thing> thingList = at.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i].Position == at && IsSameOrSimilar(thingList[i].def.entityDefToBuild))
			{
				return thingList[i];
			}
		}
		return null;
	}

	public override bool IsSpawningBlocked(IntVec3 at, Map map, Thing thingToIgnore = null, bool wipeIfCollides = false)
	{
		if (IsSpawningBlockedPermanently(at, map, thingToIgnore, wipeIfCollides))
		{
			return true;
		}
		if (!GenAdj.OccupiedRect(at, Rot4.North, def.Size).InBounds(map))
		{
			return true;
		}
		if (!GenConstruct.CanPlaceBlueprintAt(def, at, Rot4.North, map, wipeIfCollides, thingToIgnore).Accepted)
		{
			return true;
		}
		return false;
	}

	public override bool IsSpawningBlockedPermanently(IntVec3 at, Map map, Thing thingToIgnore = null, bool wipeIfCollides = false)
	{
		if (!at.InBounds(map))
		{
			return true;
		}
		if (!CanBuildOnTerrain(at, map))
		{
			return true;
		}
		if (FirstPermanentBlockerAt(at, map) != null)
		{
			return true;
		}
		return false;
	}

	public override bool CanBuildOnTerrain(IntVec3 at, Map map)
	{
		return GenConstruct.CanBuildOnTerrain(def, at, map, Rot4.North);
	}

	public override bool Spawn(IntVec3 at, Map map, Faction faction, Sketch.SpawnMode spawnMode = Sketch.SpawnMode.Normal, bool wipeIfCollides = false, bool forceTerrainAffordance = false, List<Thing> spawnedThings = null, bool dormant = false, TerrainDef defaultAffordanceTerrain = null)
	{
		if (!at.InBounds(map))
		{
			return false;
		}
		if (forceTerrainAffordance && !CanBuildOnTerrain(at, map))
		{
			ForceTerrainAffordance(at, Rot4.North, map, defaultAffordanceTerrain);
		}
		if (IsSpawningBlocked(at, map, null, wipeIfCollides))
		{
			return false;
		}
		switch (spawnMode)
		{
		case Sketch.SpawnMode.Blueprint:
			GenConstruct.PlaceBlueprintForBuild(GetDefFromStuff(), at, map, Rot4.North, faction, null, null, null, sendBPSpawnedSignal: false);
			break;
		case Sketch.SpawnMode.Normal:
			if (def.isFoundation)
			{
				map.terrainGrid.SetFoundation(at, GetDefFromStuff());
			}
			else if (def.temporary)
			{
				map.terrainGrid.SetTempTerrain(at, GetDefFromStuff());
			}
			else
			{
				map.terrainGrid.SetTerrain(at, GetDefFromStuff());
			}
			break;
		default:
			throw new NotImplementedException("Spawn mode " + spawnMode.ToString() + " not implemented!");
		}
		return true;
	}

	private TerrainDef GetDefFromStuff()
	{
		if (stuffForComparingSimilar == null)
		{
			return def;
		}
		foreach (TerrainDef allDef in DefDatabase<TerrainDef>.AllDefs)
		{
			if (IsSameOrSimilar(allDef) && !allDef.CostList.NullOrEmpty() && allDef.CostList[0].thingDef == stuffForComparingSimilar)
			{
				return allDef;
			}
		}
		return def;
	}

	public override bool SameForSubtracting(SketchEntity other)
	{
		if (!(other is SketchTerrain sketchTerrain))
		{
			return false;
		}
		if (sketchTerrain == this)
		{
			return true;
		}
		if (IsSameOrSimilar(sketchTerrain.Buildable))
		{
			return pos == sketchTerrain.pos;
		}
		return false;
	}

	public override SketchEntity DeepCopy()
	{
		SketchTerrain obj = (SketchTerrain)base.DeepCopy();
		obj.def = def;
		obj.stuffForComparingSimilar = stuffForComparingSimilar;
		obj.treatSimilarAsSame = treatSimilarAsSame;
		return obj;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref def, "def");
		Scribe_Defs.Look(ref stuffForComparingSimilar, "stuff");
		Scribe_Values.Look(ref treatSimilarAsSame, "treatSimilarAsSame", defaultValue: false);
	}
}
