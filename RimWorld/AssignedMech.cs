using Verse;

namespace RimWorld
{
	public class AssignedMech : IExposable
	{
		public Pawn pawn;

		public int tickAssigned;

		public AssignedMech()
		{
		}

		public AssignedMech(Pawn pawn)
		{
			this.pawn = pawn;
			tickAssigned = Find.TickManager.TicksGame;
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref pawn, "pawn");
			Scribe_Values.Look(ref tickAssigned, "tickAssigned", 0);
		}
	}
}
