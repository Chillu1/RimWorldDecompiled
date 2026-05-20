using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public abstract class CompInteractable : ThingComp, ITargetingSource
{
	protected int cooldownTicks;

	private int activeTicks;

	public float progress;

	[Unsaved(false)]
	public Effecter interactionEffecter;

	[Unsaved(false)]
	private Effecter progressBarEffecter;

	[Unsaved(false)]
	private Texture2D activateTex;

	[Unsaved(false)]
	private CompRefuelable refuelable;

	[Unsaved(false)]
	protected CompPowerTrader power;

	public const string InteractedSignal = "Interacted";

	public CompProperties_Interactable Props => (CompProperties_Interactable)props;

	public bool OnCooldown => cooldownTicks > 0;

	public virtual bool Active => activeTicks > 0;

	public virtual bool CanCooldown => true;

	public virtual int TicksToActivate => Props.ticksToActivate;

	public virtual NamedArgument? ExtraNamedArg => null;

	public virtual string ExposeKey => null;

	public virtual bool HideInteraction => false;

	protected virtual string ActivateOptionLabel => Props.jobString.CapitalizeFirst();

	public bool CasterIsPawn => true;

	public bool IsMeleeAttack => false;

	public bool Targetable => true;

	public bool MultiSelect => false;

	public bool HidePawnTooltips => false;

	public Thing Caster => parent;

	public Pawn CasterPawn => null;

	public Verb GetVerb => null;

	public TargetingParameters targetParams => Props.targetingParameters;

	public virtual ITargetingSource DestinationSelector => null;

	public Texture2D UIIcon
	{
		get
		{
			if (!(activateTex != null))
			{
				return activateTex = ContentFinder<Texture2D>.Get(Props.activateTexPath);
			}
			return activateTex;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		refuelable = parent.TryGetComp<CompRefuelable>();
		power = parent.TryGetComp<CompPowerTrader>();
	}

	public override void CompTick()
	{
		if (Active)
		{
			if (TryInteractTick())
			{
				Deactivate();
			}
			else
			{
				if (ShouldDeactivate())
				{
					SendDeactivateMessage();
					Deactivate();
					return;
				}
				activeTicks--;
			}
			if (activeTicks <= 0)
			{
				StartCooldown();
			}
		}
		else if (OnCooldown)
		{
			if (CanCooldown)
			{
				cooldownTicks--;
			}
			if (parent.Spawned)
			{
				if (progressBarEffecter == null)
				{
					progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
				}
				progressBarEffecter.EffectTick(parent, TargetInfo.Invalid);
				MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
				mote.progress = 1f - (float)cooldownTicks / (float)Props.cooldownTicks;
				mote.offsetZ = -0.8f;
			}
			else
			{
				progressBarEffecter?.Cleanup();
				progressBarEffecter = null;
			}
			if (Props.cooldownFleck != null && parent.IsHashIntervalTick(Props.cooldownFleckSpawnIntervalTicks))
			{
				FleckCreationData dataStatic = FleckMaker.GetDataStatic(parent.DrawPos, parent.Map, Props.cooldownFleck, Props.cooldownFleckScale);
				parent.Map.flecks.CreateFleck(dataStatic);
			}
			if (cooldownTicks <= 0)
			{
				CooldownEnded();
			}
		}
		interactionEffecter?.EffectTick(parent, parent);
	}

	public void ResetCooldown(bool sendMessage = true)
	{
		if (Active)
		{
			Deactivate();
		}
		if (OnCooldown)
		{
			cooldownTicks = 0;
			CooldownEnded(sendMessage);
		}
	}

	protected virtual void SendDeactivateMessage()
	{
		Messages.Message("MessageActivationCanceled".Translate(parent), parent, MessageTypeDefOf.NeutralEvent);
	}

	public virtual AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		if (Active)
		{
			return "AlreadyActive".Translate();
		}
		if (OnCooldown)
		{
			return Props.onCooldownString + " (" + "DurationLeft".Translate(cooldownTicks.ToStringTicksToPeriod()) + ")";
		}
		if (refuelable != null && !refuelable.HasFuel)
		{
			return refuelable.Props.outOfFuelMessage ?? ((string)"NoFuel".Translate());
		}
		if (Props.requiresPower && power != null && !power.PowerOn)
		{
			return "NoPower".Translate().CapitalizeFirst();
		}
		if (activateBy != null)
		{
			if (activateBy.Dead)
			{
				return "PawnIsDead".Translate(activateBy);
			}
			if (Props.activateStat != null && (Props.activateStat.Worker.IsDisabledFor(activateBy) || Props.activateStat.statFactors.Any((StatDef s) => s.Worker.IsDisabledFor(activateBy))))
			{
				return "Incapable".Translate().CapitalizeFirst();
			}
			if (parent.PositionHeld.IsForbidden(activateBy) && !activateBy.Drafted && !Props.ignoreForbidden)
			{
				return "CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + ": " + activateBy.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap.Label;
			}
			if (parent.IsForbidden(activateBy) && !activateBy.Drafted && !Props.ignoreForbidden)
			{
				return "CannotPrioritizeCellForbidden".Translate();
			}
			if (activateBy.Downed)
			{
				return "MessageRitualPawnDowned".Translate(activateBy);
			}
			if (activateBy.Deathresting)
			{
				return "IsDeathresting".Translate(activateBy.Named("PAWN"));
			}
			if (!activateBy.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				return "MessageIncapableOfManipulation".Translate(activateBy);
			}
			if (!activateBy.CanReach(parent.SpawnedParentOrMe, PathEndMode.ClosestTouch, Danger.Deadly))
			{
				return "CannotReach".Translate();
			}
		}
		return true;
	}

	public void Interact(Pawn caster, bool force = false)
	{
		if (CanInteract(caster, checkOptionalItems: false).Accepted || force)
		{
			activeTicks = Props.activeTicks;
			if (!Props.soundActivate.NullOrUndefined())
			{
				Props.soundActivate.PlayOneShot(SoundInfo.InMap(parent));
			}
			if (Props.fleckOnUsed != null)
			{
				FleckMaker.AttachedOverlay(caster, Props.fleckOnUsed, Vector3.zero, Props.fleckOnUsedScale);
			}
			OnInteracted(caster);
			parent.BroadcastCompSignal("Interacted");
			QuestUtility.SendQuestTargetSignals(parent.questTags, "Activated", parent.Named("SUBJECT"), caster.Named("ACTIVATOR"));
			if (!string.IsNullOrEmpty(Props.messageCompletedString))
			{
				Messages.Message(Props.messageCompletedString.Formatted(caster.Named("PAWN")), caster, MessageTypeDefOf.NeutralEvent, historical: false);
			}
		}
	}

	protected virtual void OnInteracted(Pawn caster)
	{
	}

	protected virtual bool TryInteractTick()
	{
		return true;
	}

	protected virtual bool ShouldDeactivate()
	{
		return false;
	}

	protected virtual void Deactivate()
	{
		activeTicks = 0;
	}

	protected virtual void StartCooldown()
	{
		cooldownTicks = Props.cooldownTicks;
	}

	protected virtual void CooldownEnded(bool sendMessage = true)
	{
		progressBarEffecter?.Cleanup();
		progressBarEffecter = null;
		if (sendMessage && !string.IsNullOrEmpty(Props.messageCooldownEnded))
		{
			Messages.Message(Props.messageCooldownEnded, parent, MessageTypeDefOf.NeutralEvent, historical: false);
		}
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		progressBarEffecter?.Cleanup();
		progressBarEffecter = null;
		base.PostDestroy(mode, previousMap);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (HideInteraction)
		{
			yield break;
		}
		if (parent.SpawnedOrAnyParentSpawned)
		{
			string defaultLabel = ((!string.IsNullOrEmpty(Props.activateLabelString)) ? Props.activateLabelString : ((string)("OrderActivation".Translate() + "...")));
			string defaultDesc = (ExtraNamedArg.HasValue ? ((!string.IsNullOrEmpty(Props.activateDescString)) ? ((string)Props.activateDescString.Formatted(parent.Named("THING"), ExtraNamedArg.Value)) : ((string)"OrderActivationDesc".Translate(parent.Named("THING"), ExtraNamedArg.Value))) : ((!string.IsNullOrEmpty(Props.activateDescString)) ? ((string)Props.activateDescString.Formatted(parent.Named("THING"))) : ((string)"OrderActivationDesc".Translate(parent.Named("THING")))));
			Command_Action command_Action = new Command_Action
			{
				defaultLabel = defaultLabel,
				defaultDesc = defaultDesc,
				icon = UIIcon,
				groupable = false,
				action = delegate
				{
					SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
					Find.Targeter.BeginTargeting(this);
				}
			};
			AcceptanceReport acceptanceReport = CanInteract();
			if (!acceptanceReport.Accepted)
			{
				command_Action.Disable(acceptanceReport.Reason.CapitalizeFirst());
			}
			yield return command_Action;
		}
		if (DebugSettings.ShowDevGizmos && OnCooldown)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Reset cooldown",
				action = delegate
				{
					cooldownTicks = 0;
					CooldownEnded();
				}
			};
		}
	}

	public override string CompInspectStringExtra()
	{
		TaggedString taggedString = base.CompInspectStringExtra();
		if (!taggedString.NullOrEmpty())
		{
			taggedString += "\n";
		}
		if (OnCooldown)
		{
			taggedString += Props.onCooldownString.CapitalizeFirst() + ": " + "DurationLeft".Translate(cooldownTicks.ToStringTicksToPeriod()).CapitalizeFirst() + ".";
			if (!CanCooldown)
			{
				taggedString += string.Format(" ({0})", "Paused".Translate());
			}
		}
		if (!Props.inspectString.NullOrEmpty())
		{
			if (!taggedString.NullOrEmpty())
			{
				taggedString += "\n";
			}
			taggedString += Props.inspectString;
		}
		else if (Props.showMustBeActivatedByColonist)
		{
			if (!taggedString.NullOrEmpty())
			{
				taggedString += "\n";
			}
			taggedString += "MustBeActivatedByColonist".Translate();
		}
		if (!Active && Props.maintainProgress && progress > 0f)
		{
			if (Props.remainingSecondsInInspectString)
			{
				taggedString += "\n" + "Activation".Translate() + ": " + Mathf.FloorToInt((float)TicksToActivate * (1f - progress)).ToStringSecondsFromTicks("F0");
			}
			else
			{
				taggedString += "\n" + "Activation".Translate() + ": " + progress.ToStringPercent();
			}
		}
		return taggedString.Resolve();
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (!HideInteraction)
		{
			AcceptanceReport acceptanceReport = CanInteract(selPawn);
			FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(ActivateOptionLabel, delegate
			{
				OrderForceTarget(selPawn);
			}), selPawn, parent);
			if (!acceptanceReport.Accepted)
			{
				floatMenuOption.Disabled = true;
				floatMenuOption.Label = floatMenuOption.Label + " (" + acceptanceReport.Reason.UncapitalizeFirst() + ")";
			}
			yield return floatMenuOption;
		}
	}

	public override void PostExposeData()
	{
		if (string.IsNullOrEmpty(ExposeKey))
		{
			Scribe_Values.Look(ref cooldownTicks, "cooldownTicks", 0);
			Scribe_Values.Look(ref activeTicks, "activeTicks", 0);
			Scribe_Values.Look(ref progress, "progress", 0f);
		}
		else
		{
			Scribe_Values.Look(ref cooldownTicks, ExposeKey + "_cooldownTicks", 0);
			Scribe_Values.Look(ref activeTicks, ExposeKey + "_activeTicks", 0);
			Scribe_Values.Look(ref progress, ExposeKey + "_progress", 0f);
		}
	}

	public bool CanHitTarget(LocalTargetInfo target)
	{
		return ValidateTarget(target, showMessages: false);
	}

	public bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (!target.IsValid || target.Pawn == null)
		{
			return false;
		}
		Pawn pawn = target.Pawn;
		AcceptanceReport acceptanceReport = CanInteract(pawn);
		if (!acceptanceReport.Accepted)
		{
			if (showMessages && !acceptanceReport.Reason.NullOrEmpty())
			{
				Messages.Message("CannotGenericWorkCustom".Translate(Props.jobString) + ": " + acceptanceReport.Reason.CapitalizeFirst(), pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return true;
	}

	public void DrawHighlight(LocalTargetInfo target)
	{
		if (target.IsValid)
		{
			GenDraw.DrawTargetHighlight(target);
		}
	}

	public virtual void OrderForceTarget(LocalTargetInfo target)
	{
		if (ValidateTarget(target, showMessages: false))
		{
			Job job = JobMaker.MakeJob(JobDefOf.InteractThing, parent);
			target.Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		}
	}

	public void OnGUI(LocalTargetInfo target)
	{
		string label = ((!string.IsNullOrEmpty(Props.guiLabelString)) ? Props.guiLabelString : ((string)"ChooseWhoShouldActivate".Translate()));
		Widgets.MouseAttachedLabel(label);
		if (ValidateTarget(target, showMessages: false) && Props.targetingParameters.CanTarget(target.Pawn, this))
		{
			GenUI.DrawMouseAttachment(UIIcon);
		}
		else
		{
			GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
		}
	}

	public void Notify_InteractionStarted()
	{
		if (Props.interactionEffecter != null)
		{
			interactionEffecter = Props.interactionEffecter.Spawn(parent, parent.Map);
		}
	}

	public void Notify_InteractionEnded()
	{
		if (Props.interactionEffecter != null)
		{
			interactionEffecter?.Cleanup();
			interactionEffecter = null;
		}
	}
}
