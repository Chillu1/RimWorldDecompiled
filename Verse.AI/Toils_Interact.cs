using RimWorld;

namespace Verse.AI;

public static class Toils_Interact
{
	public static Toil DestroyThing(TargetIndex ind)
	{
		Toil toil = ToilMaker.MakeToil("DestroyThing");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Thing thing = actor.jobs.curJob.GetTarget(ind).Thing;
			if (!thing.Destroyed)
			{
				if (thing.def.category == ThingCategory.Plant && thing.def.plant.IsTree && thing.def.plant.treeLoversCareIfChopped)
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.CutTree, actor.Named(HistoryEventArgsNames.Doer)));
				}
				thing.Destroy();
			}
		};
		return toil;
	}
}
