using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class SketchThing : SketchBuildable
{
	public ThingDef def;

	public ThingDef stuff;

	public int stackCount;

	public Rot4 rot;

	public QualityCategory? quality;

	public int? hitPoints;

	public float spawnOrder = 2f;

	public override BuildableDef Buildable => def;

	public override ThingDef Stuff => stuff;

	public override string Label => GenLabel.ThingLabel(def, stuff, stackCount);

	public override string LabelCap => Label.CapitalizeFirst();

	public override CellRect OccupiedRect => GenAdj.OccupiedRect(pos, rot, def.size);

	public override float SpawnOrder => spawnOrder;

	public int MaxHitPoints => Mathf.RoundToInt(def.GetStatValueAbstract(StatDefOf.MaxHitPoints, stuff ?? GenStuff.DefaultStuffFor(def)));

	public Thing Instantiate()
	{
		Thing thing = ThingMaker.MakeThing(def, stuff ?? GenStuff.DefaultStuffFor(def));
		thing.stackCount = stackCount;
		if (quality.HasValue)
		{
			thing.TryGetComp<CompQuality>()?.SetQuality(quality.Value, ArtGenerationContext.Outsider);
		}
		if (hitPoints.HasValue)
		{
			thing.HitPoints = hitPoints.Value;
		}
		return thing;
	}

	public override void DrawGhost(IntVec3 at, Color color)
	{
		GhostDrawer.DrawGhostThing(at, rot, def, def.graphic, color, AltitudeLayer.Blueprint, null, drawPlaceWorkers: false);
	}

	public Thing GetSameSpawned(IntVec3 at, Map map)
	{
		if (!at.InBounds(map))
		{
			return null;
		}
		List<Thing> thingList = at.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (IsSame(thingList[i]))
			{
				return thingList[i];
			}
		}
		return null;
	}

	public bool IsSame(Thing thing)
	{
		CellRect cellRect = GenAdj.OccupiedRect(thing.Position, rot, thing.def.Size);
		CellRect cellRect2 = GenAdj.OccupiedRect(thing.Position, rot.Opposite, thing.def.Size);
		CellRect cellRect3 = thing.OccupiedRect();
		if (thing.def == def && (stuff == null || thing.Stuff == stuff) && (thing.Rotation == rot || thing.Rotation == rot.Opposite || !def.rotatable))
		{
			if (!(cellRect == cellRect3))
			{
				return cellRect2 == cellRect3;
			}
			return true;
		}
		return false;
	}

	public override bool IsSameSpawned(IntVec3 at, Map map)
	{
		return GetSameSpawned(at, map) != null;
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
			CellRect cellRect = GenAdj.OccupiedRect(at, rot, thingList[i].def.Size);
			CellRect cellRect2 = GenAdj.OccupiedRect(at, rot.Opposite, thingList[i].def.Size);
			CellRect cellRect3 = thingList[i].OccupiedRect();
			if ((cellRect == cellRect3 || cellRect2 == cellRect3) && thingList[i].def.entityDefToBuild == def && (stuff == null || ((IConstructible)thingList[i]).EntityToBuildStuff() == stuff) && (thingList[i].Rotation == rot || thingList[i].Rotation == rot.Opposite || !def.rotatable))
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
		if (!GenAdj.OccupiedRect(at, rot, def.Size).InBounds(map))
		{
			return true;
		}
		if (!GenConstruct.CanPlaceBlueprintAt(def, at, rot, map, wipeIfCollides, thingToIgnore, null, stuff ?? GenStuff.DefaultStuffFor(def)).Accepted)
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
		return GenConstruct.CanBuildOnTerrain(def, at, map, rot, null, stuff ?? GenStuff.DefaultStuffFor(def));
	}

	public override bool Spawn(IntVec3 at, Map map, Faction faction, Sketch.SpawnMode spawnMode = Sketch.SpawnMode.Normal, bool wipeIfCollides = false, bool forceTerrainAffordance = false, List<Thing> spawnedThings = null, bool dormant = false, TerrainDef defaultAffordanceTerrain = null)
	{
		if (!at.RectAbout(def.size, rot).InBounds(map))
		{
			return false;
		}
		if (forceTerrainAffordance && !CanBuildOnTerrain(at, map))
		{
			ForceTerrainAffordance(at, rot, map, defaultAffordanceTerrain);
		}
		if (IsSpawningBlocked(at, map, null, wipeIfCollides))
		{
			return false;
		}
		switch (spawnMode)
		{
		case Sketch.SpawnMode.Blueprint:
			GenConstruct.PlaceBlueprintForBuild(def, at, map, rot, faction, stuff ?? GenStuff.DefaultStuffFor(def), null, null, sendBPSpawnedSignal: false);
			break;
		case Sketch.SpawnMode.Normal:
		{
			Thing thing2 = Instantiate();
			spawnedThings?.Add(thing2);
			if (faction != null && thing2.def.CanHaveFaction)
			{
				thing2.SetFactionDirect(faction);
			}
			SetDormant(thing2, dormant);
			GenSpawn.Spawn(thing2, at, map, rot, WipeMode.VanishOrMoveAside);
			break;
		}
		case Sketch.SpawnMode.TransportPod:
		{
			Thing thing = Instantiate();
			thing.Position = at;
			thing.Rotation = rot;
			spawnedThings?.Add(thing);
			if (faction != null)
			{
				thing.SetFactionDirect(faction);
			}
			SetDormant(thing, dormant);
			ActiveTransporterInfo activeTransporterInfo = new ActiveTransporterInfo();
			activeTransporterInfo.innerContainer.TryAdd(thing, 1);
			activeTransporterInfo.openDelay = 60;
			activeTransporterInfo.leaveSlag = false;
			activeTransporterInfo.despawnPodBeforeSpawningThing = true;
			activeTransporterInfo.spawnWipeMode = (wipeIfCollides ? new WipeMode?(WipeMode.VanishOrMoveAside) : ((WipeMode?)null));
			activeTransporterInfo.moveItemsAsideBeforeSpawning = true;
			activeTransporterInfo.setRotation = rot;
			DropPodUtility.MakeDropPodAt(at, map, activeTransporterInfo, faction);
			break;
		}
		default:
			throw new NotImplementedException("Spawn mode " + spawnMode.ToString() + " not implemented!");
		}
		return true;
	}

	private void SetDormant(Thing thing, bool dormant)
	{
		CompCanBeDormant compCanBeDormant = thing.TryGetComp<CompCanBeDormant>();
		if (compCanBeDormant != null)
		{
			if (dormant)
			{
				compCanBeDormant.ToSleep();
			}
			else
			{
				compCanBeDormant.WakeUp();
			}
		}
	}

	public override bool SameForSubtracting(SketchEntity other)
	{
		if (!(other is SketchThing sketchThing))
		{
			return false;
		}
		if (sketchThing == this)
		{
			return true;
		}
		if (def == sketchThing.def && stuff == sketchThing.stuff && stackCount == sketchThing.stackCount && pos == sketchThing.pos && rot == sketchThing.rot && quality == sketchThing.quality && hitPoints == sketchThing.hitPoints)
		{
			return spawnOrder == sketchThing.spawnOrder;
		}
		return false;
	}

	public override SketchEntity DeepCopy()
	{
		SketchThing obj = (SketchThing)base.DeepCopy();
		obj.def = def;
		obj.stuff = stuff;
		obj.stackCount = stackCount;
		obj.rot = rot;
		obj.quality = quality;
		obj.hitPoints = hitPoints;
		obj.spawnOrder = spawnOrder;
		return obj;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref def, "def");
		Scribe_Defs.Look(ref stuff, "stuff");
		Scribe_Values.Look(ref stackCount, "stackCount", 0);
		Scribe_Values.Look(ref rot, "rot");
		Scribe_Values.Look(ref quality, "quality");
		Scribe_Values.Look(ref hitPoints, "hitPoints");
		Scribe_Values.Look(ref spawnOrder, "spawnOrder", 0f);
	}
}
