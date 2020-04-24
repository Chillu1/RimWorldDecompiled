using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_DisableRandomMoodCausedMentalBreaks : QuestPartActivable
	{
		public List<Pawn> pawns = new List<Pawn>();

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void ReplacePawnReferences(Pawn replace, Pawn with)
		{
			pawns.Replace(replace, with);
		}
	}
}
