using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class SketchTerrain : SketchBuildable
	{
		public TerrainDef def;

		public override BuildableDef Buildable => def;

		public override ThingDef Stuff => null;

		public override CellRect OccupiedRect => CellRect.SingleCell(pos);

		public override float SpawnOrder => 1f;

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
			return at.GetTerrain(map) == def;
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
				if (thingList[i].Position == at && thingList[i].def.entityDefToBuild == def)
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
			if (!at.InBounds(map))
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
			if (!GenConstruct.CanBuildOnTerrain(def, at, map, Rot4.North))
			{
				return true;
			}
			foreach (IntVec3 item in GenAdj.OccupiedRect(at, Rot4.North, def.Size))
			{
				if (!item.InBounds(map))
				{
					return true;
				}
				List<Thing> thingList = item.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (!thingList[i].def.destroyable && !GenConstruct.CanPlaceBlueprintOver(def, thingList[i].def))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override bool Spawn(IntVec3 at, Map map, Faction faction, Sketch.SpawnMode spawnMode = Sketch.SpawnMode.Normal, bool wipeIfCollides = false, List<Thing> spawnedThings = null, bool dormant = false)
		{
			if (IsSpawningBlocked(at, map, null, wipeIfCollides))
			{
				return false;
			}
			switch (spawnMode)
			{
			case Sketch.SpawnMode.Blueprint:
				GenConstruct.PlaceBlueprintForBuild(def, at, map, Rot4.North, faction, null);
				break;
			case Sketch.SpawnMode.Normal:
				map.terrainGrid.SetTerrain(at, def);
				break;
			default:
				throw new NotImplementedException("Spawn mode " + spawnMode + " not implemented!");
			}
			return true;
		}

		public override bool SameForSubtracting(SketchEntity other)
		{
			SketchTerrain sketchTerrain = other as SketchTerrain;
			if (sketchTerrain == null)
			{
				return false;
			}
			if (sketchTerrain == this)
			{
				return true;
			}
			if (def == sketchTerrain.def)
			{
				return pos == sketchTerrain.pos;
			}
			return false;
		}

		public override SketchEntity DeepCopy()
		{
			SketchTerrain obj = (SketchTerrain)base.DeepCopy();
			obj.def = def;
			return obj;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref def, "def");
		}
	}
}
