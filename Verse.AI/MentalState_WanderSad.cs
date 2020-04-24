using RimWorld;

namespace Verse.AI
{
	public class MentalState_WanderSad : MentalState
	{
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
