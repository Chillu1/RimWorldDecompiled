namespace Verse
{
	public class Stance_WarmupAbilityWorld : Stance_Warmup
	{
		public Stance_WarmupAbilityWorld()
		{
		}

		public Stance_WarmupAbilityWorld(int ticks, LocalTargetInfo focusTarg, Verb verb)
			: base(ticks, focusTarg, verb)
		{
		}

		protected override void Expire()
		{
			effecter?.Cleanup();
			if (stanceTracker.curStance == this)
			{
				stanceTracker.SetStance(new Stance_Mobile());
			}
		}
	}
}
