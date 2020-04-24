using RimWorld;

namespace Verse
{
	public class SubEffecter_InteractSymbol : SubEffecter
	{
		private Mote interactMote;

		public SubEffecter_InteractSymbol(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			if (interactMote == null)
			{
				interactMote = MoteMaker.MakeInteractionOverlay(def.moteDef, A, B);
			}
		}

		public override void SubCleanup()
		{
			if (interactMote != null && !interactMote.Destroyed)
			{
				interactMote.Destroy();
			}
		}
	}
}
