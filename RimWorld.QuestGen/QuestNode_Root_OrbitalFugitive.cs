using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_OrbitalFugitive : QuestNode
{
	protected override bool TestRunInt(Slate slate)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		return true;
	}

	protected override void RunInt()
	{
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		float a = slate.Get("points", 0f);
		Site site = slate.Get<Site>("site");
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
		string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Salvager_Elite, Faction.OfSalvagers, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true));
		slate.Set("fugitive", pawn);
		IEnumerable<Pawn> collection = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Combat,
			faction = Faction.OfSalvagers,
			tile = site.Tile,
			points = Mathf.Max(a, Faction.OfSalvagers.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat) * 1.05f)
		});
		List<Pawn> list = new List<Pawn>();
		list.Add(pawn);
		list.AddRange(collection);
		quest.AddPart(new QuestPart_SpawnPawnsInStructure(list, inSignal));
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal2);
	}
}
