using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_DropMechPods : CompAbilityEffect_WithDest
{
	private const int PodOpenDelay = 140;

	public new CompProperties_DropMechPods Props => (CompProperties_DropMechPods)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		List<Thing> list = new List<Thing>();
		int randomInRange = Props.numPods.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			Thing thing = ThingMaker.MakeThing(ThingDefOf.MechanoidDropPod);
			thing.SetFaction(Faction.OfPlayer);
			list.Add(thing);
		}
		DropPodUtility.DropThingsNear(target.Cell, parent.pawn.MapHeld, list, 140, canInstaDropDuringInit: false, leaveSlag: false, canRoofPunch: true, forbid: true, allowFogged: true, Faction.OfMechanoids);
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
