using Verse;

namespace RimWorld;

public class CompAbilityEffect_DropMechCluster : CompAbilityEffect_WithDest
{
	public new CompProperties_DropMechCluster Props => (CompProperties_DropMechCluster)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		MechClusterSketch sketch = MechClusterGenerator.GenerateClusterSketch(Props.points, parent.pawn.Map, Props.startDormant);
		MechClusterUtility.SpawnCluster(target.Cell, parent.pawn.Map, sketch);
	}

	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (!ModsConfig.RoyaltyActive || Faction.OfMechanoids == null || Faction.OfMechanoids.deactivated)
		{
			return false;
		}
		return base.CanApplyOn(target, dest);
	}
}
