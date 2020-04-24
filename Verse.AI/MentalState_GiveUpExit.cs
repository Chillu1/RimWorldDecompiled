using RimWorld;

namespace Verse.AI
{
	public class MentalState_GiveUpExit : MentalState
	{
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
