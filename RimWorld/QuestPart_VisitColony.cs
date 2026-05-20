using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class QuestPart_VisitColony : QuestPart_MakeLord
	{
		public int? durationTicks;

		protected override Lord MakeLord()
		{
			if (!RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0], out var result))
			{
				result = CellFinder.RandomCell(pawns[0].Map);
			}
			LordJob_VisitColony lordJob = new LordJob_VisitColony(faction ?? pawns[0].Faction, result, durationTicks);
			return LordMaker.MakeNewLord(faction ?? pawns[0].Faction, lordJob, mapParent.Map);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref durationTicks, "durationTicks");
		}
	}
}
