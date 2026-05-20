using RimWorld;

namespace Verse.AI;

public class MentalState_Terror : MentalState
{
	public override RandomSocialMode SocialModeMax()
	{
		return RandomSocialMode.Off;
	}
}
