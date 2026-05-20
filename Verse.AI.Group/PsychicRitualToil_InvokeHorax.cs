using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse.AI.Group;

public class PsychicRitualToil_InvokeHorax : PsychicRitualToil
{
	public float hoursUntilHoraxEffect;

	public float hoursUntilOutcome;

	protected List<IntVec3> invokerPositions;

	protected List<IntVec3> targetPositions;

	protected List<IntVec3> chanterPositions;

	protected List<IntVec3> defenderPositions;

	public PsychicRitualRoleDef invokerRole;

	public PsychicRitualRoleDef targetRole;

	public PsychicRitualRoleDef chanterRole;

	public PsychicRitualRoleDef defenderRole;

	public IngredientCount requiredOffering;

	private const float CreepyWordsMTBSeconds = 8f;

	private int startingTick;

	private Effecter progressBar;

	private Sustainer sustainerOngoingHuman;

	private Sustainer sustainerOngoing;

	private Effecter effecterOngoing;

	private Effecter completeSoonEffecter;

	private bool completeFadeoutTriggered;

	private readonly List<Mote> motes = new List<Mote>();

	public int TicksSinceStarted => Find.TickManager.TicksGame - startingTick;

	public int TicksLeft => Mathf.RoundToInt(hoursUntilOutcome * 2500f) - TicksSinceStarted;

	public bool ShouldDoCompleteSoonEffect => TicksLeft <= EffecterDefOf.PsychicRitual_CompleteSoon.maintainTicks;

	public PsychicRitualToil_InvokeHorax(PsychicRitualRoleDef invokerRole, IEnumerable<IntVec3> invokerPositions, PsychicRitualRoleDef targetRole, IEnumerable<IntVec3> targetPositions, PsychicRitualRoleDef chanterRole, IEnumerable<IntVec3> chanterPositions, PsychicRitualRoleDef defenderRole, IEnumerable<IntVec3> defenderPositions, IngredientCount requiredOffering)
	{
		this.invokerRole = invokerRole;
		this.targetRole = targetRole;
		this.chanterRole = chanterRole;
		this.defenderRole = defenderRole;
		this.requiredOffering = requiredOffering;
		this.invokerPositions = new List<IntVec3>(invokerPositions ?? Enumerable.Empty<IntVec3>());
		this.targetPositions = new List<IntVec3>(targetPositions ?? Enumerable.Empty<IntVec3>());
		this.chanterPositions = new List<IntVec3>(chanterPositions ?? Enumerable.Empty<IntVec3>());
		this.defenderPositions = new List<IntVec3>(defenderPositions ?? Enumerable.Empty<IntVec3>());
	}

	protected PsychicRitualToil_InvokeHorax()
	{
	}

	public virtual void HoldRequiredOfferings(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		if (requiredOffering == null)
		{
			return;
		}
		foreach (Pawn item in psychicRitual.assignments.AssignedPawns(invokerRole))
		{
			foreach (Thing item2 in (IEnumerable<Thing>)item.inventory.GetDirectlyHeldThings())
			{
				if (requiredOffering.filter.Allows(item2))
				{
					item.inventory.innerContainer.TryTransferToContainer(item2, item.carryTracker.innerContainer, Mathf.CeilToInt(requiredOffering.GetBaseCount()));
				}
			}
		}
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		startingTick = Find.TickManager.TicksGame;
		HoldRequiredOfferings(psychicRitual, parent);
		TaggedString taggedString;
		MessageTypeDef def;
		if (psychicRitual.lord?.faction == Faction.OfPlayer)
		{
			taggedString = "PsychicRitualDef_InvocationCircle_PsychicRitualBegunMessage".Translate(psychicRitual.assignments.FirstAssignedPawn(invokerRole), psychicRitual.def.label);
			def = MessageTypeDefOf.NeutralEvent;
		}
		else
		{
			taggedString = "MessageAIPsychicRitualBegan".Translate(psychicRitual.lord.faction.Named("FACTION")).CapitalizeFirst() + ": " + psychicRitual.def.LabelCap;
			def = MessageTypeDefOf.ThreatSmall;
		}
		Messages.Message(taggedString.CapitalizeFirst(), psychicRitual.assignments.Target, def);
	}

	public override void UpdateAllDuties(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		LocalTargetInfo value = (LocalTargetInfo)psychicRitual.assignments.Target;
		int num = 0;
		bool playerRitual = psychicRitual.lord.faction == Faction.OfPlayer;
		if (invokerRole != null)
		{
			foreach (Pawn item in psychicRitual.assignments.AssignedPawns(invokerRole))
			{
				IntVec3 bestStandableRolePosition = psychicRitual.def.GetBestStandableRolePosition(playerRitual, invokerPositions[num++], value.Cell, psychicRitual.Map);
				SetPawnDuty(item, psychicRitual, parent, DutyDefOf.Invoke, value, bestStandableRolePosition);
			}
		}
		if (targetRole != null)
		{
			num = 0;
			foreach (Pawn item2 in psychicRitual.assignments.AssignedPawns(targetRole))
			{
				IntVec3 bestStandableRolePosition2 = psychicRitual.def.GetBestStandableRolePosition(playerRitual, targetPositions[num++], value.Cell, psychicRitual.Map);
				DutyDef idle = DutyDefOf.Idle;
				LocalTargetInfo? focus = bestStandableRolePosition2;
				Rot4? overrideFacing = Rot4.South;
				SetPawnDuty(item2, psychicRitual, parent, idle, focus, null, null, null, 0f, overrideFacing);
			}
		}
		if (chanterRole != null)
		{
			num = 0;
			foreach (Pawn item3 in psychicRitual.assignments.AssignedPawns(chanterRole))
			{
				IntVec3 bestStandableRolePosition3 = psychicRitual.def.GetBestStandableRolePosition(playerRitual, chanterPositions[num++], value.Cell, psychicRitual.Map);
				SetPawnDuty(item3, psychicRitual, parent, DutyDefOf.PsychicRitualDance, value, bestStandableRolePosition3);
			}
		}
		if (defenderRole == null)
		{
			return;
		}
		num = 0;
		foreach (Pawn item4 in psychicRitual.assignments.AssignedPawns(defenderRole))
		{
			IntVec3 bestStandableRolePosition4 = psychicRitual.def.GetBestStandableRolePosition(playerRitual, defenderPositions[num++], value.Cell, psychicRitual.Map);
			SetPawnDuty(item4, psychicRitual, parent, DutyDefOf.DefendInvoker, bestStandableRolePosition4, null, null, null, 3f);
		}
	}

	protected virtual void UpdateProgressBar(PsychicRitual psychicRitual)
	{
		TargetInfo target = psychicRitual.assignments.Target;
		if (!target.IsValid)
		{
			return;
		}
		if (progressBar == null)
		{
			Thing thing = psychicRitual.assignments.Target.Thing;
			if (thing != null && !thing.Destroyed)
			{
				progressBar = EffecterDefOf.ProgressBarAlwaysVisible.SpawnAttached(thing, thing.MapHeld);
			}
			else
			{
				progressBar = EffecterDefOf.ProgressBarAlwaysVisible.Spawn(target.Cell, target.Map);
			}
		}
		progressBar?.EffectTick(target, TargetInfo.Invalid);
		MoteProgressBar moteProgressBar = (progressBar?.children[0] as SubEffecter_ProgressBar)?.mote;
		if (moteProgressBar != null)
		{
			moteProgressBar.progress = Mathf.Clamp01((float)TicksSinceStarted / (hoursUntilOutcome * 2500f));
			moteProgressBar.alwaysShow = true;
		}
	}

	public override bool Tick(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		if (requiredOffering != null)
		{
			float num = requiredOffering.GetBaseCount();
			foreach (Pawn item in psychicRitual.assignments.AssignedPawns(invokerRole))
			{
				Thing thing = item.carryTracker?.CarriedThing;
				if (thing != null && requiredOffering.filter.Allows(thing))
				{
					num -= (float)thing.stackCount;
				}
			}
			if (num > 0f)
			{
				psychicRitual.CancelPsychicRitual("PsychicRitualToil_InvokeHorax_MissingRequiredOffering".Translate(psychicRitual.assignments.FirstAssignedPawn(invokerRole).Named("PAWN"), requiredOffering.filter.Summary));
				return true;
			}
		}
		if (targetRole != null)
		{
			Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(targetRole);
			if (pawn.mindState.duty != null && pawn.PositionHeld != pawn.mindState.duty.focus.Cell)
			{
				if (psychicRitual.lord.ownedPawns.Contains(pawn))
				{
					psychicRitual.lord.Notify_PawnLost(pawn, PawnLostCondition.LordRejected);
				}
				psychicRitual.CancelPsychicRitual("PsychicRitualToil_InvokeHorax_PawnIsOutOfPosition".Translate(pawn.Named("PAWN")));
				return true;
			}
		}
		UpdateProgressBar(psychicRitual);
		TickSound(psychicRitual);
		TickVfx(psychicRitual);
		if ((float)TicksSinceStarted > hoursUntilOutcome * 2500f)
		{
			return true;
		}
		return false;
	}

	private void TickVfx(PsychicRitual psychicRitual)
	{
		if (effecterOngoing == null)
		{
			effecterOngoing = EffecterDefOf.PsychicRitual_Sustained.Spawn(psychicRitual.assignments.Target.Cell, psychicRitual.assignments.Target.Cell, psychicRitual.Map);
			foreach (Pawn allAssignedPawn in psychicRitual.assignments.AllAssignedPawns)
			{
				if (psychicRitual.assignments.RoleForPawn(allAssignedPawn).showInvocationVfx)
				{
					motes.Add(MoteMaker.MakeAttachedOverlay(allAssignedPawn, ThingDefOf.Mote_HateChantShadow, new Vector3(0f, 0f, 0f)));
					motes.Add(MoteMaker.MakeAttachedOverlay(allAssignedPawn, ThingDefOf.Mote_PsychicRitualInvocation, new Vector3(0f, 0f, 0f)));
				}
			}
			float num = 4f;
			int num2 = 6;
			ThingDef[] list = new ThingDef[3]
			{
				ThingDefOf.Mote_RitualCandleA,
				ThingDefOf.Mote_RitualCandleB,
				ThingDefOf.Mote_RitualCandleC
			};
			for (int i = 0; i < num2; i++)
			{
				float f = (float)i / (float)num2 * MathF.PI * 2f;
				IntVec3 cell = psychicRitual.assignments.Target.Cell;
				cell.x += (int)(num * Mathf.Cos(f));
				cell.z += (int)(num * Mathf.Sin(f));
				motes.Add(MoteMaker.MakeStaticMote(cell, psychicRitual.Map, Rand.Element(list)));
			}
		}
		effecterOngoing.EffectTick(psychicRitual.assignments.Target, psychicRitual.assignments.Target);
		foreach (Mote mote in motes)
		{
			mote?.Maintain();
		}
		TargetInfo targetInfo = new TargetInfo(psychicRitual.assignments.Target.Cell, psychicRitual.Map);
		if (completeSoonEffecter == null && ShouldDoCompleteSoonEffect)
		{
			completeSoonEffecter = EffecterDefOf.PsychicRitual_CompleteSoon.Spawn(targetInfo, targetInfo);
		}
		completeSoonEffecter?.EffectTick(targetInfo, targetInfo);
		if (Rand.MTBEventOccurs(8f, 60f, 1f))
		{
			Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
			if (pawn != null && pawn.Spawned)
			{
				SocialInteractionUtility.ImitateInteractionWithNoPawn(pawn, InteractionDefOf.CreepyWords);
			}
		}
	}

	private void TickSound(PsychicRitual psychicRitual)
	{
		if (TicksLeft <= 300)
		{
			TickCompleteSfx(psychicRitual);
			return;
		}
		if (sustainerOngoingHuman == null)
		{
			sustainerOngoingHuman = SoundDefOf.PsychicRitual_Ongoing_Human.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(psychicRitual.assignments.Target.Cell, psychicRitual.Map), MaintenanceType.PerTick));
		}
		if (Mathf.Clamp01((float)TicksSinceStarted / (hoursUntilOutcome * 2500f)) >= 0.6f && sustainerOngoing == null)
		{
			sustainerOngoing = SoundDefOf.PsychicRitual_Ongoing.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(psychicRitual.assignments.Target.Cell, psychicRitual.Map), MaintenanceType.PerTick));
		}
		sustainerOngoingHuman?.Maintain();
		sustainerOngoing?.Maintain();
	}

	private void TickCompleteSfx(PsychicRitual psychicRitual)
	{
		if (completeFadeoutTriggered)
		{
			if (sustainerOngoingHuman != null)
			{
				sustainerOngoingHuman.End();
				sustainerOngoingHuman = null;
			}
			if (sustainerOngoing != null)
			{
				sustainerOngoing.End();
				sustainerOngoing = null;
			}
		}
		else
		{
			if (sustainerOngoingHuman == null)
			{
				sustainerOngoingHuman = SoundDefOf.PsychicRitual_Ongoing_Human.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(psychicRitual.assignments.Target.Cell, psychicRitual.Map), MaintenanceType.PerTick));
			}
			if (sustainerOngoing == null)
			{
				sustainerOngoing = SoundDefOf.PsychicRitual_Ongoing.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(psychicRitual.assignments.Target.Cell, psychicRitual.Map), MaintenanceType.PerTick));
			}
			SoundDefOf.PsychicRitual_Complete.PlayOneShot(SoundInfo.InMap(new TargetInfo(psychicRitual.assignments.Target.Cell, psychicRitual.Map)));
			completeFadeoutTriggered = true;
		}
	}

	public virtual void ConsumeRequiredOffering(PsychicRitual psychicRitual)
	{
		if (requiredOffering == null)
		{
			return;
		}
		float num = requiredOffering.GetBaseCount();
		foreach (Pawn item in psychicRitual.assignments.AssignedPawns(invokerRole))
		{
			Thing thing = item.carryTracker?.CarriedThing;
			if (thing != null && requiredOffering.filter.Allows(thing))
			{
				int num2 = Mathf.Min(Mathf.CeilToInt(num), thing.stackCount);
				if (num2 < thing.stackCount)
				{
					thing.stackCount -= num2;
				}
				else
				{
					thing.Destroy();
				}
				num -= (float)num2;
				if (num <= 0f)
				{
					return;
				}
			}
		}
		if (!(num > 0f))
		{
			return;
		}
		throw new InvalidOperationException($"Invocation should consume {requiredOffering.Summary}, but {num} is unaccounted for.");
	}

	public override void End(PsychicRitual psychicRitual, PsychicRitualGraph parent, bool success)
	{
		base.End(psychicRitual, parent, success);
		progressBar?.Cleanup();
		progressBar = null;
		if (psychicRitual.def is PsychicRitualDef_InvocationCircle { TargetRole: not null } psychicRitualDef_InvocationCircle)
		{
			Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(psychicRitualDef_InvocationCircle.TargetRole);
			if (pawn != null && pawn.IsPrisonerOfColony)
			{
				pawn.guest.WaitInsteadOfEscapingFor(1250);
			}
		}
		if (!success)
		{
			PlayInterruptedSfx(psychicRitual);
			return;
		}
		psychicRitual.succeeded = true;
		TargetInfo targetInfo = new TargetInfo(psychicRitual.assignments.Target.Cell, psychicRitual.Map);
		EffecterDefOf.PsychicRitual_Complete.SpawnMaintained(targetInfo, targetInfo);
		psychicRitual.def.CalculateMaxPower(psychicRitual.assignments, null, out var power);
		psychicRitual.maxPower = 1f;
		psychicRitual.power = power;
		ConsumeRequiredOffering(psychicRitual);
		Pawn pawn2 = psychicRitual.assignments?.FirstAssignedPawn(invokerRole);
		if (pawn2 != null)
		{
			TaleRecorder.RecordTale(TaleDefOf.PerformedPsychicRitual, pawn2);
		}
	}

	public override string GetJobReport(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		PsychicRitualRoleDef psychicRitualRoleDef = psychicRitual.assignments.RoleForPawn(pawn);
		if (psychicRitualRoleDef == null)
		{
			return base.GetJobReport(psychicRitual, parent, pawn);
		}
		string text = " (" + "DurationLeft".Translate(TicksLeft.ToStringTicksToPeriod()).Resolve() + ")";
		if (pawn.Downed)
		{
			return null;
		}
		if (psychicRitualRoleDef == invokerRole)
		{
			return "PsychicRitualToil_InvokeHorax_JobReport_Invoker".Translate().Resolve() + text;
		}
		if (psychicRitualRoleDef == targetRole)
		{
			return "PsychicRitualToil_InvokeHorax_JobReport_Target".Translate().Resolve() + text;
		}
		if (psychicRitualRoleDef == chanterRole)
		{
			return "PsychicRitualToil_InvokeHorax_JobReport_Chanter".Translate().Resolve() + text;
		}
		if (psychicRitualRoleDef == defenderRole)
		{
			return "PsychicRitualToil_InvokeHorax_JobReport_Defender".Translate().Resolve() + text;
		}
		return base.GetJobReport(psychicRitual, parent, pawn);
	}

	public override string GetReport(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		return "DurationLeft".Translate(TicksLeft.ToStringTicksToPeriod()).Resolve() ?? "";
	}

	public override ThinkResult Notify_DutyResult(PsychicRitual psychicRitual, PsychicRitualGraph parent, ThinkResult result, Pawn pawn, JobIssueParams issueParams)
	{
		if (result.Job == null)
		{
			PsychicRitualRoleDef psychicRitualRoleDef = psychicRitual.assignments.RoleForPawn(pawn);
			if (psychicRitualRoleDef != null && (psychicRitualRoleDef == invokerRole || psychicRitualRoleDef == chanterRole) && pawn.Position != pawn.mindState.duty.focusSecond.Cell)
			{
				psychicRitual.LeaveOrCancelPsychicRitual(psychicRitualRoleDef, pawn, "PsychicRitualToil_InvokeHorax_PawnIsOutOfPosition".Translate(pawn.Named("PAWN")));
			}
		}
		return base.Notify_DutyResult(psychicRitual, parent, result, pawn, issueParams);
	}

	private Gizmo DebugFinishGizmo()
	{
		return new Command_Action
		{
			defaultLabel = "Debug: Finish psychic ritual",
			defaultDesc = "Finishes the current ritual and causes its outcome to occur immediately.",
			action = delegate
			{
				hoursUntilOutcome = 0f;
			}
		};
	}

	public override IEnumerable<Gizmo> GetBuildingGizmos(PsychicRitual psychicRitual, PsychicRitualGraph parent, Building building)
	{
		foreach (Gizmo buildingGizmo in base.GetBuildingGizmos(psychicRitual, parent, building))
		{
			yield return buildingGizmo;
		}
		if (DebugSettings.godMode)
		{
			yield return DebugFinishGizmo();
		}
	}

	public override IEnumerable<Gizmo> GetPawnGizmos(PsychicRitual psychicRitual, PsychicRitualGraph parent, Pawn pawn)
	{
		foreach (Gizmo pawnGizmo in base.GetPawnGizmos(psychicRitual, parent, pawn))
		{
			yield return pawnGizmo;
		}
		if (DebugSettings.godMode)
		{
			yield return DebugFinishGizmo();
		}
	}

	private void PlayInterruptedSfx(PsychicRitual psychicRitual)
	{
		SoundDefOf.PsychicRitual_Interrupted.PlayOneShot(SoundInfo.InMap(new TargetInfo(psychicRitual.assignments.Target.Cell, psychicRitual.Map)));
		SoundDefOf.PsychicRitual_Interrupted_Human.PlayOneShot(SoundInfo.InMap(new TargetInfo(psychicRitual.assignments.Target.Cell, psychicRitual.Map)));
		if (sustainerOngoingHuman != null)
		{
			sustainerOngoingHuman.info.volumeFactor = 0f;
			sustainerOngoingHuman.End();
			sustainerOngoingHuman = null;
		}
		if (sustainerOngoing != null)
		{
			sustainerOngoing.info.volumeFactor = 0f;
			sustainerOngoing.End();
			sustainerOngoing = null;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref hoursUntilHoraxEffect, "hoursUntilHoraxEffect", 0f);
		Scribe_Values.Look(ref hoursUntilOutcome, "hoursUntilOutcome", 0f);
		Scribe_Collections.Look(ref invokerPositions, "invokerPositions", LookMode.Value);
		Scribe_Collections.Look(ref targetPositions, "targetPositions", LookMode.Value);
		Scribe_Collections.Look(ref chanterPositions, "chanterPositions", LookMode.Value);
		Scribe_Collections.Look(ref defenderPositions, "defenderPositions", LookMode.Value);
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
		Scribe_Defs.Look(ref targetRole, "targetRole");
		Scribe_Defs.Look(ref chanterRole, "chanterRole");
		Scribe_Defs.Look(ref defenderRole, "defenderRole");
		Scribe_Values.Look(ref startingTick, "startingTick", 0);
		Scribe_Deep.Look(ref requiredOffering, "requiredOffering");
	}
}
