using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetExampleRaid : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<Faction> faction;

	public SlateRef<float> points;

	protected override bool TestRunInt(Slate slate)
	{
		return slate.Exists("map");
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		Map target = slate.Get<Map>("map");
		PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Combat,
			raidStrategy = RaidStrategyDefOf.ImmediateAttack,
			faction = (faction.GetValue(slate) ?? (from x in Find.FactionManager.GetFactions(allowHidden: false, allowDefeated: false, allowNonHumanlike: true, TechLevel.Industrial)
				where x.HostileTo(Faction.OfPlayer)
				select x).RandomElement())
		};
		pawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(points.GetValue(slate), PawnsArrivalModeDefOf.EdgeWalkIn, RaidStrategyDefOf.ImmediateAttack, pawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat, target);
		IEnumerable<PawnKindDef> pawnKinds = PawnGroupMakerUtility.GeneratePawnKindsExample(pawnGroupMakerParms);
		slate.Set(storeAs.GetValue(slate), PawnUtility.PawnKindsToLineList(pawnKinds, "  - ", ColoredText.ThreatColor));
	}
}
