using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_RandomRaid : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<Faction> faction;

	public SlateRef<bool> useCurrentThreatPoints;

	public SlateRef<float?> currentThreatPointsFactor;

	public SlateRef<PawnsArrivalModeDef> arrivalMode;

	public SlateRef<string> customLetterLabel;

	public SlateRef<string> customLetterText;

	public SlateRef<RulePack> customLetterLabelRules;

	public SlateRef<RulePack> customLetterTextRules;

	private const string RootSymbol = "root";

	private static readonly FloatRange RaidPointsRandomFactor = new FloatRange(0.9f, 1.1f);

	protected override bool TestRunInt(Slate slate)
	{
		if (!Find.Storyteller.difficulty.allowViolentQuests)
		{
			return false;
		}
		if (!slate.Exists("map"))
		{
			return false;
		}
		if (!slate.Exists("enemyFaction") && faction.GetValue(slate) == null)
		{
			return false;
		}
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		Map map = QuestGen.slate.Get<Map>("map");
		float num = QuestGen.slate.Get("points", 0f);
		Faction faction = this.faction.GetValue(slate) ?? QuestGen.slate.Get<Faction>("enemyFaction");
		QuestPart_RandomRaid randomRaid = new QuestPart_RandomRaid();
		randomRaid.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		randomRaid.mapParent = map.Parent;
		randomRaid.faction = faction;
		randomRaid.pointsRange = RaidPointsRandomFactor * num;
		randomRaid.useCurrentThreatPoints = useCurrentThreatPoints.GetValue(slate);
		randomRaid.currentThreatPointsFactor = currentThreatPointsFactor.GetValue(slate) ?? 1f;
		if (arrivalMode.GetValue(slate) != null)
		{
			randomRaid.arrivalMode = arrivalMode.GetValue(slate);
		}
		if (!customLetterLabel.GetValue(slate).NullOrEmpty() || customLetterLabelRules.GetValue(slate) != null)
		{
			QuestGen.AddTextRequest("root", delegate(string x)
			{
				randomRaid.customLetterLabel = x;
			}, QuestGenUtility.MergeRules(customLetterLabelRules.GetValue(slate), customLetterLabel.GetValue(slate), "root"));
		}
		if (!customLetterText.GetValue(slate).NullOrEmpty() || customLetterTextRules.GetValue(slate) != null)
		{
			QuestGen.AddTextRequest("root", delegate(string x)
			{
				randomRaid.customLetterText = x;
			}, QuestGenUtility.MergeRules(customLetterTextRules.GetValue(slate), customLetterText.GetValue(slate), "root"));
		}
		QuestGen.quest.AddPart(randomRaid);
	}
}
