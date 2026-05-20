using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_SurveyScanner : QuestNode
{
	private SlateRef<WorldObject> site;

	private SlateRef<int> duration;

	private SlateRef<float> raidChance;

	private SlateRef<FloatRange> raidAttackRemainingHoursRange;

	private string raidLetterLabel;

	private string raidLetterText;

	private const string ReasonSymbol = "attackReason";

	private const string SiteAlias = "site";

	private const string SurveyCompleted = "SurveyCompleted";

	private const string SiteRaid = "Raid";

	private static readonly FloatRange RaidPointsRandomFactor = new FloatRange(0.9f, 1.1f);

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		Quest quest = QuestGen.quest;
		float num = QuestGen.slate.Get("points", 0f);
		string text = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("scanner");
		string inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
		string text2 = QuestGenUtility.HardcodedSignalWithQuestID(text + ".Destroyed");
		string completedSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.SurveyCompleted");
		string text3 = QuestGenUtility.HardcodedSignalWithQuestID("site.Raid");
		int value = duration.GetValue(slate);
		Site site = (Site)this.site.GetValue(slate);
		int fireRaidRemainingTicks = (int)(raidAttackRemainingHoursRange.GetValue(slate).RandomInRange * 2500f);
		if (Rand.Chance(raidChance.GetValue(slate)) && Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out var faction, tryMedievalOrBetter: true, allowDefeated: false, TechLevel.Undefined, TechLevel.Undefined, allowTemporary: false, requireHostile: true))
		{
			slate.Set("enemyFaction", faction);
			quest.Raid(site, RaidPointsRandomFactor.RandomInRange * num, faction, null, inSignal: text3, raidStrategy: RaidStrategyDefOf.ImmediateAttack, customLetterLabel: raidLetterLabel, customLetterText: raidLetterText);
		}
		quest.AddPart(new QuestPart_SurveyScanner(site, inSignalEnable, text, value));
		quest.AddPart(new QuestPart_ScannerDurationRemainingAlert(site, inSignalEnable, text2, completedSignal, text3, value, fireRaidRemainingTicks));
		quest.End(QuestEndOutcome.Fail, 0, null, text2, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		return true;
	}
}
