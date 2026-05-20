using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_MechanitorShip : QuestNode
{
	private const string QuestTag = "MechanitorShip";

	private const int TicksToShuttleArrival = 180;

	private const int TicksToBeginAssault = 5000;

	private static readonly IntRange RandomMechanitorCorpseAge = new IntRange(50, 360000000);

	private const int LandingSpotDistanceToHostiles = 55;

	public float combatPointsFactor = 1f;

	public List<ShipPawnGroup> mechGroups;

	protected override void RunInt()
	{
		if (!ModLister.CheckBiotech("Mechanitor ship"))
		{
			return;
		}
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = slate.Get<Map>("map") ?? QuestGen_Get.GetMap();
		float num = slate.Get("points", 0f);
		string questTagToAdd = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("MechanitorShip");
		string defendTimeoutSignal = QuestGen.GenerateNewSignal("DefendTimeout");
		string beginAssaultSignal = QuestGen.GenerateNewSignal("BeginAssault");
		string assaultBeganSignal = QuestGen.GenerateNewSignal("AssaultBegan");
		string attackedSignal = QuestGenUtility.HardcodedSignalWithQuestID("mechs.TookDamageFromPlayer");
		string text = QuestGenUtility.HardcodedSignalWithQuestID("mechs.Destroyed");
		string text2 = QuestGenUtility.HardcodedSignalWithQuestID("mechs.Despawned");
		slate.Set("map", map);
		List<Thing> shuttleContents = new List<Thing>();
		List<Pawn> mechs = new List<Pawn>();
		GeneratePawns(mechGroups.RandomElement(), num * combatPointsFactor, quest, mechs);
		shuttleContents.AddRange(mechs);
		string var = (from m in mechs
			group m by m.kindDef into g
			select GenLabel.BestKindLabel(g.Key, Gender.None) + " x" + g.Count()).ToLineList(" - ", capitalizeItems: true);
		slate.Set("mechList", var);
		slate.Set("mechs", mechs);
		PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Mechanitor_Basic, Faction.OfAncients, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: true);
		Pawn mechanitor = quest.GeneratePawn(request);
		mechanitor.Corpse.Age = RandomMechanitorCorpseAge.RandomInRange;
		mechanitor.relations.hidePawnRelations = true;
		mechanitor.Corpse.GetComp<CompRottable>().RotProgress += mechanitor.Corpse.Age;
		mechanitor.Corpse.SetForbidden(value: true);
		shuttleContents.Add(mechanitor.Corpse);
		slate.Set("mechanitor", mechanitor);
		Thing thing = ThingMaker.MakeThing(ThingDefOf.ShuttleCrashed_Exitable_Mechanitor);
		quest.SetFaction(Gen.YieldSingle(thing), Faction.OfAncients);
		TryFindShuttleCrashPosition(map, thing.def.size, out var shuttleCrashPosition);
		TransportShip transportShip = quest.GenerateTransportShip(TransportShipDefOf.Ship_ShuttleCrashing_Mechanitor, shuttleContents, thing).transportShip;
		quest.AddShipJob_WaitTime(transportShip, 60, leaveImmediatelyWhenSatisfied: false).showGizmos = false;
		quest.AddShipJob_Unload(transportShip, ShipJobStartMode.Queue, unforbidAll: false);
		QuestUtility.AddQuestTag(ref transportShip.questTags, questTagToAdd);
		quest.Delay(180, delegate
		{
			quest.Letter(LetterDefOf.NegativeEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, shuttleContents, filterDeadPawnsFromLookTargets: false, "[mechanitorShuttleCrashedLetterText]", null, "[mechanitorShuttleCrashedLetterLabel]");
			quest.AddShipJob_Arrive(transportShip, map.Parent, null, shuttleCrashPosition, ShipJobStartMode.Force_DelayCurrent, Faction.OfMechanoids);
			quest.DefendPoint(map.Parent, mechanitor, shuttleCrashPosition, mechs, Faction.OfMechanoids, null, null, 5f);
			quest.Delay(5000, delegate
			{
				quest.SignalPass(null, null, attackedSignal);
			}).debugLabel = "Assault delay";
			quest.AnySignal(new string[2] { attackedSignal, defendTimeoutSignal }, null, Gen.YieldSingle(beginAssaultSignal));
			quest.SignalPassActivable(delegate
			{
				quest.AnyPawnInCombatShape(mechs, delegate
				{
					QuestPart_AssaultColony questPart_AssaultColony = quest.AssaultColony(Faction.OfMechanoids, map.Parent, mechs);
					questPart_AssaultColony.canKidnap = false;
					questPart_AssaultColony.canSteal = false;
					questPart_AssaultColony.canTimeoutOrFlee = false;
					quest.Letter(LetterDefOf.ThreatSmall, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, mechs, filterDeadPawnsFromLookTargets: false, "[assaultBeginLetterText]", null, "[assaultBeginLetterLabel]");
				}, null, null, assaultBeganSignal);
			}, null, beginAssaultSignal, null, null, assaultBeganSignal);
		}).debugLabel = "Arrival delay";
		quest.AnySignal(new string[2] { text, text2 }, delegate
		{
			quest.AllPawnsDespawned(mechs, delegate
			{
				quest.AnyPawnHasHediff(Gen.YieldSingle(mechanitor), HediffDefOf.MechlinkImplant, delegate
				{
					quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(mechanitor.Corpse), filterDeadPawnsFromLookTargets: false, "[mechlinkAvailableLetterText]", null, "[mechlinkAvailableLetterLabel]");
				});
				QuestGen_End.End(quest, QuestEndOutcome.Success);
			});
		});
	}

	private void GeneratePawns(ShipPawnGroup group, float points, Quest quest, List<Pawn> outPawns)
	{
		foreach (ShipPawnOption item in group.options.Where((ShipPawnOption o) => o.requireOneOf))
		{
			outPawns.Add(quest.GeneratePawn(item.pawnKind, Faction.OfMechanoids));
			points -= item.pawnKind.combatPower;
		}
		ShipPawnOption result;
		while (group.options.Where((ShipPawnOption o) => o.pawnKind.combatPower < points).TryRandomElementByWeight((ShipPawnOption o) => o.weight, out result))
		{
			outPawns.Add(quest.GeneratePawn(result.pawnKind, Faction.OfMechanoids));
			points -= result.pawnKind.combatPower;
		}
	}

	protected override bool TestRunInt(Slate slate)
	{
		Map map = QuestGen_Get.GetMap();
		if (map == null)
		{
			return false;
		}
		if (Faction.OfMechanoids == null)
		{
			return false;
		}
		if (!TryFindShuttleCrashPosition(map, ThingDefOf.ShuttleCrashed_Exitable_Mechanitor.size, out var _))
		{
			return false;
		}
		return true;
	}

	private bool TryFindShuttleCrashPosition(Map map, IntVec2 size, out IntVec3 spot)
	{
		if (DropCellFinder.FindSafeLandingSpot(out spot, Faction.OfMechanoids, map, 55, 15, 25, size, ThingDefOf.ShuttleCrashed_Exitable_Mechanitor.interactionCellOffset))
		{
			return true;
		}
		return false;
	}
}
