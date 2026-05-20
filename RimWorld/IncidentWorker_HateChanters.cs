using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class IncidentWorker_HateChanters : IncidentWorker
{
	public const int SmallGroupSize = 20;

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		parms.faction = Faction.OfHoraxCult;
		parms.sendLetter = false;
		parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkInHateChanters;
		PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(parms.pawnGroupKind ?? PawnGroupKindDefOf.PsychicRitualSiege, parms);
		float num = Faction.OfHoraxCult.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat);
		if (parms.points < num)
		{
			parms.points = (defaultPawnGroupMakerParms.points = num * 2f);
		}
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
		LordJob_HateChant lordJob_HateChant = new LordJob_HateChant();
		lordJob_HateChant.smallGroup = list.Count < 20;
		parms.raidArrivalMode.Worker.Arrive(list, parms);
		Lord lord = LordMaker.MakeNewLord(Faction.OfHoraxCult, lordJob_HateChant, (Map)parms.target, list);
		TaggedString letterText = "HateChantersText".Translate();
		if (AnomalyIncidentUtility.IncidentShardChance(parms.points))
		{
			AnomalyIncidentUtility.PawnShardOnDeath(list.RandomElement());
		}
		TaggedString letterLabel = "HateChantersLabel".Translate();
		TaggedString taggedString = "LetterRelatedPawnsRaidEnemy".Translate(Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
		PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref letterLabel, ref letterText, taggedString, informEvenIfSeenBefore: true);
		Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.ThreatBig, lord.ownedPawns);
		return true;
	}
}
