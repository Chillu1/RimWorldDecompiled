using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_Bombardment : CompAbilityEffect_WithDest
	{
		public new CompProperties_Bombardment Props => (CompProperties_Bombardment)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			Bombardment obj = (Bombardment)GenSpawn.Spawn(ThingDefOf.Bombardment, target.Cell, parent.pawn.Map);
			obj.duration = Props.durationTicks;
			obj.instigator = parent.pawn;
			obj.StartStrike();
		}
	}
}
