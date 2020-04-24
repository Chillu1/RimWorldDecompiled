using RimWorld;

namespace Verse.AI
{
	public class MentalState_WanderConfused : MentalState
	{
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
