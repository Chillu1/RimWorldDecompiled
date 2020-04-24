using RimWorld;

namespace Verse
{
	public abstract class DeathActionWorker
	{
		public virtual RulePackDef DeathRules => RulePackDefOf.Transition_Died;

		public virtual bool DangerousInMelee => false;

		public abstract void PawnDied(Corpse corpse);
	}
}
