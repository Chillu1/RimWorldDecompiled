using RimWorld;

namespace Verse.AI
{
	public class MentalState_Berserk : MentalState
	{
		public override bool ForceHostileTo(Thing t)
		{
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
}
