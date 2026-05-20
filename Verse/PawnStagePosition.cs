namespace Verse
{
	public class PawnStagePosition : IExposable
	{
		public IntVec3 cell;

		public Thing thing;

		public Rot4 orientation = Rot4.Invalid;

		public bool highlight;

		public PawnStagePosition()
		{
		}

		public PawnStagePosition(IntVec3 cell, Thing thing, Rot4 orientation, bool highlight)
		{
			this.cell = cell;
			this.thing = thing;
			this.orientation = orientation;
			this.highlight = highlight;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref cell, "cell");
			Scribe_Values.Look(ref orientation, "orientation", Rot4.Invalid);
			Scribe_Values.Look(ref highlight, "highlight", defaultValue: false);
			Scribe_References.Look(ref thing, "thing");
		}
	}
}
