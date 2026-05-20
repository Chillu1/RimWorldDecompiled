using Verse;

namespace RimWorld;

public class QuestPart_TransporterPawns_Feed : QuestPart_TransporterPawns
{
	public override void Process(Pawn pawn)
	{
		if (pawn.needs?.food != null)
		{
			pawn.needs.food.CurLevel = pawn.needs.food.MaxLevel;
		}
	}
}
