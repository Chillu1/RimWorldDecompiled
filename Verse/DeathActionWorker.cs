using RimWorld;
using Verse.AI.Group;

namespace Verse;

public abstract class DeathActionWorker
{
	public DeathActionProperties props;

	public virtual RulePackDef DeathRules => RulePackDefOf.Transition_Died;

	public virtual bool DangerousInMelee => false;

	public abstract void PawnDied(Corpse corpse, Lord prevLord);
}
