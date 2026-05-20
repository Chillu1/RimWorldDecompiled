using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_WorkGivers : FloatMenuOptionProvider
{
	private static bool shouldProcessEquivalenceGroups = false;

	private static FloatMenuOption[] equivalenceGroupTempStorage;

	private static HashSet<string> tmpUsedLabels = new HashSet<string>();

	private static List<FloatMenuOption> tmpFloatMenuOptions = new List<FloatMenuOption>();

	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool MechanoidCanDo => false;

	protected override bool CanSelfTarget => true;

	public override IEnumerable<FloatMenuOption> GetOptions(FloatMenuContext context)
	{
		shouldProcessEquivalenceGroups = false;
		if (equivalenceGroupTempStorage == null || equivalenceGroupTempStorage.Length != DefDatabase<WorkGiverEquivalenceGroupDef>.DefCount)
		{
			equivalenceGroupTempStorage = new FloatMenuOption[DefDatabase<WorkGiverEquivalenceGroupDef>.DefCount];
		}
		tmpUsedLabels.Clear();
		foreach (FloatMenuOption item in GetWorkGiversOptionsFor(context.FirstSelectedPawn, context.ClickedCell, context))
		{
			yield return item;
		}
	}

	public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
	{
		shouldProcessEquivalenceGroups = false;
		foreach (FloatMenuOption item in GetWorkGiversOptionsFor(context.FirstSelectedPawn, clickedThing, context))
		{
			yield return item;
		}
		if (!shouldProcessEquivalenceGroups)
		{
			yield break;
		}
		int i = 0;
		while (i < equivalenceGroupTempStorage.Length)
		{
			if (equivalenceGroupTempStorage[i] != null)
			{
				tmpUsedLabels.Add(equivalenceGroupTempStorage[i].Label);
				yield return equivalenceGroupTempStorage[i];
				equivalenceGroupTempStorage[i] = null;
			}
			int num = i + 1;
			i = num;
		}
	}

	private IEnumerable<FloatMenuOption> GetWorkGiversOptionsFor(Pawn pawn, LocalTargetInfo target, FloatMenuContext context)
	{
		if (pawn.thinker.TryGetMainTreeThinkNode<JobGiver_Work>() == null)
		{
			yield break;
		}
		tmpFloatMenuOptions.Clear();
		foreach (WorkTypeDef item in DefDatabase<WorkTypeDef>.AllDefsListForReading)
		{
			foreach (WorkGiverDef item2 in item.workGiversByPriority)
			{
				try
				{
					FloatMenuOption workGiverOption = GetWorkGiverOption(pawn, item2, target, context);
					if (workGiverOption != null)
					{
						tmpUsedLabels.Add(workGiverOption.Label);
						tmpFloatMenuOptions.Add(workGiverOption);
					}
				}
				catch (Exception ex)
				{
					Log.Error("Exception in WorkGiverOptionProvider_WorkGivers.GetWorkGiversOptionsFor for " + item2.defName + ": " + ex);
				}
			}
		}
		foreach (FloatMenuOption tmpFloatMenuOption in tmpFloatMenuOptions)
		{
			yield return tmpFloatMenuOption;
		}
	}

	private FloatMenuOption GetWorkGiverOption(Pawn pawn, WorkGiverDef workGiver, LocalTargetInfo target, FloatMenuContext context)
	{
		if (pawn.Drafted && !workGiver.canBeDoneWhileDrafted)
		{
			return null;
		}
		if (!(workGiver.Worker is WorkGiver_Scanner workGiver_Scanner) || !workGiver_Scanner.def.directOrderable)
		{
			return null;
		}
		JobFailReason.Clear();
		if (target.HasThing)
		{
			if (ScannerShouldSkip(pawn, workGiver_Scanner, target.Thing))
			{
				return null;
			}
		}
		else if (!workGiver_Scanner.PotentialWorkCellsGlobal(pawn).Contains(target.Cell) || workGiver_Scanner.ShouldSkip(pawn, forced: true))
		{
			return null;
		}
		Action action = null;
		Job job = (target.HasThing ? (workGiver_Scanner.HasJobOnThing(pawn, target.Thing, forced: true) ? workGiver_Scanner.JobOnThing(pawn, target.Thing, forced: true) : null) : (workGiver_Scanner.HasJobOnCell(pawn, target.Cell, forced: true) ? workGiver_Scanner.JobOnCell(pawn, target.Cell, forced: true) : null));
		string text = ((!JobFailReason.CustomJobString.NullOrEmpty()) ? ((string)"CannotGenericWorkCustom".Translate(JobFailReason.CustomJobString).CapitalizeFirst()) : ((!target.HasThing) ? ((string)"CannotGenericWork".Translate(workGiver.verb, "AreaLower".Translate())) : ((string)"CannotGenericWork".Translate(workGiver.verb, target.Thing.Label))));
		if (job == null)
		{
			if (!JobFailReason.HaveReason)
			{
				return null;
			}
			text = text + ": " + JobFailReason.Reason.CapitalizeFirst();
		}
		else
		{
			WorkTypeDef workType = workGiver_Scanner.def.workType;
			PawnCapacityDef pawnCapacityDef = workGiver_Scanner.MissingRequiredCapacity(pawn);
			if (pawnCapacityDef != null)
			{
				text += ": " + "CannotMissingHealthActivities".Translate(pawnCapacityDef.label);
			}
			else if (pawn.WorkTagIsDisabled(workGiver_Scanner.def.workTags))
			{
				text += ": " + "CannotPrioritizeWorkGiverDisabled".Translate(workGiver_Scanner.def.label);
			}
			else if (pawn.jobs.curJob != null && pawn.jobs.curJob.JobIsSameAs(pawn, job))
			{
				text = ((!target.HasThing) ? ((string)"CannotGenericAlreadyAmCustom".Translate(workGiver_Scanner.PostProcessedGerund(job))) : ((string)"CannotGenericAlreadyAm".Translate(workGiver_Scanner.PostProcessedGerund(job), target.Thing.LabelShort, target.Thing)));
			}
			else if (pawn.workSettings.GetPriority(workType) == 0)
			{
				text = (pawn.WorkTypeIsDisabled(workType) ? ((string)(text + (": " + "CannotPrioritizeWorkTypeDisabled".Translate(workType.gerundLabel)))) : ((!"CannotPrioritizeNotAssignedToWorkType".CanTranslate()) ? ((string)(text + (": " + "CannotPrioritizeWorkTypeDisabled".Translate(workType.pawnLabel)))) : ((string)(text + (": " + "CannotPrioritizeNotAssignedToWorkType".Translate(workType.gerundLabel))))));
			}
			else if (job.def == JobDefOf.Research && target.Thing is Building_ResearchBench)
			{
				text += ": " + "CannotPrioritizeResearch".Translate();
			}
			else if (target.HasThing && target.Thing.IsForbidden(pawn))
			{
				text = (target.Thing.Position.InAllowedArea(pawn) ? ((string)(text + (": " + "CannotPrioritizeForbidden".Translate(target.Thing.Label, target.Thing)))) : ((string)(text + (": " + "CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + (" (" + pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap.Label + ")")))));
			}
			else if (!target.HasThing && target.Cell.IsForbidden(pawn))
			{
				text = (target.Cell.InAllowedArea(pawn) ? ((string)(text + (": " + "CannotPrioritizeCellForbidden".Translate()))) : ((string)(text + (": " + "CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + (" (" + pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap.Label + ")")))));
			}
			else if ((target.HasThing && !pawn.CanReach(target.Thing, workGiver_Scanner.PathEndMode, Danger.Deadly)) || (!target.HasThing && !pawn.CanReach(target.Cell, PathEndMode.Touch, Danger.Deadly)))
			{
				text += ": " + "NoPath".Translate().CapitalizeFirst();
			}
			else
			{
				text = (target.HasThing ? ((string)"PrioritizeGeneric".Translate(workGiver_Scanner.PostProcessedGerund(job), target.Thing.Label).CapitalizeFirst()) : ((!job.def.displayAsAreaInFloatMenu) ? ((string)"PrioritizeGenericSimple".Translate(workGiver_Scanner.PostProcessedGerund(job)).CapitalizeFirst()) : ((string)"PrioritizeGeneric".Translate(workGiver_Scanner.PostProcessedGerund(job), "AreaLower".Translate()).CapitalizeFirst())));
				Job localJob = job;
				WorkGiver_Scanner localScanner = workGiver_Scanner;
				job.workGiverDef = workGiver_Scanner.def;
				WorkGiverDef giver = workGiver;
				action = delegate
				{
					if (pawn.jobs.TryTakeOrderedJobPrioritizedWork(localJob, localScanner, context.ClickedCell))
					{
						if (giver.forceMote != null)
						{
							MoteMaker.MakeStaticMote(context.ClickedCell, pawn.Map, giver.forceMote);
						}
						if (giver.forceFleck != null)
						{
							FleckMaker.Static(context.ClickedCell, pawn.Map, giver.forceFleck);
						}
					}
				};
			}
		}
		if (DebugViewSettings.showFloatMenuWorkGivers)
		{
			text += $" (from {workGiver.defName})";
		}
		FloatMenuOption menuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, action), pawn, target);
		if (tmpUsedLabels.Any((string used) => used == menuOption.Label))
		{
			return null;
		}
		if (pawn.Drafted && workGiver.autoTakeablePriorityDrafted != -1)
		{
			menuOption.autoTakeable = true;
			menuOption.autoTakeablePriority = workGiver.autoTakeablePriorityDrafted;
		}
		if (target.HasThing && workGiver.equivalenceGroup != null)
		{
			if (equivalenceGroupTempStorage[workGiver.equivalenceGroup.index] == null || (equivalenceGroupTempStorage[workGiver.equivalenceGroup.index].Disabled && !menuOption.Disabled))
			{
				equivalenceGroupTempStorage[workGiver.equivalenceGroup.index] = menuOption;
				shouldProcessEquivalenceGroups = true;
			}
			return null;
		}
		return menuOption;
	}

	private static bool ScannerShouldSkip(Pawn pawn, WorkGiver_Scanner scanner, Thing t)
	{
		if (scanner.PotentialWorkThingRequest.Accepts(t) || (scanner.PotentialWorkThingsGlobal(pawn) != null && scanner.PotentialWorkThingsGlobal(pawn).Contains(t)))
		{
			return scanner.ShouldSkip(pawn, forced: true);
		}
		return true;
	}
}
