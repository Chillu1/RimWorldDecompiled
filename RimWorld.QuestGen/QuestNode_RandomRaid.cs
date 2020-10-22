using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_RandomRaid : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<bool> useCurrentThreatPoints;

		private static readonly FloatRange RaidPointsRandomFactor = new FloatRange(0.9f, 1.1f);

		protected override bool TestRunInt(Slate slate)
		{
			if (!Find.Storyteller.difficultyValues.allowViolentQuests)
			{
				return false;
			}
			if (!slate.Exists("map"))
			{
				return false;
			}
			if (!slate.Exists("enemyFaction"))
			{
				return false;
			}
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			Map map = QuestGen.slate.Get<Map>("map");
			float val = QuestGen.slate.Get("points", 0f);
			Faction faction = QuestGen.slate.Get<Faction>("enemyFaction");
			QuestPart_RandomRaid questPart_RandomRaid = new QuestPart_RandomRaid();
			questPart_RandomRaid.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_RandomRaid.mapParent = map.Parent;
			questPart_RandomRaid.faction = faction;
			questPart_RandomRaid.pointsRange = RaidPointsRandomFactor * val;
			questPart_RandomRaid.useCurrentThreatPoints = useCurrentThreatPoints.GetValue(slate);
			QuestGen.quest.AddPart(questPart_RandomRaid);
		}
	}
}
