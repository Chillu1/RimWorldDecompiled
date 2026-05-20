using Verse;

namespace RimWorld.Planet;

public static class CaravanBedUtility
{
	public static bool InCaravanBed(this Pawn p)
	{
		return p.CurrentCaravanBed() != null;
	}

	public static Building_Bed CurrentCaravanBed(this Pawn p)
	{
		return p.GetCaravan()?.beds.GetBedUsedBy(p);
	}

	public static bool WouldBenefitFromRestingInBed(Pawn p)
	{
		if (!p.Dead)
		{
			return p.health.hediffSet.HasImmunizableNotImmuneHediff();
		}
		return false;
	}

	public static string AppendUsingBedsLabel(string str, int bedCount)
	{
		string text = ((bedCount == 1) ? ((string)"UsingBedroll".Translate()) : ((string)"UsingBedrolls".Translate(bedCount)));
		return str + " (" + text + ")";
	}
}
