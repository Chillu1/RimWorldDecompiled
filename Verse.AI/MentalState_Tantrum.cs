using RimWorld;

namespace Verse.AI
{
	public abstract class MentalState_Tantrum : MentalState
	{
		public Thing target;

		protected bool hitTargetAtLeastOnce;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref target, "target");
			Scribe_Values.Look(ref hitTargetAtLeastOnce, "hitTargetAtLeastOnce", defaultValue: false);
		}

		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}

		public override void Notify_AttackedTarget(LocalTargetInfo hitTarget)
		{
			base.Notify_AttackedTarget(hitTarget);
			if (target != null && hitTarget.Thing == target)
			{
				hitTargetAtLeastOnce = true;
			}
		}
	}
}
