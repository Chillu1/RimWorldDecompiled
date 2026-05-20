using RimWorld;

namespace Verse.AI;

public class MentalState_CocoonDisturbed : MentalState
{
	public override bool ForceHostileTo(Thing t)
	{
		if (t is Pawn pawn)
		{
			if (pawn.RaceProps.Insect)
			{
				return false;
			}
			if (pawn.RaceProps.Animal && (pawn.Roamer || pawn.Faction == null))
			{
				return false;
			}
		}
		return true;
	}

	public override bool ForceHostileTo(Faction f)
	{
		return true;
	}

	public override RandomSocialMode SocialModeMax()
	{
		return RandomSocialMode.Off;
	}
}
