using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_OnlyTargetHostiles : CompAbilityEffect
{
	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		if (parent.pawn != null)
		{
			return parent.pawn.HostileTo(target.Thing);
		}
		return false;
	}

	public override bool Valid(GlobalTargetInfo target, bool throwMessages = false)
	{
		if (parent.pawn != null)
		{
			return parent.pawn.HostileTo(target.Thing);
		}
		return false;
	}

	public override bool AICanTargetNow(LocalTargetInfo target)
	{
		if (parent.pawn != null)
		{
			return parent.pawn.HostileTo(target.Thing);
		}
		return false;
	}
}
