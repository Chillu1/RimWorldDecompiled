using Verse;

namespace RimWorld
{
	public abstract class CompTargetEffect : ThingComp
	{
		public abstract void DoEffectOn(Pawn user, Thing target);
	}
}
