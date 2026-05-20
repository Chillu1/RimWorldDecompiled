using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_MysteriousCargoUnnaturalCorpse : QuestNode_Root_MysteriousCargo
{
	protected override Thing GenerateThing(Pawn pawn)
	{
		return AnomalyUtility.MakeUnnaturalCorpse(pawn);
	}

	protected override bool ValidatePawn(Pawn pawn)
	{
		if (!base.ValidatePawn(pawn))
		{
			return false;
		}
		if (pawn.RaceProps.unnaturalCorpseDef == null)
		{
			return false;
		}
		return !Find.Anomaly.PawnHasUnnaturalCorpse(pawn);
	}

	protected override void AddPostDroppedQuestParts(Pawn pawn, Thing thing, Quest quest)
	{
		quest.PawnDestroyed(pawn, null, delegate
		{
			quest.LinkUnnaturalCorpse(pawn, thing as UnnaturalCorpse);
		});
	}
}
