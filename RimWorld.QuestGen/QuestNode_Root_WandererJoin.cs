using System;
using Verse;

namespace RimWorld.QuestGen;

public abstract class QuestNode_Root_WandererJoin : QuestNode
{
	protected virtual int AllowKilledBeforeTicks => 15000;

	protected virtual bool CanBeSpace => false;

	public abstract Pawn GeneratePawn();

	[Obsolete]
	public abstract void SendLetter(Quest quest, Pawn pawn);

	public virtual void SendLetter_NewTemp(Quest quest, Pawn pawn, Map map)
	{
		SendLetter(quest, pawn);
	}

	protected virtual void AddSpawnPawnQuestParts(Quest quest, Map map, Pawn pawn)
	{
		quest.DropPods(map.Parent, Gen.YieldSingle(pawn), null, null, null, null, false);
	}

	protected virtual void AddLeftMapQuestParts(Quest quest, Pawn pawn)
	{
		quest.AnyPawnUnhealthy(Gen.YieldSingle(pawn), delegate
		{
			QuestGen_End.End(quest, QuestEndOutcome.Fail);
		}, delegate
		{
			quest.AnyColonistWithCharityPrecept(delegate
			{
				quest.Message("MessageCharityEventFulfilled".Translate() + ": " + "MessageWandererLeftHealthy".Translate(pawn), MessageTypeDefOf.PositiveEvent, getLookTargetsFromSignal: false, null, pawn);
			});
			QuestGen_End.End(quest, QuestEndOutcome.Success);
		});
	}

	protected override void RunInt()
	{
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		if (!slate.TryGet<Map>("map", out var var))
		{
			bool canBeSpace = CanBeSpace;
			var = QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace);
		}
		if (!CanBeSpace)
		{
			quest.AcceptanceRequirementNotSpace(var.Parent);
		}
		Pawn pawn = GeneratePawn();
		AddSpawnPawnQuestParts(quest, var, pawn);
		slate.Set("pawn", pawn);
		SendLetter_NewTemp(quest, pawn, var);
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("pawn.Killed");
		string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("pawn.LeftBehind");
		string inSignal3 = QuestGenUtility.HardcodedSignalWithQuestID("pawn.PlayerTended");
		string inSignal4 = QuestGenUtility.HardcodedSignalWithQuestID("pawn.LeftMap");
		string inSignal5 = QuestGenUtility.HardcodedSignalWithQuestID("pawn.Recruited");
		quest.End(QuestEndOutcome.Success, 0, null, inSignal3);
		quest.Signal(inSignal, delegate
		{
			quest.AcceptedAfterTicks(AllowKilledBeforeTicks, delegate
			{
				quest.AnyColonistWithCharityPrecept(delegate
				{
					quest.Message("MessageCharityEventRefused".Translate() + ": " + "MessageWandererLeftToDie".Translate(pawn), MessageTypeDefOf.NegativeEvent, getLookTargetsFromSignal: false, null, pawn);
				});
				QuestGen_End.End(quest, QuestEndOutcome.Fail);
			}, delegate
			{
				QuestGen_End.End(quest, QuestEndOutcome.Fail);
			});
		});
		quest.Signal(inSignal2, delegate
		{
			quest.AnyColonistWithCharityPrecept(delegate
			{
				quest.Message("MessageCharityEventRefused".Translate() + ": " + "MessageWandererLeftBehind".Translate(pawn), MessageTypeDefOf.NegativeEvent, getLookTargetsFromSignal: false, null, pawn);
			});
			QuestGen_End.End(quest, QuestEndOutcome.Fail);
		});
		quest.AnyColonistWithCharityPrecept(delegate
		{
			quest.Message("MessageCharityEventFulfilled".Translate() + ": " + "MessageWandererRecruited".Translate(pawn), MessageTypeDefOf.PositiveEvent, getLookTargetsFromSignal: false, null, pawn);
		}, null, inSignal5);
		quest.End(QuestEndOutcome.Success, 0, null, inSignal5);
		quest.Signal(inSignal4, delegate
		{
			AddLeftMapQuestParts(quest, pawn);
		});
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (!slate.TryGet<Map>("map", out var _))
		{
			bool canBeSpace = CanBeSpace;
			return QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace) != null;
		}
		if (!CanBeSpace)
		{
			return Find.AnyPlayerHomeMap != null;
		}
		return Find.RandomSurfacePlayerHomeMap != null;
	}
}
