using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_MechanitorStartingMech : QuestNode
{
	private const int DelayTicks = 600;

	public List<PawnKindDef> mechTypes;

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		Quest quest = QuestGen.quest;
		Pawn pawn = slate.Get<Pawn>("asker");
		PawnKindDef pawnKindDef = mechTypes.RandomElement();
		Map map = pawn.MapHeld;
		PawnGenerationRequest request = new PawnGenerationRequest(pawnKindDef, pawn.Faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn);
		Pawn mech = quest.GeneratePawn(request);
		slate.Set("mechanitor", pawn);
		slate.Set("mech", pawnKindDef);
		quest.Delay(600, delegate
		{
			quest.AssignMechToMechanitor(pawn, mech);
			quest.DropPods(map.Parent, Gen.YieldSingle(mech), null, null, null, null, dropSpot: pawn.PositionHeld, sendStandardLetter: true, useTradeDropSpot: false, joinPlayer: false, makePrisoners: false, inSignal: null, thingsToExcludeFromHyperlinks: null, signalListenMode: QuestPart.SignalListenMode.OngoingOnly, destroyItemsOnCleanup: true, dropAllInSamePod: false, allowFogged: false, canRetargetAnyMap: true);
			quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(mech), filterDeadPawnsFromLookTargets: false, "[arrivalLetterText]", null, "[arrivalLetterLabel]");
			QuestGen_End.End(quest, QuestEndOutcome.Success);
		});
		quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("asker.Killed"));
	}

	protected override bool TestRunInt(Slate slate)
	{
		Pawn pawn = slate.Get<Pawn>("asker");
		if (pawn != null && pawn.SpawnedOrAnyParentSpawned)
		{
			return MechanitorUtility.IsMechanitor(pawn);
		}
		return false;
	}
}
