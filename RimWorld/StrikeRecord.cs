using Verse;

namespace RimWorld
{
	internal struct StrikeRecord : IExposable
	{
		public IntVec3 cell;

		public int ticksGame;

		public ThingDef def;

		private const int StrikeRecordExpiryDays = 15;

		public bool Expired => Find.TickManager.TicksGame > ticksGame + 900000;

		public void ExposeData()
		{
			Scribe_Values.Look(ref cell, "cell");
			Scribe_Values.Look(ref ticksGame, "ticksGame", 0);
			Scribe_Defs.Look(ref def, "def");
		}

		public override string ToString()
		{
			return "(" + cell + ", " + def + ", " + ticksGame + ")";
		}
	}
}
