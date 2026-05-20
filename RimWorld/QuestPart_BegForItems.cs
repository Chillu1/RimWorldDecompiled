using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class QuestPart_BegForItems : QuestPart_MakeLord
	{
		public Pawn target;

		public ThingDef thingDef;

		public int amount;

		public string outSignalItemsReceived;

		protected override Lord MakeLord()
		{
			if (!RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0], out var result))
			{
				result = CellFinder.RandomCell(pawns[0].Map);
			}
			return LordMaker.MakeNewLord(faction, new LordJob_BegForItems(faction, result, target, thingDef, amount, outSignalItemsReceived), base.Map);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref target, "target");
			Scribe_Defs.Look(ref thingDef, "thingDef");
			Scribe_Values.Look(ref amount, "amount", 0);
		}
	}
}
