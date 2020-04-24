using RimWorld;

namespace Verse.AI
{
	public abstract class MentalState_InsultingSpree : MentalState
	{
		public Pawn target;

		public bool insultedTargetAtLeastOnce;

		public int lastInsultTicks = -999999;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref target, "target");
			Scribe_Values.Look(ref insultedTargetAtLeastOnce, "insultedTargetAtLeastOnce", defaultValue: false);
			Scribe_Values.Look(ref lastInsultTicks, "lastInsultTicks", 0);
		}

		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
