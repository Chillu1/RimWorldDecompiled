using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen
{
	public static class QuestGen_Threat
	{
		public static void Raid(this Quest quest, Map map, float points, Faction faction, string inSignalLeave = null, string customLetterLabel = null, string customLetterText = null, RulePack customLetterLabelRules = null, RulePack customLetterTextRules = null, IntVec3? walkInSpot = null, string tag = null, string inSignal = null, string rootSymbol = "root", PawnsArrivalModeDef raidArrivalMode = null)
		{
			QuestPart_Incident questPart_Incident = new QuestPart_Incident();
			questPart_Incident.debugLabel = "raid";
			questPart_Incident.incident = IncidentDefOf.RaidEnemy;
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.forced = true;
			incidentParms.target = map;
			incidentParms.points = Mathf.Max(points, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
			incidentParms.faction = faction;
			incidentParms.pawnGroupMakerSeed = Rand.Int;
			incidentParms.inSignalEnd = inSignalLeave;
			incidentParms.questTag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(tag);
			incidentParms.raidArrivalMode = raidArrivalMode;
			if (!customLetterLabel.NullOrEmpty() || customLetterLabelRules != null)
			{
				QuestGen.AddTextRequest(rootSymbol, delegate(string x)
				{
					incidentParms.customLetterLabel = x;
				}, QuestGenUtility.MergeRules(customLetterLabelRules, customLetterLabel, rootSymbol));
			}
			if (!customLetterText.NullOrEmpty() || customLetterTextRules != null)
			{
				QuestGen.AddTextRequest(rootSymbol, delegate(string x)
				{
					incidentParms.customLetterText = x;
				}, QuestGenUtility.MergeRules(customLetterTextRules, customLetterText, rootSymbol));
			}
			IncidentWorker_Raid obj = (IncidentWorker_Raid)questPart_Incident.incident.Worker;
			obj.ResolveRaidStrategy(incidentParms, PawnGroupKindDefOf.Combat);
			obj.ResolveRaidArriveMode(incidentParms);
			if (incidentParms.raidArrivalMode.walkIn)
			{
				incidentParms.spawnCenter = walkInSpot ?? QuestGen.slate.Get<IntVec3?>("walkInSpot") ?? IntVec3.Invalid;
			}
			PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms);
			defaultPawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(defaultPawnGroupMakerParms.points, incidentParms.raidArrivalMode, incidentParms.raidStrategy, defaultPawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat);
			IEnumerable<PawnKindDef> pawnKinds = PawnGroupMakerUtility.GeneratePawnKindsExample(defaultPawnGroupMakerParms);
			questPart_Incident.SetIncidentParmsAndRemoveTarget(incidentParms);
			questPart_Incident.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			QuestGen.quest.AddPart(questPart_Incident);
			QuestGen.AddQuestDescriptionRules(new List<Rule>
			{
				new Rule_String("raidPawnKinds", PawnUtility.PawnKindsToLineList(pawnKinds, "  - ", ColoredText.ThreatColor)),
				new Rule_String("raidArrivalModeInfo", incidentParms.raidArrivalMode.textWillArrive.Formatted(faction))
			});
		}
	}
}
