using RimWorld;

namespace Verse.AI;

public class MentalState_CubeSculpting : MentalState
{
	public override RandomSocialMode SocialModeMax()
	{
		return RandomSocialMode.Off;
	}
}
