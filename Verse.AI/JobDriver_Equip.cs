using System.Collections.Generic;
using Verse.Sound;

namespace Verse.AI
{
	public class JobDriver_Equip : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			int maxPawns = 1;
			int stackCount = -1;
			if (job.targetA.HasThing && job.targetA.Thing.Spawned && job.targetA.Thing.def.IsIngestible)
			{
				maxPawns = 10;
				stackCount = 1;
			}
			return pawn.Reserve(job.targetA, job, maxPawns, stackCount, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			Toil toil = new Toil();
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
				if (thingWithComps.def.soundInteract != null)
				{
					thingWithComps.def.soundInteract.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
		}
	}
}
