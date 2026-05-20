using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_ReservePawns : QuestPart
{
	public List<Pawn> pawns = new List<Pawn>();

	public override bool QuestPartReserves(Pawn p)
	{
		return pawns.Contains(p);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
