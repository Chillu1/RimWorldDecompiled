using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class Command_BestowerCeremony : Command
{
	private Pawn bestower;

	private Pawn forPawn;

	private Action<List<Pawn>> action;

	private LordJob_BestowingCeremony job;

	public Command_BestowerCeremony(LordJob_BestowingCeremony job, Pawn bestower, Pawn forPawn, Action<List<Pawn>> action)
	{
		this.bestower = bestower;
		this.forPawn = forPawn;
		this.action = action;
		this.job = job;
		defaultLabel = "BeginCeremony".Translate(this.forPawn);
		icon = ContentFinder<Texture2D>.Get("UI/Icons/Rituals/BestowCeremony");
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		if (!JobDriver_BestowingCeremony.AnalyzeThroneRoom(bestower, forPawn))
		{
			disabledReason = "BestowingCeremonyThroneroomRequirementsNotSatisfiedShort".Translate(forPawn.Named("PAWN"), forPawn.royalty.GetTitleAwardedWhenUpdating(bestower.Faction, forPawn.royalty.GetFavor(bestower.Faction)).label.Named("TITLE"));
			disabled = true;
		}
		else if (!job.GetSpot().IsValid)
		{
			disabledReason = "MessageBestowerUnreachable".Translate();
			disabled = true;
		}
		else
		{
			Lord lord = forPawn.GetLord();
			if (lord != null)
			{
				if (lord.LordJob is LordJob_Ritual)
				{
					disabledReason = "CantStartRitualTargetIsAlreadyInRitual".Translate(forPawn.LabelShort);
					disabled = true;
				}
				else
				{
					disabledReason = "MessageBestowingTargetIsBusy".Translate(forPawn.LabelShort);
					disabled = true;
				}
			}
		}
		return base.GizmoOnGUI(topLeft, maxWidth, parms);
	}

	public override void ProcessInput(Event ev)
	{
		base.ProcessInput(ev);
		Find.WindowStack.Add(new Dialog_BeginRitual("RitualBestowingCeremony".Translate(), null, job.targetSpot.ToTargetInfo(bestower.Map), bestower.Map, delegate(RitualRoleAssignments assignments)
		{
			action(assignments.Participants.Where((Pawn p) => p != bestower).ToList());
			return true;
		}, bestower, null, delegate(Pawn pawn, bool voluntary, bool allowOtherIdeos)
		{
			if (pawn.GetLord()?.LordJob is LordJob_Ritual)
			{
				return false;
			}
			if (pawn.IsSubhuman)
			{
				return false;
			}
			return !pawn.IsPrisonerOfColony && !pawn.RaceProps.Animal;
		}, "Begin".Translate(), new List<Pawn> { bestower, forPawn }, null, RitualOutcomeEffectDefOf.BestowingCeremony));
	}
}
