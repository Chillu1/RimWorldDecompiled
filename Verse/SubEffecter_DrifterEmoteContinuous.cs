namespace Verse
{
	public class SubEffecter_DrifterEmoteContinuous : SubEffecter_DrifterEmote
	{
		private int ticksUntilMote;

		public SubEffecter_DrifterEmoteContinuous(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			ticksUntilMote--;
			if (ticksUntilMote <= 0)
			{
				MakeMote(A);
				ticksUntilMote = def.ticksBetweenMotes;
			}
		}
	}
}
