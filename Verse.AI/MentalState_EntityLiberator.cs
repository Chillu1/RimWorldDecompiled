using RimWorld;

namespace Verse.AI;

public class MentalState_EntityLiberator : MentalState
{
	public override RandomSocialMode SocialModeMax()
	{
		return RandomSocialMode.Off;
	}

	public override void Notify_ReleasedTarget()
	{
		RecoverFromState();
	}
}
