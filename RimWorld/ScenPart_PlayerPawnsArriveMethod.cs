using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.SketchGen;
using Verse;

namespace RimWorld
{
	public class ScenPart_PlayerPawnsArriveMethod : ScenPart
	{
		private PlayerPawnsArriveMethod method;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref method, "method", PlayerPawnsArriveMethod.Standing);
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			if (!Widgets.ButtonText(listing.GetScenPartRect(this, ScenPart.RowHeight), method.ToStringHuman()))
			{
				return;
			}
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (PlayerPawnsArriveMethod value in Enum.GetValues(typeof(PlayerPawnsArriveMethod)))
			{
				if (value != PlayerPawnsArriveMethod.Gravship || ModsConfig.OdysseyActive)
				{
					PlayerPawnsArriveMethod localM = value;
					list.Add(new FloatMenuOption(localM.ToStringHuman(), delegate
					{
						method = localM;
					}));
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public override string Summary(Scenario scen)
		{
			if (method == PlayerPawnsArriveMethod.DropPods)
			{
				return "ScenPart_ArriveInDropPods".Translate();
			}
			return null;
		}

		public override void Randomize()
		{
			method = ((Rand.Value < 0.5f) ? PlayerPawnsArriveMethod.DropPods : PlayerPawnsArriveMethod.Standing);
		}

		public override void GenerateIntoMap(Map map)
		{
			if (Find.GameInitData == null)
			{
				return;
			}
			List<Thing> list = new List<Thing>();
			foreach (ScenPart allPart in Find.Scenario.AllParts)
			{
				list.AddRange(allPart.PlayerStartingThings());
			}
			foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
			{
				foreach (ThingDefCount item in Find.GameInitData.startingPossessions[startingAndOptionalPawn])
				{
					list.Add(StartingPawnUtility.GenerateStartingPossession(item));
				}
			}
			if (method == PlayerPawnsArriveMethod.Gravship && ModsConfig.OdysseyActive)
			{
				DoGravship(map, list);
			}
			else
			{
				DoDropPods(map, list);
			}
		}

		private void DoDropPods(Map map, List<Thing> startingItems)
		{
			List<List<Thing>> list = new List<List<Thing>>();
			foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
			{
				List<Thing> list2 = new List<Thing>();
				list2.Add(startingAndOptionalPawn);
				list.Add(list2);
			}
			int num = 0;
			foreach (Thing startingItem in startingItems)
			{
				if (startingItem.def.CanHaveFaction)
				{
					startingItem.SetFactionDirect(Faction.OfPlayer);
				}
				list[num].Add(startingItem);
				num++;
				if (num >= list.Count)
				{
					num = 0;
				}
			}
			DropPodUtility.DropThingGroupsNear(MapGenerator.PlayerStartSpot, map, list, 110, Find.GameInitData.QuickStarted || method != PlayerPawnsArriveMethod.DropPods, leaveSlag: true, canRoofPunch: true, forbid: true, allowFogged: false);
		}

		private void DoGravship(Map map, List<Thing> startingItems)
		{
			Sketch sketch = RimWorld.SketchGen.SketchGen.Generate(parms: new SketchResolveParams
			{
				sketch = new Sketch()
			}, root: SketchResolverDefOf.Gravship);
			sketch.Rotate(Rot4.Random);
			HashSet<IntVec3> hashSet = sketch.OccupiedRect.Cells.Select((IntVec3 c) => c - sketch.OccupiedCenter).ToHashSet();
			List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
			map.regionAndRoomUpdater.Enabled = true;
			IntVec3 playerStartSpot = MapGenerator.PlayerStartSpot;
			if (!MapGenerator.PlayerStartSpotValid)
			{
				GenStep_ReserveGravshipArea.SetStartSpot(map, hashSet, orGenerateVar);
				playerStartSpot = MapGenerator.PlayerStartSpot;
			}
			GravshipPlacementUtility.ClearAreaForGravship(map, playerStartSpot, hashSet);
			List<Thing> list = new List<Thing>();
			sketch.Spawn(map, playerStartSpot, Faction.OfPlayer, Sketch.SpawnPosType.OccupiedCenter, Sketch.SpawnMode.Normal, wipeIfCollides: true, forceTerrainAffordance: true, clearEdificeWhereFloor: true, list, dormant: false, buildRoofsInstantly: true);
			IntVec3 offset = playerStartSpot - sketch.OccupiedCenter;
			CellRect cellRect = sketch.OccupiedRect.MovedBy(offset);
			orGenerateVar.Add(cellRect);
			foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
			{
				if (!cellRect.TryRandomElement((IntVec3 c) => c.Standable(map) && (c.GetTerrain(map)?.IsSubstructure ?? false), out var result))
				{
					Log.Error("Could not find a valid spawn location for pawn " + startingAndOptionalPawn.Name);
				}
				else
				{
					GenPlace.TryPlaceThing(startingAndOptionalPawn, result, map, ThingPlaceMode.Near);
				}
			}
			foreach (Thing startingItem in startingItems)
			{
				if (startingItem.def.CanHaveFaction)
				{
					startingItem.SetFactionDirect(Faction.OfPlayer);
				}
				int num = startingItem.stackCount;
				int num2 = 99;
				while (num > 0 && num2-- > 0)
				{
					if (list.Where((Thing t) => t.def == ThingDefOf.Shelf || t.def == ThingDefOf.ShelfSmall).TryRandomElement(out var result2))
					{
						IntVec3 randomCell = result2.OccupiedRect().RandomCell;
						Thing thing = startingItem.SplitOff(Math.Min(startingItem.def.stackLimit, num));
						num -= thing.stackCount;
						GenPlace.TryPlaceThing(thing, randomCell, map, ThingPlaceMode.Near);
					}
				}
			}
			foreach (Thing item in list)
			{
				if (item.def == ThingDefOf.Door)
				{
					MapGenerator.rootsToUnfog.AddRange(GenAdj.CellsAdjacentCardinal(item));
				}
				if (item.TryGetComp(out CompRefuelable comp))
				{
					comp.Refuel(comp.Props.fuelCapacity);
				}
				if (item is Building_GravEngine building_GravEngine)
				{
					building_GravEngine.silentlyActivate = true;
				}
			}
			foreach (IntVec3 item2 in cellRect)
			{
				if (item2.GetTerrain(map) == TerrainDefOf.Substructure)
				{
					map.areaManager.Home[item2] = true;
				}
			}
		}

		public override void PostMapGenerate(Map map)
		{
			if (Find.GameInitData != null && method == PlayerPawnsArriveMethod.DropPods)
			{
				PawnUtility.GiveAllStartingPlayerPawnsThought(ThoughtDefOf.CrashedTogether);
			}
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ method.GetHashCode();
		}
	}
}
