namespace Verse
{
	public class SubEffecter
	{
		public Effecter parent;

		public SubEffecterDef def;

		public SubEffecter(SubEffecterDef subDef, Effecter parent)
		{
			def = subDef;
			this.parent = parent;
		}

		public virtual void SubEffectTick(TargetInfo A, TargetInfo B)
		{
		}

		public virtual void SubTrigger(TargetInfo A, TargetInfo B)
		{
		}

		public virtual void SubCleanup()
		{
		}
	}
}
