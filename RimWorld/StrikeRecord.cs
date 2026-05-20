using Verse;

namespace RimWorld;

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
		string[] obj = new string[7] { "(", null, null, null, null, null, null };
		IntVec3 intVec = cell;
		obj[1] = intVec.ToString();
		obj[2] = ", ";
		obj[3] = def?.ToString();
		obj[4] = ", ";
		obj[5] = ticksGame.ToString();
		obj[6] = ")";
		return string.Concat(obj);
	}
}
