using System;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_WandererJoinAbasia : QuestNode_Root_WandererJoin
{
	private const int TimeoutTicks = 60000;

	private const float RelationWithColonistWeight = 20f;

	private string signalAccept;

	private string signalReject;

	protected override void RunInt()
	{
		base.RunInt();
		Quest quest = QuestGen.quest;
		quest.Delay(60000, delegate
		{
			QuestGen_End.End(quest, QuestEndOutcome.Fail);
		});
	}

	public override Pawn GeneratePawn()
	{
		PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Refugee, null, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 20f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, forceRecruitable: true);
		if (Find.Storyteller.difficulty.ChildrenAllowed)
		{
			request.AllowedDevelopmentalStages |= DevelopmentalStage.Child;
		}
		Pawn pawn = PawnGenerator.GeneratePawn(request);
		pawn.health.AddHediff(HediffDefOf.Abasia);
		if (!pawn.IsWorldPawn())
		{
			Find.WorldPawns.PassToWorld(pawn);
		}
		return pawn;
	}

	protected override void AddSpawnPawnQuestParts(Quest quest, Map map, Pawn pawn)
	{
		base.AddSpawnPawnQuestParts(quest, map, pawn);
		signalAccept = QuestGenUtility.HardcodedSignalWithQuestID("Accept");
		signalReject = QuestGenUtility.HardcodedSignalWithQuestID("Reject");
		quest.Signal(signalAccept, delegate
		{
			quest.SetFaction(Gen.YieldSingle(pawn), Faction.OfPlayer);
			QuestGen_End.End(quest, QuestEndOutcome.Success);
		});
		quest.Signal(signalReject, delegate
		{
			quest.GiveDiedOrDownedThoughts(pawn, PawnDiedOrDownedThoughtsKind.DeniedJoining);
			QuestGen_End.End(quest, QuestEndOutcome.Fail);
		});
	}

	[Obsolete]
	public override void SendLetter(Quest quest, Pawn pawn)
	{
		SendLetter_NewTemp(quest, pawn, Find.AnyPlayerHomeMap);
	}

	public override void SendLetter_NewTemp(Quest quest, Pawn pawn, Map map)
	{
		TaggedString title = "LetterLabelWandererJoinsAbasia".Translate(pawn);
		TaggedString letterText = "LetterTextWandererJoinsAbasia".Translate(pawn, HediffDefOf.Abasia);
		QuestNode_Root_WandererJoin_WalkIn.AppendCharityInfoToLetter("AfflictedJoinerChartityInfo".Translate(pawn, AllowKilledBeforeTicks.ToStringTicksToPeriod()), ref letterText);
		PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref letterText, ref title, pawn);
		QuestNode_Root_WandererJoin_WalkIn.ApplyBestSkillInfoToLetter(ref letterText, pawn);
		ChoiceLetter_AcceptJoiner choiceLetter_AcceptJoiner = (ChoiceLetter_AcceptJoiner)LetterMaker.MakeLetter(title, letterText, LetterDefOf.AcceptJoiner, pawn);
		choiceLetter_AcceptJoiner.signalAccept = signalAccept;
		choiceLetter_AcceptJoiner.signalReject = signalReject;
		choiceLetter_AcceptJoiner.quest = quest;
		choiceLetter_AcceptJoiner.StartTimeout(60000);
		Find.LetterStack.ReceiveLetter(choiceLetter_AcceptJoiner);
	}
}
