using RimWorld;

namespace Verse.AI;

public class MentalState_BerserkWarcall : MentalState
{
	public override bool ForceHostileTo(Thing t)
	{
		if (sourceFaction == null)
		{
			return t.HostileTo(pawn);
		}
		return t.HostileTo(sourceFaction);
	}

	public override bool ForceHostileTo(Faction f)
	{
		return f.HostileTo(sourceFaction);
	}

	public override RandomSocialMode SocialModeMax()
	{
		return RandomSocialMode.Off;
	}
}
