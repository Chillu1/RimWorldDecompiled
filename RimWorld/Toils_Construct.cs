using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class Toils_Construct
	{
		public static Toil MakeSolidThingFromBlueprintIfNecessary(TargetIndex blueTarget, TargetIndex targetToUpdate = TargetIndex.None)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Blueprint blueprint = curJob.GetTarget(blueTarget).Thing as Blueprint;
				if (blueprint != null)
				{
					bool flag = targetToUpdate != 0 && curJob.GetTarget(targetToUpdate).Thing == blueprint;
					if (blueprint.TryReplaceWithSolidThing(actor, out var createdThing, out var _))
					{
						curJob.SetTarget(blueTarget, createdThing);
						if (flag)
						{
							curJob.SetTarget(targetToUpdate, createdThing);
						}
						if (createdThing is Frame)
						{
							actor.Reserve(createdThing, curJob);
						}
					}
				}
			};
			return toil;
		}

		public static Toil UninstallIfMinifiable(TargetIndex thingInd)
		{
			Toil uninstallIfMinifiable = new Toil().FailOnDestroyedNullOrForbidden(thingInd);
			uninstallIfMinifiable.initAction = delegate
			{
				Pawn actor2 = uninstallIfMinifiable.actor;
				JobDriver curDriver2 = actor2.jobs.curDriver;
				Thing thing2 = actor2.CurJob.GetTarget(thingInd).Thing;
				if (thing2.def.Minifiable)
				{
					curDriver2.uninstallWorkLeft = thing2.def.building.uninstallWork;
				}
				else
				{
					curDriver2.ReadyForNextToil();
				}
			};
			uninstallIfMinifiable.tickAction = delegate
			{
				Pawn actor = uninstallIfMinifiable.actor;
				JobDriver curDriver = actor.jobs.curDriver;
				Job curJob = actor.CurJob;
				curDriver.uninstallWorkLeft -= actor.GetStatValue(StatDefOf.ConstructionSpeed) * 1.7f;
				if (curDriver.uninstallWorkLeft <= 0f)
				{
					Thing thing = curJob.GetTarget(thingInd).Thing;
					MinifiedThing minifiedThing = thing.MakeMinified();
					GenSpawn.Spawn(minifiedThing, thing.Position, uninstallIfMinifiable.actor.Map);
					curJob.SetTarget(thingInd, minifiedThing);
					actor.jobs.curDriver.ReadyForNextToil();
				}
			};
			uninstallIfMinifiable.defaultCompleteMode = ToilCompleteMode.Never;
			uninstallIfMinifiable.WithProgressBar(thingInd, () => 1f - uninstallIfMinifiable.actor.jobs.curDriver.uninstallWorkLeft / uninstallIfMinifiable.actor.CurJob.targetA.Thing.def.building.uninstallWork);
			return uninstallIfMinifiable;
		}
	}
}
