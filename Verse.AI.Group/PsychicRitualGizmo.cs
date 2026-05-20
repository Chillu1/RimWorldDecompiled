using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse.AI.Group;

public static class PsychicRitualGizmo
{
	private static readonly List<PsychicRitualDef_InvocationCircle> tmpVisibleRituals = new List<PsychicRitualDef_InvocationCircle>(16);

	public static IEnumerable<Gizmo> GetGizmos(Thing target)
	{
		IOrderedEnumerable<PsychicRitualDef_InvocationCircle> orderedEnumerable = from psychicRitualDef_InvocationCircle in VisibleRituals()
			orderby psychicRitualDef_InvocationCircle.label
			select psychicRitualDef_InvocationCircle;
		foreach (PsychicRitualDef_InvocationCircle ritualDef in orderedEnumerable)
		{
			Command_PsychicRitual command_PsychicRitual = new Command_PsychicRitual
			{
				defaultLabel = ritualDef.LabelCap,
				defaultDesc = ritualDef.description + $"\n\n{ritualDef.TimeAndOfferingLabel()}",
				action = delegate
				{
					InitializePsychicRitual(ritualDef, target);
				},
				icon = ritualDef.uiIcon
			};
			AcceptanceReport acceptanceReport = Find.PsychicRitualManager.CanInvoke(ritualDef, target.Map);
			if ((bool)acceptanceReport)
			{
				command_PsychicRitual.Disabled = false;
				command_PsychicRitual.disabledReason = null;
			}
			else
			{
				command_PsychicRitual.Disable(acceptanceReport.Reason.CapitalizeFirst());
			}
			yield return command_PsychicRitual;
		}
	}

	public static Gizmo CancelGizmo(PsychicRitual psychicRitual)
	{
		return new Command_Action
		{
			defaultLabel = "CommandCancelPsychicRitual".Translate(psychicRitual.def.label).CapitalizeFirst(),
			defaultDesc = "CommandCancelPsychicRitualDesc".Translate(psychicRitual.def.label).CapitalizeFirst().EndWithPeriod(),
			icon = ContentFinder<Texture2D>.Get("UI/Commands/CancelPsychicRitual_Gizmo"),
			action = delegate
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CommandCancelPsychicRitualConfirm".Translate(psychicRitual.def.label), delegate
				{
					psychicRitual.CancelPsychicRitual(TaggedString.Empty);
				}));
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			},
			hotKey = KeyBindingDefOf.Misc6
		};
	}

	public static Gizmo LeaveGizmo(PsychicRitual psychicRitual, Pawn pawn)
	{
		return new Command_Action
		{
			defaultLabel = "CommandLeavePsychicRitual".Translate(psychicRitual.def.label).CapitalizeFirst(),
			defaultDesc = "CommandLeavePsychicRitualDesc".Translate(psychicRitual.def.label).CapitalizeFirst().EndWithPeriod(),
			action = delegate
			{
				psychicRitual.LeavePsychicRitual(pawn, TaggedString.Empty);
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			},
			hotKey = KeyBindingDefOf.Misc5
		};
	}

	private static List<PsychicRitualDef_InvocationCircle> VisibleRituals()
	{
		tmpVisibleRituals.Clear();
		foreach (PsychicRitualDef allDef in DefDatabase<PsychicRitualDef>.AllDefs)
		{
			if (allDef.Visible && allDef is PsychicRitualDef_InvocationCircle item)
			{
				tmpVisibleRituals.Add(item);
			}
		}
		return tmpVisibleRituals;
	}

	private static void InitializePsychicRitual(PsychicRitualDef_InvocationCircle psychicRitualDef, Thing target)
	{
		TargetInfo target2 = new TargetInfo(target);
		PsychicRitualRoleAssignments assignments = psychicRitualDef.BuildRoleAssignments(target2);
		PsychicRitualCandidatePool candidatePool = psychicRitualDef.FindCandidatePool();
		Map currentMap = Find.CurrentMap;
		psychicRitualDef.InitializeCast(currentMap);
		Find.WindowStack.Add(new Dialog_BeginPsychicRitual(psychicRitualDef, candidatePool, assignments, currentMap));
	}
}
