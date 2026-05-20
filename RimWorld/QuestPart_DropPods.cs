using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_DropPods : QuestPart
{
	public string inSignal;

	public string outSignalResult;

	public IntVec3 dropSpot = IntVec3.Invalid;

	public bool useTradeDropSpot;

	public MapParent mapParent;

	private List<Thing> items = new List<Thing>();

	private List<Pawn> pawns = new List<Pawn>();

	private List<Pawn> pawnsInContainers = new List<Pawn>();

	public List<ThingDefCountClass> thingDefs = new List<ThingDefCountClass>();

	public List<ThingDef> thingsToExcludeFromHyperlinks = new List<ThingDef>();

	public bool joinPlayer;

	public bool makePrisoners;

	public bool destroyItemsOnCleanup = true;

	public bool dropAllInSamePod;

	public bool allowFogged;

	public Faction faction;

	public bool canRetargetAnyMap;

	public string customLetterText;

	public string customLetterLabel;

	public LetterDef customLetterDef;

	public bool sendStandardLetter = true;

	private Thing importantLookTarget;

	private List<Thing> tmpThingsToDrop = new List<Thing>();

	public IEnumerable<Thing> Things
	{
		get
		{
			return items.Concat(pawns.Cast<Thing>());
		}
		set
		{
			items.Clear();
			pawns.Clear();
			if (value == null)
			{
				return;
			}
			foreach (Thing item3 in value)
			{
				if (item3.Destroyed)
				{
					Log.Error("Tried to add a destroyed thing to QuestPart_DropPods: " + item3.ToStringSafe());
					continue;
				}
				if (item3 is Pawn item)
				{
					pawns.Add(item);
					continue;
				}
				items.Add(item3);
				ThingOwner thingOwner = item3.TryGetInnerInteractableThingOwner();
				if (thingOwner == null)
				{
					continue;
				}
				for (int i = 0; i < thingOwner.Count; i++)
				{
					if (thingOwner[i] is Pawn item2)
					{
						pawnsInContainers.Add(item2);
					}
				}
			}
		}
	}

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (mapParent != null)
			{
				yield return mapParent;
			}
			foreach (Pawn questLookTarget2 in PawnsArriveQuestPartUtility.GetQuestLookTargets(pawns.Concat(pawnsInContainers)))
			{
				yield return questLookTarget2;
			}
			if (importantLookTarget != null)
			{
				yield return importantLookTarget;
			}
		}
	}

	public override bool IncreasesPopulation => PawnsArriveQuestPartUtility.IncreasesPopulation(pawns, joinPlayer, makePrisoners);

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag != inSignal)
		{
			return;
		}
		if (mapParent == null || !mapParent.HasMap || !quest.IsParentSuitableForQuest(mapParent))
		{
			if (canRetargetAnyMap)
			{
				mapParent = quest.TryFindNewSuitablePlayerMapParentForRetarget(checkQuestScript: false);
			}
			else
			{
				mapParent = quest.TryFindNewSuitablePlayerMapParentForRetarget();
			}
		}
		if (mapParent == null || !mapParent.HasMap)
		{
			return;
		}
		pawns.RemoveAll((Pawn x) => x.Destroyed);
		pawnsInContainers.RemoveAll((Pawn x) => x.Destroyed);
		items.RemoveAll((Thing x) => x.Destroyed);
		tmpThingsToDrop.Clear();
		tmpThingsToDrop.AddRange(Things);
		for (int num = 0; num < thingDefs.Count; num++)
		{
			Thing thing = ThingMaker.MakeThing(thingDefs[num].thingDef, GenStuff.RandomStuffByCommonalityFor(thingDefs[num].thingDef));
			thing.stackCount = thingDefs[num].count;
			tmpThingsToDrop.Add(thing);
		}
		tmpThingsToDrop.RemoveAll((Thing x) => x.Spawned);
		Thing thing2 = tmpThingsToDrop.Where((Thing x) => x is Pawn).MaxByWithFallback((Thing x) => x.MarketValue);
		Thing thing3 = tmpThingsToDrop.MaxByWithFallback((Thing x) => x.MarketValue * (float)x.stackCount);
		if (!tmpThingsToDrop.Any())
		{
			return;
		}
		Map map = mapParent.Map;
		IntVec3 intVec = (dropSpot.IsValid ? dropSpot : GetRandomDropSpot());
		TaggedString text = null;
		TaggedString label = null;
		if (sendStandardLetter)
		{
			if (joinPlayer && pawns.Count == 1 && pawns[0].RaceProps.Humanlike)
			{
				text = "LetterRefugeeJoins".Translate(pawns[0].Named("PAWN"));
				label = "LetterLabelRefugeeJoins".Translate(pawns[0].Named("PAWN"));
				PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawns[0]);
			}
			else
			{
				text = "LetterQuestDropPodsArrived".Translate(GenLabel.ThingsLabel(tmpThingsToDrop));
				label = "LetterLabelQuestDropPodsArrived".Translate();
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref label, ref text, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
			}
			label = (customLetterLabel.NullOrEmpty() ? label : customLetterLabel.Formatted(label.Named("BASELABEL")));
			text = (customLetterText.NullOrEmpty() ? text : customLetterText.Formatted(text.Named("BASETEXT")));
		}
		if (joinPlayer)
		{
			for (int num2 = 0; num2 < pawns.Count; num2++)
			{
				if (pawns[num2].Faction != Faction.OfPlayer)
				{
					pawns[num2].SetFaction(Faction.OfPlayer);
				}
			}
		}
		else if (makePrisoners)
		{
			for (int num3 = 0; num3 < pawns.Count; num3++)
			{
				if (pawns[num3].RaceProps.Humanlike)
				{
					if (!pawns[num3].IsPrisonerOfColony)
					{
						pawns[num3].guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Prisoner);
					}
					HealthUtility.TryAnesthetize(pawns[num3]);
				}
			}
		}
		if (dropAllInSamePod)
		{
			DropPodUtility.DropThingGroupsNear(intVec, map, new List<List<Thing>> { tmpThingsToDrop }, 110, instaDrop: false, leaveSlag: false, !useTradeDropSpot, forbid: false, allowFogged, canTransfer: false, faction);
		}
		else
		{
			DropPodUtility.DropThingsNear(intVec, map, tmpThingsToDrop, 110, canInstaDropDuringInit: false, leaveSlag: false, !useTradeDropSpot, forbid: false, allowFogged, faction);
		}
		for (int num4 = 0; num4 < pawns.Count; num4++)
		{
			pawns[num4].needs.SetInitialLevels();
			pawns[num4].mindState?.SetupLastHumanMeatTick();
		}
		if (sendStandardLetter)
		{
			IntVec3 cell = intVec;
			for (int num5 = 0; num5 < tmpThingsToDrop.Count; num5++)
			{
				if (tmpThingsToDrop[num5].SpawnedOrAnyParentSpawned)
				{
					cell = tmpThingsToDrop[num5].PositionHeld;
					break;
				}
			}
			Find.LetterStack.ReceiveLetter(label, text, customLetterDef ?? LetterDefOf.PositiveEvent, new TargetInfo(cell, map), null, quest);
		}
		importantLookTarget = items.Find((Thing x) => x.GetInnerIfMinified() is MonumentMarker).GetInnerIfMinified();
		items.Clear();
		if (!outSignalResult.NullOrEmpty())
		{
			if (thing2 != null)
			{
				Find.SignalManager.SendSignal(new Signal(outSignalResult, thing2.Named("SUBJECT")));
			}
			else if (thing3 != null)
			{
				Find.SignalManager.SendSignal(new Signal(outSignalResult, thing3.Named("SUBJECT")));
			}
			else
			{
				Find.SignalManager.SendSignal(new Signal(outSignalResult));
			}
		}
	}

	public override bool QuestPartReserves(Pawn p)
	{
		if (!pawns.Contains(p))
		{
			return pawnsInContainers.Contains(p);
		}
		return true;
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		pawns.Replace(replace, with);
	}

	public override void PostQuestAdded()
	{
		base.PostQuestAdded();
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].def == ThingDefOf.PsychicAmplifier)
			{
				Find.History.Notify_PsylinkAvailable();
				break;
			}
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (!destroyItemsOnCleanup)
		{
			return;
		}
		for (int i = 0; i < items.Count; i++)
		{
			if (!items[i].Destroyed)
			{
				items[i].Destroy();
			}
		}
		items.Clear();
	}

	private IntVec3 GetRandomDropSpot()
	{
		Map map = mapParent.Map;
		if (useTradeDropSpot)
		{
			return DropCellFinder.TradeDropSpot(map);
		}
		if (CellFinderLoose.TryGetRandomCellWith((IntVec3 x) => x.Standable(map) && !x.Roofed(map) && (allowFogged || !x.Fogged(map)) && map.reachability.CanReachColony(x), map, 1000, out var result))
		{
			return result;
		}
		return DropCellFinder.RandomDropSpot(map);
	}

	public override void Notify_FactionRemoved(Faction f)
	{
		if (faction == f)
		{
			faction = null;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref outSignalResult, "outSignalResult");
		Scribe_Values.Look(ref dropSpot, "dropSpot");
		Scribe_Values.Look(ref useTradeDropSpot, "useTradeDropSpot", defaultValue: false);
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Collections.Look(ref items, "items", LookMode.Deep);
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_Collections.Look(ref pawnsInContainers, "pawnsInContainers", LookMode.Reference);
		Scribe_Collections.Look(ref thingDefs, "thingDefs", LookMode.Deep);
		Scribe_Values.Look(ref joinPlayer, "joinPlayer", defaultValue: false);
		Scribe_Values.Look(ref makePrisoners, "makePrisoners", defaultValue: false);
		Scribe_Values.Look(ref customLetterLabel, "customLetterLabel");
		Scribe_Values.Look(ref customLetterText, "customLetterText");
		Scribe_Defs.Look(ref customLetterDef, "customLetterDef");
		Scribe_Values.Look(ref sendStandardLetter, "sendStandardLetter", defaultValue: true);
		Scribe_References.Look(ref importantLookTarget, "importantLookTarget");
		Scribe_Collections.Look(ref thingsToExcludeFromHyperlinks, "thingsToExcludeFromHyperlinks", LookMode.Def);
		Scribe_Values.Look(ref destroyItemsOnCleanup, "destroyItemsOnCleanup", defaultValue: false);
		Scribe_Values.Look(ref allowFogged, "allowFogged", defaultValue: false);
		Scribe_References.Look(ref faction, "faction");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (thingsToExcludeFromHyperlinks == null)
			{
				thingsToExcludeFromHyperlinks = new List<ThingDef>();
			}
			items.RemoveAll((Thing x) => x == null);
			pawns.RemoveAll((Pawn x) => x == null);
			pawnsInContainers.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		if (Find.AnyPlayerHomeMap == null)
		{
			return;
		}
		mapParent = Find.RandomPlayerHomeMap.Parent;
		List<Thing> list = ThingSetMakerDefOf.DebugQuestDropPodsContents.root.Generate();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is Pawn pawn)
			{
				pawn.relations.everSeenByPlayer = true;
				if (!pawn.IsWorldPawn())
				{
					Find.WorldPawns.PassToWorld(pawn);
				}
			}
		}
		Things = list;
	}
}
