using RimWorld;

namespace Verse
{
	public class SubEffecter_ProgressBar : SubEffecter
	{
		public MoteProgressBar mote;

		private const float Width = 0.68f;

		private const float Height = 0.12f;

		public SubEffecter_ProgressBar(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			if (mote == null)
			{
				mote = (MoteProgressBar)MoteMaker.MakeInteractionOverlay(def.moteDef, A, B);
				mote.exactScale.x = 0.68f;
				mote.exactScale.z = 0.12f;
			}
		}

		public override void SubCleanup()
		{
			if (mote != null && !mote.Destroyed)
			{
				mote.Destroy();
			}
		}
	}
}
