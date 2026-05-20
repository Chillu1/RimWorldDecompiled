using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_Relic : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool RequiresManipulation => true;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		return ModsConfig.IdeologyActive;
	}

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
	{
		if (clickedThing.TryGetComp(out CompRelicContainer container))
		{
			FloatMenuOption option;
			if (container.Full)
			{
				yield return ExtractRelic(container, context.FirstSelectedPawn);
				if (!context.FirstSelectedPawn.Map.IsPlayerHome && !context.FirstSelectedPawn.IsFormingCaravan())
				{
					yield return ExtractToInventory(container, context.FirstSelectedPawn);
				}
			}
			else if (InstallRelicFromContainer(container.parent, context.FirstSelectedPawn, out option))
			{
				yield return option;
			}
		}
		if (CompRelicContainer.IsRelic(clickedThing) && clickedThing.Spawned)
		{
			yield return InstallRelic(clickedThing, context.FirstSelectedPawn);
		}
	}

	private FloatMenuOption InstallRelic(Thing containerThing, Pawn pawn)
	{
		IEnumerable<Thing> searchSet = from x in containerThing.Map.listerThings.ThingsOfDef(ThingDefOf.Reliquary)
			where x.TryGetComp<CompRelicContainer>().ContainedThing == null
			select x;
		Thing thing = GenClosest.ClosestThing_Global_Reachable(containerThing.Position, containerThing.Map, searchSet, PathEndMode.Touch, TraverseParms.For(pawn), 9999f, (Thing t) => pawn.CanReserve(t));
		if (thing == null)
		{
			return new FloatMenuOption("InstallInReliquary".Translate() + " (" + "NoEmptyReliquary".Translate() + ")", null);
		}
		Job job = JobMaker.MakeJob(JobDefOf.InstallRelic, containerThing, thing, thing.InteractionCell);
		job.count = 1;
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InstallInReliquary".Translate(), delegate
		{
			pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), pawn, new LocalTargetInfo(containerThing));
	}

	private FloatMenuOption ExtractRelic(CompRelicContainer container, Pawn pawn)
	{
		TaggedString taggedString = "ExtractRelic".Translate(container.ContainedThing.Label);
		if (!StoreUtility.TryFindBestBetterStorageFor(container.ContainedThing, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out var foundCell, out var _))
		{
			return new FloatMenuOption(taggedString + " (" + HaulAIUtility.NoEmptyPlaceLowerTrans + ")", null);
		}
		Job job = JobMaker.MakeJob(JobDefOf.ExtractRelic, container.parent, container.ContainedThing, foundCell);
		job.count = 1;
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString, delegate
		{
			pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), pawn, new LocalTargetInfo(container.parent));
	}

	private bool InstallRelicFromContainer(Thing containerThing, Pawn pawn, out FloatMenuOption option)
	{
		option = null;
		IEnumerable<Thing> enumerable = pawn.Map.listerThings.AllThings.Where((Thing x) => CompRelicContainer.IsRelic(x) && pawn.CanReach(x, PathEndMode.ClosestTouch, Danger.Deadly));
		if (!enumerable.Any())
		{
			option = new FloatMenuOption("NoRelicToInstall".Translate(), null);
			return true;
		}
		using (IEnumerator<Thing> enumerator = enumerable.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				Thing current = enumerator.Current;
				Job job = JobMaker.MakeJob(JobDefOf.InstallRelic, current, containerThing, containerThing.InteractionCell);
				job.count = 1;
				option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InstallRelic".Translate(current.Label), delegate
				{
					pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				}), pawn, new LocalTargetInfo(containerThing));
				return true;
			}
		}
		return false;
	}

	private FloatMenuOption ExtractToInventory(CompRelicContainer container, Pawn pawn)
	{
		return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("ExtractRelicToInventory".Translate(container.ContainedThing.Label, 300.ToStringTicksToPeriod()), delegate
		{
			Job job = JobMaker.MakeJob(JobDefOf.ExtractToInventory, container.parent, container.ContainedThing, container.parent.InteractionCell);
			job.count = 1;
			pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}), pawn, new LocalTargetInfo(container.parent));
	}
}
