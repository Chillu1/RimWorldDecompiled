using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class Ability : IVerbOwner, IExposable, ILoadReferenceable
{
	public int Id = -1;

	public Pawn pawn;

	public AbilityDef def;

	public List<AbilityComp> comps;

	protected Command gizmo;

	private VerbTracker verbTracker;

	private bool inCooldown;

	private int cooldownDuration;

	private int cooldownEndTick;

	private Mote warmupMote;

	private Effecter warmupEffecter;

	private Sustainer soundCast;

	private bool wasCastingOnPrevTick;

	private int charges;

	public int maxCharges;

	public int lastCastTick = -99999;

	public Precept sourcePrecept;

	private List<PreCastAction> preCastActions = new List<PreCastAction>();

	private List<Tuple<Effecter, TargetInfo, TargetInfo>> maintainedEffecters = new List<Tuple<Effecter, TargetInfo, TargetInfo>>();

	private List<Mote> customWarmupMotes = new List<Mote>();

	private bool needToRecacheWarmupMotes = true;

	private List<CompAbilityEffect> effectComps;

	private readonly List<LocalTargetInfo> affectedTargetsCached = new List<LocalTargetInfo>();

	private TargetInfo verbTargetInfoTmp = null;

	public Verb verb => verbTracker.PrimaryVerb;

	public List<Tool> Tools { get; private set; }

	public Thing ConstantCaster => pawn;

	public List<VerbProperties> VerbProperties => new List<VerbProperties> { def.verbProperties };

	public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

	public int CooldownTicksRemaining
	{
		get
		{
			if (inCooldown)
			{
				return Mathf.Max(cooldownEndTick - GenTicks.TicksGame, 0);
			}
			return 0;
		}
	}

	public int CooldownTicksTotal => cooldownDuration;

	public bool UsesCharges => maxCharges > 0;

	public int RemainingCharges
	{
		get
		{
			return charges;
		}
		set
		{
			charges = value;
		}
	}

	public string GizmoExtraLabel
	{
		get
		{
			if (!UsesCharges)
			{
				return null;
			}
			return $"{charges} / {maxCharges}";
		}
	}

	public VerbTracker VerbTracker
	{
		get
		{
			if (verbTracker == null)
			{
				verbTracker = new VerbTracker(this);
			}
			return verbTracker;
		}
	}

	public bool HasCooldown
	{
		get
		{
			if (!(def.cooldownTicksRange != default(IntRange)) && (def.groupDef == null || def.groupDef.cooldownTicks <= 0))
			{
				return def.hasExternallyHandledCooldown;
			}
			return true;
		}
	}

	public bool OnCooldown
	{
		get
		{
			if (HasCooldown)
			{
				return inCooldown;
			}
			return false;
		}
	}

	public virtual AcceptanceReport CanCast
	{
		get
		{
			if (!comps.NullOrEmpty())
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (!comps[i].CanCast)
					{
						return false;
					}
				}
			}
			if (inCooldown)
			{
				return def.cooldownPerCharge && charges > 0;
			}
			if (UsesCharges)
			{
				return charges > 0;
			}
			if (pawn.MapHeld != null)
			{
				if (pawn.MapHeld.IsPocketMap && VerbProperties.Any((VerbProperties vp) => !vp.useableInPocketMaps))
				{
					return "CannotUseReason_PocketMap".Translate(pawn.MapHeld.generatorDef.label);
				}
				if (pawn.MapHeld.Biome.inVacuum && VerbProperties.Any((VerbProperties vp) => !vp.useableInVacuum))
				{
					return "CannotFunctionInVacuum".Translate();
				}
				PlanetTile tile = pawn.MapHeld.Tile;
				if (LayerPreventsCast(pawn.MapHeld.Tile))
				{
					PlanetLayerDef layerDef = tile.LayerDef;
					return "CannotPerformPlanetLayer".Translate(layerDef.gerundLabel.Named("GERUND"), layerDef.label.Named("LAYER"));
				}
			}
			return true;
		}
	}

	public bool Casting
	{
		get
		{
			if (!verb.WarmingUp && (!(pawn.jobs?.curDriver is JobDriver_CastAbilityWorld) || pawn.CurJob.ability != this))
			{
				if (pawn?.CurJobDef != null && pawn.CurJobDef.abilityCasting)
				{
					return pawn.CurJob.ability == this;
				}
				return false;
			}
			return true;
		}
	}

	public bool CanCooldown
	{
		get
		{
			if (HasCooldown)
			{
				if (def.waitForJobEnd)
				{
					return pawn.jobs?.curJob?.def != def.jobDef;
				}
				return true;
			}
			return false;
		}
	}

	public virtual string Tooltip
	{
		get
		{
			string text = def.GetTooltip(pawn);
			if (UsesCharges)
			{
				text = ((!def.cooldownPerCharge) ? (text + "\n\n" + "Charges".Translate().ToString() + $": {charges} / {maxCharges}") : (text + "\n\n" + "Uses".Translate().ToString() + $": {charges} / {maxCharges}"));
			}
			if (EffectComps != null)
			{
				foreach (CompAbilityEffect effectComp in EffectComps)
				{
					string text2 = effectComp.ExtraTooltipPart();
					if (!text2.NullOrEmpty())
					{
						text = text + "\n\n" + text2;
					}
				}
			}
			return text;
		}
	}

	public virtual bool CanQueueCast
	{
		get
		{
			if (!CanCast)
			{
				return false;
			}
			if (pawn.jobs == null)
			{
				return false;
			}
			int num = 0;
			foreach (Job item in pawn.jobs.AllJobs())
			{
				if (SameForQueueing(item))
				{
					num++;
					if (!UsesCharges || num >= charges)
					{
						return false;
					}
				}
			}
			return true;
			bool SameForQueueing(Job j)
			{
				if (j.verbToUse != verb)
				{
					if (def.groupDef != null && j.ability != null)
					{
						return j.ability.def.groupDef == def.groupDef;
					}
					return false;
				}
				return true;
			}
		}
	}

	public List<CompAbilityEffect> EffectComps
	{
		get
		{
			if (effectComps == null)
			{
				IEnumerable<CompAbilityEffect> enumerable = CompsOfType<CompAbilityEffect>();
				effectComps = ((enumerable == null) ? new List<CompAbilityEffect>() : enumerable.ToList());
			}
			return effectComps;
		}
	}

	public string UniqueVerbOwnerID()
	{
		return GetUniqueLoadID();
	}

	public bool VerbsStillUsableBy(Pawn p)
	{
		return true;
	}

	private bool LayerPreventsCast(PlanetTile tile)
	{
		if (!tile.Valid)
		{
			return false;
		}
		foreach (VerbProperties verbProperty in VerbProperties)
		{
			if (!verbProperty.layerWhitelist.NullOrEmpty() && !verbProperty.layerWhitelist.Contains(tile.LayerDef))
			{
				return true;
			}
			if (!verbProperty.layerBlacklist.NullOrEmpty() && verbProperty.layerBlacklist.Contains(tile.LayerDef))
			{
				return true;
			}
		}
		return false;
	}

	public Ability()
	{
	}

	public Ability(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public Ability(Pawn pawn, Precept sourcePrecept)
	{
		this.pawn = pawn;
		this.sourcePrecept = sourcePrecept;
	}

	public Ability(Pawn pawn, AbilityDef def)
	{
		this.pawn = pawn;
		this.def = def;
		Initialize();
	}

	public Ability(Pawn pawn, Precept sourcePrecept, AbilityDef def)
	{
		this.pawn = pawn;
		this.def = def;
		this.sourcePrecept = sourcePrecept;
		Initialize();
	}

	public virtual bool CanApplyOn(LocalTargetInfo target)
	{
		if (effectComps != null)
		{
			foreach (CompAbilityEffect effectComp in effectComps)
			{
				if (!effectComp.CanApplyOn(target, null))
				{
					return false;
				}
			}
		}
		if (!def.verbProperties.targetable && def.verbProperties.targetParams.canTargetSelf)
		{
			return target == pawn;
		}
		return true;
	}

	public virtual bool CanApplyOn(GlobalTargetInfo target)
	{
		if (EffectComps != null)
		{
			foreach (CompAbilityEffect effectComp in EffectComps)
			{
				if (!effectComp.CanApplyOn(target))
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual bool AICanTargetNow(LocalTargetInfo target)
	{
		if (!def.aiCanUse || !CanCast)
		{
			return false;
		}
		if (!CanApplyOn(target))
		{
			return false;
		}
		if (EffectComps != null)
		{
			foreach (CompAbilityEffect effectComp in EffectComps)
			{
				if (!effectComp.AICanTargetNow(target))
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual LocalTargetInfo AIGetAOETarget()
	{
		if (def.ai_SearchAOEForTargets)
		{
			foreach (Thing item in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, verb.EffectiveRange, useCenter: true))
			{
				if (!ValidAOEAffectedTarget(item))
				{
					continue;
				}
				bool flag = true;
				foreach (CompAbilityEffect effectComp in EffectComps)
				{
					if (!effectComp.AICanTargetNow(item))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return item;
				}
			}
		}
		return LocalTargetInfo.Invalid;
	}

	public Window ConfirmationDialog(LocalTargetInfo target, Action action)
	{
		if (EffectComps != null)
		{
			foreach (CompAbilityEffect effectComp in effectComps)
			{
				Window window = effectComp.ConfirmationDialog(target, action);
				if (window != null)
				{
					return window;
				}
			}
		}
		if (!def.confirmationDialogText.NullOrEmpty())
		{
			return Dialog_MessageBox.CreateConfirmation(def.confirmationDialogText.Formatted(pawn.Named("PAWN")), action);
		}
		return null;
	}

	public Window ConfirmationDialog(GlobalTargetInfo target, Action action)
	{
		if (EffectComps != null)
		{
			foreach (CompAbilityEffect effectComp in EffectComps)
			{
				Window window = effectComp.ConfirmationDialog(target, action);
				if (window != null)
				{
					return window;
				}
			}
		}
		if (!def.confirmationDialogText.NullOrEmpty())
		{
			return Dialog_MessageBox.CreateConfirmation(def.confirmationDialogText.Formatted(pawn.Named("PAWN")), action);
		}
		return null;
	}

	protected virtual void PreActivate(LocalTargetInfo? target)
	{
		if (UsesCharges)
		{
			charges--;
		}
		if (HasCooldown)
		{
			if (def.groupDef != null)
			{
				int num = (def.overrideGroupCooldown ? def.cooldownTicksRange.RandomInRange : def.groupDef.cooldownTicks);
				foreach (Ability item in this.pawn.abilities.AllAbilitiesForReading)
				{
					item.Notify_GroupStartedCooldown(def.groupDef, num);
				}
				if (this.pawn.Ideo != null)
				{
					foreach (Precept_Ritual item2 in this.pawn.Ideo.PreceptsListForReading.OfType<Precept_Ritual>())
					{
						if (item2.def.useCooldownFromAbilityGroupDef == def.groupDef)
						{
							item2.Notify_CooldownFromAbilityStarted(num);
						}
					}
				}
			}
			else if (UsesCharges)
			{
				if (def.cooldownPerCharge)
				{
					if (charges < maxCharges && CooldownTicksRemaining == 0)
					{
						StartCooldown(def.cooldownTicksRange.RandomInRange);
					}
				}
				else if (charges <= 0)
				{
					StartCooldown(def.cooldownTicksRange.RandomInRange);
				}
			}
			else
			{
				StartCooldown(def.cooldownTicksRange.RandomInRange);
			}
		}
		if (ConstantCaster is Pawn pawn)
		{
			pawn.equipment?.Notify_AbilityUsed(this);
		}
		if (def.writeCombatLog)
		{
			Find.BattleLog.Add(new BattleLogEntry_AbilityUsed(this.pawn, target?.Thing, def, RulePackDefOf.Event_AbilityUsed));
		}
		customWarmupMotes.Clear();
	}

	public virtual bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
	{
		PreActivate(target);
		if (def.hostile && pawn.mindState != null)
		{
			pawn.mindState.lastCombatantTick = Find.TickManager.TicksGame;
		}
		if (EffectComps.Any())
		{
			affectedTargetsCached.Clear();
			affectedTargetsCached.AddRange(GetAffectedTargets(target));
			ApplyEffects(EffectComps, affectedTargetsCached, dest);
		}
		preCastActions.Clear();
		return true;
	}

	public virtual bool Activate(GlobalTargetInfo target)
	{
		PreActivate(null);
		if (def.hostile && pawn.mindState != null)
		{
			pawn.mindState.lastCombatantTick = Find.TickManager.TicksGame;
		}
		if (EffectComps.Any())
		{
			ApplyEffects(EffectComps, target);
		}
		preCastActions.Clear();
		return true;
	}

	public IEnumerable<LocalTargetInfo> GetAffectedTargets(LocalTargetInfo target)
	{
		if (def.HasAreaOfEffect && def.canUseAoeToGetTargets)
		{
			foreach (LocalTargetInfo item in from t in GenRadial.RadialDistinctThingsAround(target.Cell, pawn.Map, def.EffectRadius, useCenter: true).Where(ValidAOEAffectedTarget)
				select new LocalTargetInfo(t))
			{
				yield return item;
			}
		}
		else
		{
			yield return target;
		}
	}

	private bool ValidAOEAffectedTarget(Thing target)
	{
		if (!verb.targetParams.CanTarget(target))
		{
			return false;
		}
		if (target.Fogged())
		{
			return false;
		}
		for (int i = 0; i < EffectComps.Count; i++)
		{
			if (!EffectComps[i].Valid((LocalTargetInfo)target, throwMessages: false))
			{
				return false;
			}
		}
		return true;
	}

	public virtual void QueueCastingJob(LocalTargetInfo target, LocalTargetInfo destination)
	{
		if (!CanQueueCast || !CanApplyOn(target))
		{
			return;
		}
		if (verb.verbProps.nonInterruptingSelfCast)
		{
			verb.TryStartCastOn(verb.Caster);
			return;
		}
		ShowCastingConfirmationIfNeeded(target, delegate
		{
			needToRecacheWarmupMotes = true;
			pawn.jobs.TryTakeOrderedJob(GetJob(target, destination), JobTag.Misc);
		});
	}

	public virtual Job GetJob(LocalTargetInfo target, LocalTargetInfo destination)
	{
		Job job = JobMaker.MakeJob(def.jobDef ?? JobDefOf.CastAbilityOnThing);
		job.verbToUse = verb;
		job.targetA = target;
		job.targetB = destination;
		job.ability = this;
		needToRecacheWarmupMotes = true;
		return job;
	}

	public virtual void QueueCastingJob(GlobalTargetInfo target)
	{
		if (!CanQueueCast || !CanApplyOn(target))
		{
			return;
		}
		ShowCastingConfirmationIfNeeded(target, delegate
		{
			if (!pawn.IsCaravanMember())
			{
				Job job = JobMaker.MakeJob(def.jobDef ?? JobDefOf.CastAbilityOnWorldTile);
				job.verbToUse = verb;
				job.globalTarget = target;
				job.ability = this;
				needToRecacheWarmupMotes = true;
				pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
			else
			{
				Activate(target);
			}
		});
	}

	private void ShowCastingConfirmationIfNeeded(LocalTargetInfo target, Action cast)
	{
		Window window = ConfirmationDialog(target, cast);
		if (window == null)
		{
			cast();
		}
		else
		{
			Find.WindowStack.Add(window);
		}
	}

	private void ShowCastingConfirmationIfNeeded(GlobalTargetInfo target, Action cast)
	{
		Window window = ConfirmationDialog(target, cast);
		if (window == null)
		{
			cast();
		}
		else
		{
			Find.WindowStack.Add(window);
		}
	}

	public bool ValidateGlobalTarget(GlobalTargetInfo target)
	{
		for (int i = 0; i < EffectComps.Count; i++)
		{
			if (!EffectComps[i].Valid(target, throwMessages: true))
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool GizmoDisabled(out string reason)
	{
		if (CanCooldown && OnCooldown && (!def.cooldownPerCharge || charges == 0))
		{
			reason = "AbilityOnCooldown".Translate(CooldownTicksRemaining.ToStringTicksToPeriod()).Resolve();
			return true;
		}
		if (UsesCharges && charges <= 0)
		{
			reason = "AbilityNoCharges".Translate();
			return true;
		}
		if (!comps.NullOrEmpty())
		{
			for (int i = 0; i < comps.Count; i++)
			{
				if (comps[i].GizmoDisabled(out reason))
				{
					return true;
				}
			}
		}
		AcceptanceReport canCast = CanCast;
		if (!canCast.Accepted)
		{
			reason = canCast.Reason;
			return true;
		}
		Lord lord = pawn.GetLord();
		if (lord != null)
		{
			AcceptanceReport acceptanceReport = lord.AbilityAllowed(this);
			if (!acceptanceReport)
			{
				reason = acceptanceReport.Reason;
				return true;
			}
		}
		if (!pawn.Drafted && def.disableGizmoWhileUndrafted && pawn.GetCaravan() == null && !DebugSettings.ShowDevGizmos)
		{
			reason = "AbilityDisabledUndrafted".Translate();
			return true;
		}
		if (pawn.DevelopmentalStage.Baby())
		{
			reason = "IsIncapped".Translate(pawn.LabelShort, pawn);
			return true;
		}
		if (pawn.Downed)
		{
			reason = "CommandDisabledUnconscious".TranslateWithBackup("CommandCallRoyalAidUnconscious").Formatted(pawn);
			return true;
		}
		if (pawn.Deathresting)
		{
			reason = "CommandDisabledDeathresting".Translate(pawn);
			return true;
		}
		if (def.casterMustBeCapableOfViolence && pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			reason = "IsIncapableOfViolence".Translate(pawn.LabelShort, pawn);
			return true;
		}
		if (!CanQueueCast)
		{
			reason = "AbilityAlreadyQueued".Translate();
			return true;
		}
		reason = null;
		return false;
	}

	public virtual void AbilityTick()
	{
		VerbTracker.VerbsTick();
		if (def.emittedFleck != null && Casting && pawn.IsHashIntervalTick(def.emissionInterval))
		{
			FleckMaker.ThrowMetaIcon(verb.CurrentTarget.Cell, pawn.Map, def.emittedFleck, 0.75f);
		}
		MoteTick();
		EffectersTick();
		WarmupTick();
		CastingTick();
		SoundTick();
		CooldownTick();
		if (!comps.NullOrEmpty())
		{
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].CompTick();
			}
		}
		if (wasCastingOnPrevTick && !Casting)
		{
			lastCastTick = Find.TickManager.TicksGame;
		}
		wasCastingOnPrevTick = Casting;
	}

	private void MoteTick()
	{
		if ((def.warmupMote == null && def.WarmupMoteSocialSymbol == null) || !Casting)
		{
			return;
		}
		Vector3 vector = pawn.DrawPos + def.moteDrawOffset;
		vector += (verb.CurrentTarget.CenterVector3 - vector) * def.moteOffsetAmountTowardsTarget;
		if (warmupMote == null || warmupMote.Destroyed)
		{
			if (def.WarmupMoteSocialSymbol == null)
			{
				if (def.warmupMote.thingClass != typeof(MoteAttached))
				{
					warmupMote = MoteMaker.MakeStaticMote(vector, pawn.Map, def.warmupMote);
				}
				else
				{
					warmupMote = MoteMaker.MakeAttachedOverlay((verb.CurrentTarget.Thing as Corpse) ?? verb.CurrentTarget.Thing, def.warmupMote, Vector3.zero);
				}
			}
			else
			{
				warmupMote = MoteMaker.MakeInteractionBubble(pawn, verb.CurrentTarget.Pawn, ThingDefOf.Mote_Speech, def.WarmupMoteSocialSymbol);
			}
		}
		else
		{
			if (!(warmupMote is MoteAttached))
			{
				warmupMote.exactPosition = vector;
			}
			warmupMote.Maintain();
		}
	}

	private void EffectersTick()
	{
		for (int num = maintainedEffecters.Count - 1; num >= 0; num--)
		{
			Effecter item = maintainedEffecters[num].Item1;
			if (item.ticksLeft > 0)
			{
				TargetInfo item2 = maintainedEffecters[num].Item2;
				TargetInfo item3 = maintainedEffecters[num].Item3;
				item.EffectTick(item2, item3);
				item.ticksLeft--;
			}
			else
			{
				item.Cleanup();
				maintainedEffecters.RemoveAt(num);
			}
		}
	}

	private void CooldownTick()
	{
		if (!inCooldown || !CanCooldown || CooldownTicksRemaining > 0)
		{
			return;
		}
		inCooldown = false;
		if (UsesCharges && def.cooldownPerCharge)
		{
			charges = Mathf.Min(++charges, maxCharges);
			if (charges < maxCharges)
			{
				StartCooldown(def.cooldownTicksRange.RandomInRange);
			}
		}
		if (PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			if (def.sendLetterOnCooldownComplete)
			{
				Find.LetterStack.ReceiveLetter("AbilityReadyLabel".Translate(def.LabelCap), "AbilityReadyText".Translate(pawn, def.label), LetterDefOf.NeutralEvent, new LookTargets(pawn));
			}
			if (def.sendMessageOnCooldownComplete && !pawn.IsPrisoner)
			{
				Messages.Message("AbilityReadyLabel".Translate(def.LabelCap, pawn.Named("PAWN")), pawn, MessageTypeDefOf.NeutralEvent);
			}
		}
		if (def.groupDef != null && def.groupDef.sendMessageOnCooldownComplete && MessagesRepeatAvoider.MessageShowAllowed(def.groupDef.label + "_offcooldown", 1f))
		{
			string text = (from a in pawn.abilities.AllAbilitiesForReading
				where a.def.groupDef == def.groupDef
				select a.def.label).ToCommaList();
			Messages.Message("AbilitiesReadyLabel".Translate(text, pawn.Named("PAWN")), pawn, MessageTypeDefOf.NeutralEvent);
		}
	}

	private void CastingTick()
	{
		if (!Casting)
		{
			if (warmupEffecter != null)
			{
				warmupEffecter.Cleanup();
				warmupEffecter = null;
			}
			return;
		}
		SetupWarmupEffecter(verb.CurrentTarget);
		warmupEffecter?.EffectTick(verbTargetInfoTmp, verbTargetInfoTmp);
		if (needToRecacheWarmupMotes && !EffectComps.NullOrEmpty() && verb.CurrentTarget.Thing != null)
		{
			customWarmupMotes.Clear();
			foreach (CompAbilityEffect effectComp in effectComps)
			{
				foreach (Mote item in effectComp.CustomWarmupMotes(verb.CurrentTarget))
				{
					customWarmupMotes.Add(item);
				}
			}
			needToRecacheWarmupMotes = false;
		}
		foreach (Mote customWarmupMote in customWarmupMotes)
		{
			customWarmupMote.Maintain();
		}
	}

	private void WarmupTick()
	{
		if (!verb.WarmingUp)
		{
			return;
		}
		if (!(def.targetWorldCell ? CanApplyOn(pawn.CurJob.globalTarget) : CanApplyOn(verb.CurrentTarget)) && def.targetRequired)
		{
			if (def.targetWorldCell)
			{
				pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
			}
			verb.WarmupStance?.Interrupt();
			verb.Reset();
			customWarmupMotes.Clear();
			preCastActions.Clear();
			return;
		}
		for (int num = preCastActions.Count - 1; num >= 0; num--)
		{
			if (preCastActions[num].ticksAwayFromCast >= verb.WarmupTicksLeft)
			{
				preCastActions[num].action(verb.CurrentTarget, verb.CurrentDestination);
				preCastActions.RemoveAt(num);
			}
		}
	}

	private void SoundTick()
	{
		if (!pawn.Spawned || !Casting)
		{
			return;
		}
		if (def.warmupSound != null)
		{
			if (soundCast == null || soundCast.Ended)
			{
				soundCast = def.warmupSound.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map), MaintenanceType.PerTick));
			}
			else
			{
				soundCast.Maintain();
			}
		}
		if (!wasCastingOnPrevTick && def.warmupStartSound != null)
		{
			def.warmupStartSound.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
		}
		if (def.warmupPreEndSound != null && verb.WarmupTicksLeft == def.warmupPreEndSoundSeconds.SecondsToTicks())
		{
			def.warmupPreEndSound.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
		}
	}

	private void SetupWarmupEffecter(LocalTargetInfo target)
	{
		if (warmupEffecter != null || def.warmupEffecter == null)
		{
			return;
		}
		if (!def.useAverageTargetPositionForWarmupEffecter || EffectComps.NullOrEmpty())
		{
			if (target.Thing != null)
			{
				warmupEffecter = def.warmupEffecter.SpawnAttached(target.Thing, pawn.MapHeld);
				verbTargetInfoTmp = target.Thing;
			}
			else
			{
				warmupEffecter = def.warmupEffecter.Spawn(target.Cell, pawn.MapHeld);
				verbTargetInfoTmp = new TargetInfo(target.Cell, pawn.MapHeld);
			}
		}
		else
		{
			Vector3 zero = Vector3.zero;
			IEnumerable<LocalTargetInfo> affectedTargets = GetAffectedTargets(target);
			foreach (LocalTargetInfo item in affectedTargets)
			{
				zero += item.Cell.ToVector3Shifted();
			}
			zero /= (float)affectedTargets.Count();
			warmupEffecter = def.warmupEffecter.Spawn(zero.ToIntVec3(), pawn.MapHeld);
			verbTargetInfoTmp = new TargetInfo(zero.ToIntVec3(), pawn.MapHeld);
		}
		warmupEffecter.Trigger(verbTargetInfoTmp, verbTargetInfoTmp);
	}

	public void Notify_StartedCasting()
	{
		for (int i = 0; i < EffectComps.Count; i++)
		{
			preCastActions.AddRange(EffectComps[i].GetPreCastActions());
		}
	}

	public void DrawEffectPreviews(LocalTargetInfo target)
	{
		for (int i = 0; i < EffectComps.Count; i++)
		{
			EffectComps[i].DrawEffectPreview(target);
		}
	}

	public bool GizmosVisible()
	{
		if (!EffectComps.NullOrEmpty())
		{
			foreach (CompAbilityEffect effectComp in EffectComps)
			{
				if (effectComp.ShouldHideGizmo)
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual IEnumerable<Command> GetGizmos()
	{
		if (gizmo == null)
		{
			gizmo = (Command)Activator.CreateInstance(def.gizmoClass, this, pawn);
			gizmo.Order = def.uiOrder;
		}
		if (!pawn.Drafted || def.showWhenDrafted)
		{
			yield return gizmo;
		}
		if (DebugSettings.ShowDevGizmos && inCooldown && CanCooldown)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Reset cooldown",
				action = delegate
				{
					inCooldown = false;
					charges = maxCharges;
				}
			};
		}
	}

	public virtual IEnumerable<Gizmo> GetGizmosExtra()
	{
		if (comps == null)
		{
			yield break;
		}
		foreach (AbilityComp comp in comps)
		{
			foreach (Gizmo item in comp.CompGetGizmosExtra())
			{
				yield return item;
			}
		}
	}

	public string GetInspectString()
	{
		if (comps.NullOrEmpty())
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < comps.Count; i++)
		{
			string text = comps[i].CompInspectStringExtra();
			if (!text.NullOrEmpty())
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(text);
			}
		}
		return stringBuilder.ToString();
	}

	private void ApplyEffects(IEnumerable<CompAbilityEffect> effects, List<LocalTargetInfo> targets, LocalTargetInfo dest)
	{
		foreach (LocalTargetInfo target in targets)
		{
			ApplyEffects(effects, target, dest);
		}
		foreach (CompAbilityEffect effect in effects)
		{
			effect.PostApplied(targets, pawn.MapHeld);
		}
	}

	public void StartCooldown(int ticks)
	{
		inCooldown = true;
		cooldownEndTick = GenTicks.TicksGame + ticks;
		cooldownDuration = ticks;
		if (!def.cooldownPerCharge)
		{
			charges = maxCharges;
		}
	}

	public void ResetCooldown()
	{
		inCooldown = false;
	}

	public void Notify_GroupStartedCooldown(AbilityGroupDef group, int ticks)
	{
		if (group == def.groupDef)
		{
			StartCooldown(ticks);
		}
	}

	protected virtual void ApplyEffects(IEnumerable<CompAbilityEffect> effects, LocalTargetInfo target, LocalTargetInfo dest)
	{
		foreach (CompAbilityEffect effect in effects)
		{
			effect.Apply(target, dest);
		}
	}

	protected virtual void ApplyEffects(IEnumerable<CompAbilityEffect> effects, GlobalTargetInfo target)
	{
		foreach (CompAbilityEffect effect in effects)
		{
			effect.Apply(target);
		}
	}

	public virtual void OnGizmoUpdate()
	{
		foreach (CompAbilityEffect effectComp in EffectComps)
		{
			effectComp.OnGizmoUpdate();
		}
	}

	public IEnumerable<T> CompsOfType<T>() where T : AbilityComp
	{
		return comps?.Where((AbilityComp c) => c is T).Cast<T>();
	}

	public T CompOfType<T>() where T : AbilityComp
	{
		return comps?.FirstOrDefault((AbilityComp c) => c is T) as T;
	}

	public void Initialize()
	{
		if (def.comps.Any())
		{
			comps = new List<AbilityComp>();
			for (int i = 0; i < def.comps.Count; i++)
			{
				AbilityComp abilityComp = null;
				try
				{
					abilityComp = (AbilityComp)Activator.CreateInstance(def.comps[i].compClass);
					abilityComp.parent = this;
					comps.Add(abilityComp);
					abilityComp.Initialize(def.comps[i]);
				}
				catch (Exception ex)
				{
					Log.Error("Could not instantiate or initialize an AbilityComp: " + ex);
					comps.Remove(abilityComp);
				}
			}
		}
		if (Id == -1)
		{
			Id = Find.UniqueIDsManager.GetNextAbilityID();
		}
		if (VerbTracker.PrimaryVerb is IAbilityVerb abilityVerb)
		{
			abilityVerb.Ability = this;
		}
		if (def.charges > 0)
		{
			maxCharges = def.charges;
			charges = maxCharges;
		}
	}

	public float FinalPsyfocusCost(LocalTargetInfo target)
	{
		if (def.AnyCompOverridesPsyfocusCost)
		{
			foreach (AbilityComp comp in comps)
			{
				if (comp.props.OverridesPsyfocusCost)
				{
					return comp.PsyfocusCostForTarget(target);
				}
			}
		}
		return def.PsyfocusCost;
	}

	public float HemogenCost()
	{
		if (comps != null)
		{
			foreach (AbilityComp comp in comps)
			{
				if (comp is CompAbilityEffect_HemogenCost compAbilityEffect_HemogenCost)
				{
					return compAbilityEffect_HemogenCost.Props.hemogenCost;
				}
			}
		}
		return 0f;
	}

	public TaggedString WorldMapExtraLabel(GlobalTargetInfo t)
	{
		foreach (CompAbilityEffect effectComp in EffectComps)
		{
			string text = effectComp.WorldMapExtraLabel(t);
			if (text != null)
			{
				return text;
			}
		}
		return null;
	}

	public void AddEffecterToMaintain(Effecter eff, IntVec3 pos, int ticks, Map map = null)
	{
		eff.ticksLeft = ticks;
		TargetInfo targetInfo = new TargetInfo(pos, map ?? pawn.Map);
		maintainedEffecters.Add(new Tuple<Effecter, TargetInfo, TargetInfo>(eff, targetInfo, targetInfo));
	}

	public void AddEffecterToMaintain(Effecter eff, IntVec3 posA, IntVec3 posB, int ticks, Map map = null)
	{
		eff.ticksLeft = ticks;
		TargetInfo item = new TargetInfo(posA, map ?? pawn.Map);
		TargetInfo item2 = new TargetInfo(posB, map ?? pawn.Map);
		maintainedEffecters.Add(new Tuple<Effecter, TargetInfo, TargetInfo>(eff, item, item2));
	}

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		if (def == null)
		{
			return;
		}
		Scribe_Values.Look(ref Id, "Id", -1);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (Id == -1)
			{
				Id = Find.UniqueIDsManager.GetNextAbilityID();
			}
			Initialize();
		}
		Scribe_References.Look(ref sourcePrecept, "sourcePrecept");
		Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
		Scribe_Values.Look(ref inCooldown, "inCooldown", defaultValue: false);
		Scribe_Values.Look(ref cooldownEndTick, "cooldownEndTick", 0);
		Scribe_Values.Look(ref cooldownDuration, "cooldownDuration", 0);
		Scribe_Values.Look(ref maxCharges, "maxCharges", def.charges);
		Scribe_Values.Look(ref charges, "charges", def.charges);
		Scribe_Values.Look(ref lastCastTick, "lastCastTick", -99999);
		ExposeBackCompatability();
		if (comps != null)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].PostExposeData();
			}
		}
	}

	private void ExposeBackCompatability()
	{
		int value = 0;
		int value2 = 0;
		Scribe_Values.Look(ref value, "cooldownTicks", 0);
		Scribe_Values.Look(ref value2, "cooldownTicksDuration", 0);
		if (Scribe.mode == LoadSaveMode.LoadingVars && value != 0 && value2 != 0)
		{
			inCooldown = true;
			cooldownDuration = value2;
			cooldownEndTick = GenTicks.TicksGame + value;
		}
	}

	public string GetUniqueLoadID()
	{
		return "Ability_" + Id;
	}
}
