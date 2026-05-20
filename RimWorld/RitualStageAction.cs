using Verse;

namespace RimWorld
{
	public abstract class RitualStageAction : IExposable
	{
		public virtual void Apply(LordJob_Ritual ritual)
		{
		}

		public virtual void ApplyToPawn(LordJob_Ritual ritual, Pawn pawn)
		{
		}

		public abstract void ExposeData();
	}
}
