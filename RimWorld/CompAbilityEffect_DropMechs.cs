using System.Linq;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_DropMechs : CompAbilityEffect_WithDest
{
	private const int PodOpenDelay = 140;

	public new CompProperties_DropMechs Props => (CompProperties_DropMechs)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		PawnGroupMakerParms parms = new PawnGroupMakerParms
		{
			faction = Faction.OfMechanoids,
			groupKind = PawnGroupKindDefOf.Combat,
			points = Props.points.RandomInRange,
			tile = parent.pawn.MapHeld.Tile
		};
		DropPodUtility.DropThingsNear(target.Cell, parent.pawn.MapHeld, PawnGroupMakerUtility.GeneratePawns(parms).ToList(), 140, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: true, forbid: true, allowFogged: true, Faction.OfMechanoids);
	}

	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (!ModsConfig.OdysseyActive || Faction.OfMechanoids == null || Faction.OfMechanoids.deactivated)
		{
			return false;
		}
		return base.CanApplyOn(target, dest);
	}
}
