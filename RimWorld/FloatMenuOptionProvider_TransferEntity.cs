using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_TransferEntity : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return ModsConfig.AnomalyActive;
	}

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
	{
		if (!clickedThing.TryGetComp(out CompEntityHolder comp) || comp.HeldPawn == null || !context.FirstSelectedPawn.CanReserveAndReach(clickedThing, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true))
		{
			yield break;
		}
		Pawn heldPawn = comp.HeldPawn;
		if (GenClosest.ClosestThing_Global_Reachable(context.FirstSelectedPawn.Position, context.FirstSelectedPawn.Map, context.FirstSelectedPawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_HoldingPlatform>(), PathEndMode.ClosestTouch, TraverseParms.For(context.FirstSelectedPawn, Danger.Some), 9999f, delegate(Thing b)
		{
			if (!(b is Building_HoldingPlatform building_HoldingPlatform))
			{
				return false;
			}
			if (building_HoldingPlatform.Occupied)
			{
				return false;
			}
			return context.FirstSelectedPawn.CanReserve(building_HoldingPlatform) ? true : false;
		}, delegate(Thing t)
		{
			CompEntityHolder compEntityHolder = t.TryGetComp<CompEntityHolder>();
			return (compEntityHolder != null && compEntityHolder.ContainmentStrength >= heldPawn.GetStatValue(StatDefOf.MinimumContainmentStrength)) ? (compEntityHolder.ContainmentStrength / Mathf.Max(heldPawn.PositionHeld.DistanceTo(t.Position), 1f)) : 0f;
		}) != null)
		{
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("TransferEntity".Translate(heldPawn) + " (" + "ChooseEntityHolder".Translate() + "...)", delegate
			{
				StudyUtility.TargetHoldingPlatformForEntity(context.FirstSelectedPawn, heldPawn, transferBetweenPlatforms: true, clickedThing);
			}), context.FirstSelectedPawn, clickedThing);
		}
	}
}
