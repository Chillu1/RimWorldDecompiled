using Verse;

namespace RimWorld;

public interface ICompAbilityEffectOnJumpCompleted
{
	void OnJumpCompleted(IntVec3 origin, LocalTargetInfo target);
}
