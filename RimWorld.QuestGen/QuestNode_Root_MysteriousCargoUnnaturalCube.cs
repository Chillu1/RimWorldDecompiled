using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_MysteriousCargoUnnaturalCube : QuestNode_Root_MysteriousCargo
{
	protected override Thing GenerateThing(Pawn pawn)
	{
		return ThingMaker.MakeThing(ThingDefOf.GoldenCube);
	}

	protected override void AddPostDroppedQuestParts(Pawn pawn, Thing thing, Quest quest)
	{
		quest.PawnDestroyed(pawn, null, delegate
		{
			quest.GiveHediff(pawn, HediffDefOf.CubeInterest);
		});
	}

	protected override bool ValidatePawn(Pawn pawn)
	{
		if (!base.ValidatePawn(pawn))
		{
			return false;
		}
		if (!pawn.health.hediffSet.HasHediff(HediffDefOf.CubeInterest))
		{
			return !pawn.health.hediffSet.HasHediff(HediffDefOf.CubeComa);
		}
		return false;
	}
}
