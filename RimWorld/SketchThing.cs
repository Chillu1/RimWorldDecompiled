using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class SketchThing : SketchBuildable
	{
		public ThingDef def;

		public ThingDef stuff;

		public int stackCount;

		public Rot4 rot;

		public QualityCategory? quality;

		public int? hitPoints;

		public override BuildableDef Buildable => def;

		public override ThingDef Stuff => stuff;

		public override string Label => GenLabel.ThingLabel(def, stuff, stackCount);

		public override CellRect OccupiedRect => GenAdj.OccupiedRect(pos, rot, def.size);

		public override float SpawnOrder => 2f;

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
			GhostDrawer.DrawGhostThing(at, rot, def, def.graphic, color, AltitudeLayer.Blueprint);
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
				CellRect lhs = GenAdj.OccupiedRect(at, rot, thingList[i].def.Size);
				CellRect lhs2 = GenAdj.OccupiedRect(at, rot.Opposite, thingList[i].def.Size);
				CellRect rhs = thingList[i].OccupiedRect();
				if ((lhs == rhs || lhs2 == rhs) && thingList[i].def == def && (stuff == null || thingList[i].Stuff == stuff) && (thingList[i].Rotation == rot || thingList[i].Rotation == rot.Opposite || !def.rotatable))
				{
					return thingList[i];
				}
			}
			return null;
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
				CellRect lhs = GenAdj.OccupiedRect(at, rot, thingList[i].def.Size);
				CellRect lhs2 = GenAdj.OccupiedRect(at, rot.Opposite, thingList[i].def.Size);
				CellRect rhs = thingList[i].OccupiedRect();
				if ((lhs == rhs || lhs2 == rhs) && thingList[i].def.entityDefToBuild == def && (stuff == null || ((IConstructible)thingList[i]).EntityToBuildStuff() == stuff) && (thingList[i].Rotation == rot || thingList[i].Rotation == rot.Opposite || !def.rotatable))
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
			foreach (IntVec3 item in GenAdj.OccupiedRect(at, rot, def.Size))
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

		public override bool CanBuildOnTerrain(IntVec3 at, Map map)
		{
			return GenConstruct.CanBuildOnTerrain(def, at, map, rot, null, stuff ?? GenStuff.DefaultStuffFor(def));
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
				GenConstruct.PlaceBlueprintForBuild(def, at, map, rot, faction, stuff ?? GenStuff.DefaultStuffFor(def));
				break;
			case Sketch.SpawnMode.Normal:
			{
				Thing thing2 = Instantiate();
				spawnedThings?.Add(thing2);
				if (faction != null)
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
				ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
				activeDropPodInfo.innerContainer.TryAdd(thing, 1);
				activeDropPodInfo.openDelay = 60;
				activeDropPodInfo.leaveSlag = false;
				activeDropPodInfo.despawnPodBeforeSpawningThing = true;
				activeDropPodInfo.spawnWipeMode = (wipeIfCollides ? new WipeMode?(WipeMode.VanishOrMoveAside) : null);
				activeDropPodInfo.moveItemsAsideBeforeSpawning = true;
				activeDropPodInfo.setRotation = rot;
				DropPodUtility.MakeDropPodAt(at, map, activeDropPodInfo);
				break;
			}
			default:
				throw new NotImplementedException(string.Concat("Spawn mode ", spawnMode, " not implemented!"));
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
			SketchThing sketchThing = other as SketchThing;
			if (sketchThing == null)
			{
				return false;
			}
			if (sketchThing == this)
			{
				return true;
			}
			if (def == sketchThing.def && stuff == sketchThing.stuff && stackCount == sketchThing.stackCount && pos == sketchThing.pos && rot == sketchThing.rot && quality == sketchThing.quality)
			{
				return hitPoints == sketchThing.hitPoints;
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
		}
	}
}
