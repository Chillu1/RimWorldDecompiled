using Verse;

namespace RimWorld;

public static class HuntJobUtility
{
	public static bool WasKilledByHunter(Pawn pawn, DamageInfo? dinfo)
	{
		if (!dinfo.HasValue)
		{
			return false;
		}
		if (!(dinfo.Value.Instigator is Pawn { CurJob: not null } pawn2))
		{
			return false;
		}
		if (pawn2.jobs.curDriver is JobDriver_Hunt jobDriver_Hunt)
		{
			return jobDriver_Hunt.Victim == pawn;
		}
		return false;
	}
}
