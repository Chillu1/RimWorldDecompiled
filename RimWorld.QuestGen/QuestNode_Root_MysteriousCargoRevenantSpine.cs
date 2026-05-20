using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_MysteriousCargoRevenantSpine : QuestNode_Root_MysteriousCargo
{
	protected override bool RequiresPawn { get; }

	protected override Thing GenerateThing(Pawn _)
	{
		return ThingMaker.MakeThing(ThingDefOf.RevenantSpine);
	}
}
