using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_PollutionRaid : QuestNode
{
	private static readonly IntRange MaxDelayTicksRange = new IntRange(60000, 180000);

	private const float PointModifier = 1.5f;

	protected override bool TestRunInt(Slate slate)
	{
		if (QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace: true) != null && slate.TryGet<Faction>("enemyFaction", out var var))
		{
			return IsValidFaction(var);
		}
		return false;
	}

	protected override void RunInt()
	{
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = slate.Get<Map>("map");
		Faction faction = slate.Get<Faction>("enemyFaction");
		float points = slate.Get("points", 0f) * 1.5f;
		string completeSignal = QuestGen.GenerateNewSignal("DelayCompleted");
		quest.Delay(MaxDelayTicksRange.RandomInRange, delegate
		{
			quest.SignalPass(null, null, completeSignal);
		}, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, null, null, null, tickHistorically: false, QuestPart.SignalListenMode.OngoingOnly, waitUntilPlayerHasHomeMap: true).debugLabel = "Arrival delay";
		Quest quest2 = quest;
		MapParent parent = map.Parent;
		string inSignal = completeSignal;
		quest2.Raid(parent, points, faction, null, "[retaliationLetterLabel]", "[retaliationLetterText]", null, null, null, null, inSignal);
		quest.End(QuestEndOutcome.Unknown, 0, null, completeSignal);
		slate.Set("enemyFaction", faction);
	}

	private static bool IsValidFaction(Faction faction)
	{
		if (!faction.IsPlayer)
		{
			return faction.HostileTo(Faction.OfPlayer);
		}
		return false;
	}
}
