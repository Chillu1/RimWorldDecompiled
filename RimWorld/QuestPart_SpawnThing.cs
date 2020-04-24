using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
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

		private Thing innerSkyfallerThing;

		private bool spawned;

		public override IEnumerable<GlobalTargetInfo> QuestLookTargets
		{
			get
			{
				foreach (GlobalTargetInfo questLookTarget2 in base.QuestLookTargets)
				{
					yield return questLookTarget2;
				}
				if (questLookTarget)
				{
					yield return innerSkyfallerThing ?? thing;
				}
			}
		}

		public override bool IncreasesPopulation
		{
			get
			{
				Pawn pawn = thing as Pawn;
				if (pawn == null)
				{
					return false;
				}
				return PawnsArriveQuestPartUtility.IncreasesPopulation(Gen.YieldSingle(pawn), joinPlayer: false, makePrisoners: false);
			}
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (!(signal.tag == inSignal) || !mapParent.HasMap)
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
				if (tryLandInShipLandingZone && !DropCellFinder.TryFindShipLandingArea(mapParent.Map, out result, out Thing firstBlockingThing))
				{
					if (firstBlockingThing != null)
					{
						Messages.Message("ShuttleBlocked".Translate("BlockedBy".Translate(firstBlockingThing).CapitalizeFirst()), firstBlockingThing, MessageTypeDefOf.NeutralEvent);
					}
					result = DropCellFinder.TryFindSafeLandingSpotCloseToColony(mapParent.Map, thing.def.Size);
				}
				if (!result.IsValid && (!lookForSafeSpot || !DropCellFinder.FindSafeLandingSpot(out result, factionForFindingSpot, mapParent.Map, 35, 15, 25, thing.def.size)))
				{
					IntVec3 intVec = DropCellFinder.RandomDropSpot(mapParent.Map);
					if (!DropCellFinder.TryFindDropSpotNear(intVec, mapParent.Map, out result, allowFogged: false, canRoofPunch: false, allowIndoors: false, thing.def.size))
					{
						result = intVec;
					}
				}
			}
			GenPlace.TryPlaceThing(thing, result, mapParent.Map, ThingPlaceMode.Near);
			spawned = true;
			Skyfaller skyfaller = thing as Skyfaller;
			if (skyfaller != null && skyfaller.innerContainer.Count == 1)
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
			return p == thing;
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
}
