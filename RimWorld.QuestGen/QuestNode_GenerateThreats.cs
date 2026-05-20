using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GenerateThreats : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> inSignalDisable;

	[NoTranslate]
	public SlateRef<string> storeThreatExampleAs;

	[NoTranslate]
	public SlateRef<int> threatStartTicks;

	public SlateRef<ThreatsGeneratorParams> parms;

	public SlateRef<Faction> faction;

	protected override bool TestRunInt(Slate slate)
	{
		if (!Find.Storyteller.difficulty.allowViolentQuests)
		{
			return false;
		}
		return slate.Get<Map>("map") != null;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		Map map = slate.Get<Map>("map");
		QuestPart_ThreatsGenerator questPart_ThreatsGenerator = new QuestPart_ThreatsGenerator();
		questPart_ThreatsGenerator.threatStartTicks = threatStartTicks.GetValue(slate);
		questPart_ThreatsGenerator.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? slate.Get<string>("inSignal");
		questPart_ThreatsGenerator.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate));
		ThreatsGeneratorParams value = parms.GetValue(slate);
		value.faction = faction.GetValue(slate) ?? value.faction;
		questPart_ThreatsGenerator.parms = value;
		questPart_ThreatsGenerator.mapParent = map.Parent;
		QuestGen.quest.AddPart(questPart_ThreatsGenerator);
		if (!storeThreatExampleAs.GetValue(slate).NullOrEmpty())
		{
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
			pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;
			pawnGroupMakerParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
			pawnGroupMakerParms.faction = value.faction ?? (from x in Find.FactionManager.GetFactions(allowHidden: false, allowDefeated: false, allowNonHumanlike: true, TechLevel.Industrial)
				where x.HostileTo(Faction.OfPlayer)
				select x).RandomElement();
			float num = value.threatPoints ?? (StorytellerUtility.DefaultThreatPointsNow(map) * value.currentThreatPointsFactor);
			if (value.minThreatPoints.HasValue)
			{
				num = Mathf.Max(num, value.minThreatPoints.Value);
			}
			pawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(num, PawnsArrivalModeDefOf.EdgeWalkIn, RaidStrategyDefOf.ImmediateAttack, pawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat, map);
			IEnumerable<PawnKindDef> pawnKinds = PawnGroupMakerUtility.GeneratePawnKindsExample(pawnGroupMakerParms);
			slate.Set(storeThreatExampleAs.GetValue(slate), PawnUtility.PawnKindsToLineList(pawnKinds, "  - ", ColoredText.ThreatColor));
		}
	}
}
