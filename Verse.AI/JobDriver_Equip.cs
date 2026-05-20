using System.Collections.Generic;
using RimWorld;
using Verse.Sound;

namespace Verse.AI;

public class JobDriver_Equip : JobDriver
{
	private const TargetIndex EquippableIndex = TargetIndex.A;

	private const TargetIndex OutfitStandIndex = TargetIndex.B;

	private Thing Target => job.GetTarget(TargetIndex.A).Thing;

	private bool TargetIsOnOutfitStand
	{
		get
		{
			Thing target = Target;
			if (target != null && !target.Spawned)
			{
				return target.ParentHolder is Building_OutfitStand;
			}
			return false;
		}
	}

	private Building_OutfitStand OutfitStand => (Building_OutfitStand)job.GetTarget(TargetIndex.B).Thing;

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		if (TargetIsOnOutfitStand)
		{
			job.targetB = (Building_OutfitStand)Target.ParentHolder;
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (EquipmentUtility.AlreadyBondedToWeapon(Target, pawn))
		{
			return false;
		}
		int maxPawns = 1;
		int stackCount = -1;
		_ = TargetIsOnOutfitStand;
		if (job.targetA.HasThing && job.targetA.Thing.Spawned && job.targetA.Thing.def.IsIngestible)
		{
			maxPawns = 10;
			stackCount = 1;
		}
		if (TargetIsOnOutfitStand)
		{
			return pawn.Reserve((Building_OutfitStand)job.targetA.Thing.ParentHolder, job, maxPawns, stackCount, null, errorOnFailed);
		}
		return pawn.Reserve(job.targetA, job, maxPawns, stackCount, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOnBurningImmobile(TargetIndex.A);
		yield return Toils_General.Do(delegate
		{
			pawn.mindState.droppedWeapon = null;
		});
		bool targetOnStand = TargetIsOnOutfitStand;
		Toil f = Toils_Goto.GotoThing((!targetOnStand) ? TargetIndex.A : TargetIndex.B, PathEndMode.ClosestTouch);
		if (job.ignoreForbidden)
		{
			yield return f.FailOnDespawnedOrNull((!targetOnStand) ? TargetIndex.A : TargetIndex.B);
		}
		else
		{
			yield return f.FailOnDespawnedNullOrForbidden((!targetOnStand) ? TargetIndex.A : TargetIndex.B);
		}
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		if (targetOnStand)
		{
			toil.initAction = delegate
			{
				Building_OutfitStand outfitStand = OutfitStand;
				ThingWithComps heldWeapon = outfitStand.HeldWeapon;
				if (heldWeapon == null || heldWeapon != Target)
				{
					EndJobWith(JobCondition.Errored);
				}
				else if (!outfitStand.RemoveHeldWeapon(heldWeapon))
				{
					EndJobWith(JobCondition.Errored);
				}
				else
				{
					pawn.equipment.MakeRoomFor(heldWeapon);
					pawn.equipment.AddEquipment(heldWeapon);
					heldWeapon.def.soundInteract?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
				}
			};
		}
		else
		{
			toil.initAction = delegate
			{
				ThingWithComps thingWithComps = (ThingWithComps)job.targetA.Thing;
				ThingWithComps thingWithComps2 = null;
				if (thingWithComps.def.stackLimit > 1 && thingWithComps.stackCount > 1)
				{
					thingWithComps2 = (ThingWithComps)thingWithComps.SplitOff(1);
				}
				else
				{
					thingWithComps2 = thingWithComps;
					thingWithComps2.DeSpawn();
				}
				pawn.equipment.MakeRoomFor(thingWithComps2);
				pawn.equipment.AddEquipment(thingWithComps2);
				thingWithComps.def.soundInteract?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
			};
		}
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil;
	}
}
