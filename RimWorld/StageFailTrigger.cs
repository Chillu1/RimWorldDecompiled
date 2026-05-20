using Verse;

namespace RimWorld
{
	public abstract class StageFailTrigger : IExposable
	{
		[MustTranslate]
		public string desc;

		public int allowanceTicks;

		public string Reason(LordJob_Ritual ritual, TargetInfo spot)
		{
			return desc;
		}

		public abstract bool Failed(LordJob_Ritual ritual, TargetInfo spot, TargetInfo focus);

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref desc, "desc");
			Scribe_Values.Look(ref allowanceTicks, "allowanceTicks", 0);
		}
	}
}
