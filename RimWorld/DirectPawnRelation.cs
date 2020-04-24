using Verse;

namespace RimWorld
{
	public class DirectPawnRelation : IExposable
	{
		public PawnRelationDef def;

		public Pawn otherPawn;

		public int startTicks;

		public DirectPawnRelation()
		{
		}

		public DirectPawnRelation(PawnRelationDef def, Pawn otherPawn, int startTicks)
		{
			this.def = def;
			this.otherPawn = otherPawn;
			this.startTicks = startTicks;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_References.Look(ref otherPawn, "otherPawn", saveDestroyedThings: true);
			Scribe_Values.Look(ref startTicks, "startTicks", 0);
		}
	}
}
