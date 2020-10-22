using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen
{
	public class QuestNode_Raid : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<IntVec3?> walkInSpot;

		public SlateRef<string> customLetterLabel;

		public SlateRef<string> customLetterText;

		public SlateRef<RulePack> customLetterLabelRules;

		public SlateRef<RulePack> customLetterTextRules;

		[NoTranslate]
		public SlateRef<string> inSignalLeave;

		[NoTranslate]
		public SlateRef<string> tag;

		private const string RootSymbol = "root";

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
			Map target = QuestGen.slate.Get<Map>("map");
			float a = QuestGen.slate.Get("points", 0f);
			Faction faction = QuestGen.slate.Get<Faction>("enemyFaction");
			QuestPart_Incident questPart_Incident = new QuestPart_Incident();
			questPart_Incident.debugLabel = "raid";
			questPart_Incident.incident = IncidentDefOf.RaidEnemy;
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.forced = true;
			incidentParms.target = target;
			incidentParms.points = Mathf.Max(a, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
			incidentParms.faction = faction;
			incidentParms.pawnGroupMakerSeed = Rand.Int;
			incidentParms.inSignalEnd = QuestGenUtility.HardcodedSignalWithQuestID(inSignalLeave.GetValue(slate));
			incidentParms.questTag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(tag.GetValue(slate));
			if (!customLetterLabel.GetValue(slate).NullOrEmpty() || customLetterLabelRules.GetValue(slate) != null)
			{
				QuestGen.AddTextRequest("root", delegate(string x)
				{
					incidentParms.customLetterLabel = x;
				}, QuestGenUtility.MergeRules(customLetterLabelRules.GetValue(slate), customLetterLabel.GetValue(slate), "root"));
			}
			if (!customLetterText.GetValue(slate).NullOrEmpty() || customLetterTextRules.GetValue(slate) != null)
			{
				QuestGen.AddTextRequest("root", delegate(string x)
				{
					incidentParms.customLetterText = x;
				}, QuestGenUtility.MergeRules(customLetterTextRules.GetValue(slate), customLetterText.GetValue(slate), "root"));
			}
			IncidentWorker_Raid obj = (IncidentWorker_Raid)questPart_Incident.incident.Worker;
			obj.ResolveRaidStrategy(incidentParms, PawnGroupKindDefOf.Combat);
			obj.ResolveRaidArriveMode(incidentParms);
			if (incidentParms.raidArrivalMode.walkIn)
			{
				incidentParms.spawnCenter = walkInSpot.GetValue(slate) ?? QuestGen.slate.Get<IntVec3?>("walkInSpot") ?? IntVec3.Invalid;
			}
			PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms);
			defaultPawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(defaultPawnGroupMakerParms.points, incidentParms.raidArrivalMode, incidentParms.raidStrategy, defaultPawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat);
			IEnumerable<PawnKindDef> pawnKinds = PawnGroupMakerUtility.GeneratePawnKindsExample(defaultPawnGroupMakerParms);
			questPart_Incident.SetIncidentParmsAndRemoveTarget(incidentParms);
			questPart_Incident.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			QuestGen.quest.AddPart(questPart_Incident);
			QuestGen.AddQuestDescriptionRules(new List<Rule>
			{
				new Rule_String("raidPawnKinds", PawnUtility.PawnKindsToLineList(pawnKinds, "  - ", ColoredText.ThreatColor)),
				new Rule_String("raidArrivalModeInfo", incidentParms.raidArrivalMode.textWillArrive.Formatted(faction))
			});
		}
	}
}
