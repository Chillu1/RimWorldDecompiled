using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_SpawnThing : QuestPart
{
	public string inSignal;

	public Thing thing;

	public Faction factionForFindingSpot;

	public MapParent mapParent;

	public IntVec3 cell = IntVec3.Invalid;

	public bool questLookTarget = true;

	public bool lookForSafeSpot;

	public bool tryLandInShipLandingZone;

	public Thing tryLandNearThing;

	public Pawn mapParentOfPawn;

	public EffecterDef spawnEffecter;

	private Thing innerSkyfallerThing;

	private bool spawned;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (this.questLookTarget)
			{
				yield return innerSkyfallerThing ?? thing;
			}
		}
	}

	public override bool IncreasesPopulation
	{
		get
		{
			if (!(thing is Pawn val))
			{
				return false;
			}
			return PawnsArriveQuestPartUtility.IncreasesPopulation(Gen.YieldSingle(val), joinPlayer: false, makePrisoners: false);
		}
	}

	public MapParent MapParent
	{
		get
		{
			if (mapParentOfPawn != null)
			{
				return mapParentOfPawn.MapHeld?.Parent;
			}
			return mapParent;
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag != inSignal)
		{
			return;
		}
		if (MapParent == null || MapParent.Destroyed)
		{
			mapParentOfPawn = null;
			mapParent = quest.TryFindNewSuitableMapParentForRetarget();
			cell = IntVec3.Invalid;
		}
		if (!MapParent.HasMap)
		{
			return;
		}
		IntVec3 result = IntVec3.Invalid;
		if (cell.IsValid)
		{
			result = cell;
		}
		else
		{
			if (tryLandInShipLandingZone && !DropCellFinder.TryFindShipLandingArea(MapParent.Map, out result, out var firstBlockingThing))
			{
				if (firstBlockingThing != null)
				{
					Messages.Message("ShuttleBlocked".Translate("BlockedBy".Translate(firstBlockingThing).CapitalizeFirst()), firstBlockingThing, MessageTypeDefOf.NeutralEvent);
				}
				result = DropCellFinder.TryFindSafeLandingSpotCloseToColony(MapParent.Map, thing.def.Size, factionForFindingSpot);
			}
			if (!result.IsValid && tryLandNearThing != null)
			{
				DropCellFinder.FindSafeLandingSpotNearAvoidingHostiles(tryLandNearThing, MapParent.Map, out result, 35, 15, 25, thing.def.size);
			}
			if (!result.IsValid && (!lookForSafeSpot || !DropCellFinder.FindSafeLandingSpot(out result, factionForFindingSpot, MapParent.Map, 35, 15, 25, thing.def.size)))
			{
				IntVec3 intVec = DropCellFinder.RandomDropSpot(MapParent.Map);
				if (!DropCellFinder.TryFindDropSpotNear(intVec, MapParent.Map, out result, allowFogged: false, canRoofPunch: false, allowIndoors: false, thing.def.size))
				{
					result = intVec;
				}
			}
		}
		GenPlace.TryPlaceThing(thing, result, MapParent.Map, ThingPlaceMode.Near);
		spawned = true;
		if (spawnEffecter != null)
		{
			spawnEffecter.SpawnMaintained(thing, thing.Map);
		}
		if (thing is Skyfaller skyfaller && skyfaller.innerContainer.Count == 1)
		{
			innerSkyfallerThing = skyfaller.innerContainer.First();
		}
		else
		{
			innerSkyfallerThing = null;
		}
	}

	public override bool QuestPartReserves(Pawn p)
	{
		if (p != thing)
		{
			if (thing is Skyfaller)
			{
				return ((Skyfaller)thing).innerContainer.Contains(p);
			}
			return false;
		}
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref spawned, "spawned", defaultValue: false);
		if (!spawned && (thing == null || !(thing is Pawn)))
		{
			Scribe_Deep.Look(ref thing, "thing");
		}
		else
		{
			Scribe_References.Look(ref thing, "thing");
		}
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Values.Look(ref cell, "cell");
		Scribe_Values.Look(ref lookForSafeSpot, "lookForSafeSpot", defaultValue: false);
		Scribe_References.Look(ref factionForFindingSpot, "factionForFindingSpot");
		Scribe_Values.Look(ref questLookTarget, "questLookTarget", defaultValue: true);
		Scribe_References.Look(ref innerSkyfallerThing, "innerSkyfallerThing");
		Scribe_Values.Look(ref tryLandInShipLandingZone, "tryLandInShipLandingZone", defaultValue: false);
		Scribe_References.Look(ref tryLandNearThing, "tryLandNearThing");
		Scribe_References.Look(ref mapParentOfPawn, "mapParentOfPawn");
		Scribe_Defs.Look(ref spawnEffecter, "spawnEffecter");
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		if (Find.AnyPlayerHomeMap != null)
		{
			mapParent = Find.RandomPlayerHomeMap.Parent;
			thing = ThingMaker.MakeThing(ThingDefOf.Silver);
		}
	}
}
