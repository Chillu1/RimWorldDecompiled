using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_PowerBeam : CompAbilityEffect_WithDest
	{
		public new CompProperties_PowerBeam Props => (CompProperties_PowerBeam)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			PowerBeam obj = (PowerBeam)GenSpawn.Spawn(ThingDefOf.PowerBeam, target.Cell, parent.pawn.Map);
			obj.duration = Props.durationTicks;
			obj.instigator = parent.pawn;
			obj.StartStrike();
		}
	}
}
