using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen;

public class QuestNode_Raid : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	public SlateRef<IntVec3?> walkInSpot;

	public SlateRef<IntVec3?> dropSpot;

	public SlateRef<string> customLetterLabel;

	public SlateRef<string> customLetterText;

	public SlateRef<RulePack> customLetterLabelRules;

	public SlateRef<RulePack> customLetterTextRules;

	public SlateRef<PawnsArrivalModeDef> arrivalMode;

	public SlateRef<PawnKindDef> raidPawnKind;

	public SlateRef<bool?> canTimeoutOrFlee;

	[NoTranslate]
	public SlateRef<string> inSignalLeave;

	[NoTranslate]
	public SlateRef<string> tag;

	private const string RootSymbol = "root";

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
		float points = QuestGen.slate.Get("points", 0f);
		Faction faction = QuestGen.slate.Get<Faction>("enemyFaction");
		QuestPart_Incident questPart_Incident = new QuestPart_Incident();
		questPart_Incident.debugLabel = "raid";
		questPart_Incident.incident = IncidentDefOf.RaidEnemy;
		int num = 0;
		IncidentParms incidentParms;
		PawnGroupMakerParms defaultPawnGroupMakerParms;
		IEnumerable<PawnKindDef> enumerable;
		do
		{
			incidentParms = GenerateIncidentParms(map, points, faction, slate, questPart_Incident);
			defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, ensureCanGenerateAtLeastOnePawn: true);
			defaultPawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(defaultPawnGroupMakerParms.points, incidentParms.raidArrivalMode, incidentParms.raidStrategy, defaultPawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat, incidentParms.target);
			enumerable = PawnGroupMakerUtility.GeneratePawnKindsExample(defaultPawnGroupMakerParms);
			num++;
		}
		while (!enumerable.Any() && num < 50);
		if (!enumerable.Any())
		{
			Log.Error("No pawnkinds example for " + QuestGen.quest.root.defName + " parms=" + defaultPawnGroupMakerParms?.ToString() + " iterations=" + num);
		}
		questPart_Incident.SetIncidentParmsAndRemoveTarget(incidentParms);
		questPart_Incident.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		QuestGen.quest.AddPart(questPart_Incident);
		QuestGen.AddQuestDescriptionRules(new List<Rule>
		{
			new Rule_String("raidPawnKinds", PawnUtility.PawnKindsToLineList(enumerable, "  - ", ColoredText.ThreatColor)),
			new Rule_String("raidArrivalModeInfo", incidentParms.raidArrivalMode.textWillArrive.Formatted(faction))
		});
	}

	private IncidentParms GenerateIncidentParms(Map map, float points, Faction faction, Slate slate, QuestPart_Incident questPart)
	{
		IncidentParms incidentParms = new IncidentParms
		{
			forced = true,
			target = map,
			points = Mathf.Max(points, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat)),
			faction = faction,
			pawnGroupMakerSeed = Rand.Int,
			inSignalEnd = QuestGenUtility.HardcodedSignalWithQuestID(inSignalLeave.GetValue(slate)),
			questTag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(tag.GetValue(slate)),
			canTimeoutOrFlee = (map.CanEverExit && (canTimeoutOrFlee.GetValue(slate) ?? true))
		};
		if (raidPawnKind.GetValue(slate) != null)
		{
			incidentParms.pawnKind = raidPawnKind.GetValue(slate);
			incidentParms.pawnCount = Mathf.Max(1, Mathf.RoundToInt(incidentParms.points / incidentParms.pawnKind.combatPower));
		}
		if (arrivalMode.GetValue(slate) != null)
		{
			incidentParms.raidArrivalMode = arrivalMode.GetValue(slate);
		}
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
		IncidentWorker_Raid obj = (IncidentWorker_Raid)questPart.incident.Worker;
		obj.ResolveRaidStrategy(incidentParms, PawnGroupKindDefOf.Combat);
		obj.ResolveRaidArriveMode(incidentParms);
		obj.ResolveRaidAgeRestriction(incidentParms);
		if (incidentParms.raidArrivalMode.walkIn)
		{
			incidentParms.spawnCenter = walkInSpot.GetValue(slate) ?? QuestGen.slate.Get<IntVec3?>("walkInSpot") ?? IntVec3.Invalid;
		}
		else
		{
			incidentParms.spawnCenter = dropSpot.GetValue(slate) ?? QuestGen.slate.Get<IntVec3?>("dropSpot") ?? IntVec3.Invalid;
		}
		return incidentParms;
	}
}
