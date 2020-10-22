using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class Ability : IVerbOwner, IExposable, ILoadReferenceable
	{
		public Pawn pawn;

		public AbilityDef def;

		public List<AbilityComp> comps;

		protected Command gizmo;

		private VerbTracker verbTracker;

		private int cooldownTicks;

		private int cooldownTicksDuration;

		private Mote warmupMote;

		private Sustainer soundCast;

		private bool wasCastingOnPrevTick;

		private List<PreCastAction> preCastActions = new List<PreCastAction>();

		private List<Pair<Effecter, TargetInfo>> maintainedEffecters = new List<Pair<Effecter, TargetInfo>>();

		private List<CompAbilityEffect> effectComps;

		public Verb verb => verbTracker.PrimaryVerb;

		public List<Tool> Tools
		{
			get;
			private set;
		}

		public Thing ConstantCaster => pawn;

		public List<VerbProperties> VerbProperties => new List<VerbProperties>
		{
			def.verbProperties
		};

		public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

		public int CooldownTicksRemaining => cooldownTicks;

		public int CooldownTicksTotal => cooldownTicksDuration;

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

		public bool HasCooldown => def.cooldownTicksRange != default(IntRange);

		public virtual bool CanCast => cooldownTicks <= 0;

		public bool Casting
		{
			get
			{
				if (!verb.WarmingUp)
				{
					if (pawn.jobs?.curDriver is JobDriver_CastAbilityWorld)
					{
						return pawn.CurJob.ability == this;
					}
					return false;
				}
				return true;
			}
		}

		public virtual bool CanQueueCast
		{
			get
			{
				if (HasCooldown)
				{
					if (pawn.jobs.curJob != null && pawn.jobs.curJob.verbToUse == verb)
					{
						return false;
					}
					return !pawn.jobs.jobQueue.Where((QueuedJob qj) => qj.job.verbToUse == verb).Any();
				}
				return true;
			}
		}

		public List<CompAbilityEffect> EffectComps
		{
			get
			{
				if (effectComps == null)
				{
					effectComps = CompsOfType<CompAbilityEffect>().ToList();
				}
				return effectComps;
			}
		}

		public string UniqueVerbOwnerID()
		{
			return "Ability_" + def.label + pawn.ThingID;
		}

		public bool VerbsStillUsableBy(Pawn p)
		{
			return true;
		}

		public Ability(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public Ability(Pawn pawn, AbilityDef def)
		{
			this.pawn = pawn;
			this.def = def;
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
			return true;
		}

		public virtual bool CanApplyOn(GlobalTargetInfo target)
		{
			if (effectComps != null)
			{
				foreach (CompAbilityEffect effectComp in effectComps)
				{
					if (!effectComp.CanApplyOn(target))
					{
						return false;
					}
				}
			}
			return true;
		}

		public string ConfirmationDialogText(LocalTargetInfo target)
		{
			if (effectComps != null)
			{
				foreach (CompAbilityEffect effectComp in effectComps)
				{
					string text = effectComp.ConfirmationDialogText(target);
					if (!text.NullOrEmpty())
					{
						return text;
					}
				}
			}
			return def.confirmationDialogText;
		}

		public string ConfirmationDialogText(GlobalTargetInfo target)
		{
			if (effectComps != null)
			{
				foreach (CompAbilityEffect effectComp in effectComps)
				{
					string text = effectComp.ConfirmationDialogText(target);
					if (!text.NullOrEmpty())
					{
						return text;
					}
				}
			}
			return def.confirmationDialogText;
		}

		public virtual bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (!EffectComps.Any())
			{
				return false;
			}
			ApplyEffects(EffectComps, GetAffectedTargets(target), dest);
			if (def.writeCombatLog)
			{
				Find.BattleLog.Add(new BattleLogEntry_AbilityUsed(pawn, target.Thing, def, RulePackDefOf.Event_AbilityUsed));
			}
			preCastActions.Clear();
			return true;
		}

		public virtual bool Activate(GlobalTargetInfo target)
		{
			if (!EffectComps.Any())
			{
				return false;
			}
			ApplyEffects(EffectComps, target);
			if (def.writeCombatLog)
			{
				Find.BattleLog.Add(new BattleLogEntry_AbilityUsed(pawn, null, def, RulePackDefOf.Event_AbilityUsed));
			}
			preCastActions.Clear();
			return true;
		}

		public IEnumerable<LocalTargetInfo> GetAffectedTargets(LocalTargetInfo target)
		{
			if (def.HasAreaOfEffect && def.canUseAoeToGetTargets)
			{
				foreach (LocalTargetInfo item in from t in GenRadial.RadialDistinctThingsAround(target.Cell, pawn.Map, def.EffectRadius, useCenter: true)
					where verb.targetParams.CanTarget(t) && !t.Fogged()
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

		public virtual void QueueCastingJob(LocalTargetInfo target, LocalTargetInfo destination)
		{
			if (CanQueueCast && CanApplyOn(target))
			{
				ShowCastingConfirmationIfNeeded(target, delegate
				{
					Job job = JobMaker.MakeJob(def.jobDef ?? JobDefOf.CastAbilityOnThing);
					job.verbToUse = verb;
					job.targetA = target;
					job.targetB = destination;
					job.ability = this;
					pawn.jobs.TryTakeOrderedJob(job);
				});
			}
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
					pawn.jobs.TryTakeOrderedJob(job);
				}
				else
				{
					Activate(target);
				}
			});
		}

		private void ShowCastingConfirmationIfNeeded(LocalTargetInfo target, Action cast)
		{
			string str = ConfirmationDialogText(target);
			if (str.NullOrEmpty())
			{
				cast();
				return;
			}
			Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation(str.Formatted(pawn.Named("PAWN")), cast);
			Find.WindowStack.Add(window);
		}

		private void ShowCastingConfirmationIfNeeded(GlobalTargetInfo target, Action cast)
		{
			string str = ConfirmationDialogText(target);
			if (str.NullOrEmpty())
			{
				cast();
				return;
			}
			Dialog_MessageBox window = Dialog_MessageBox.CreateConfirmation(str.Formatted(pawn.Named("PAWN")), cast);
			Find.WindowStack.Add(window);
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
			if (!CanCast)
			{
				reason = "AbilityOnCooldown".Translate(cooldownTicks.ToStringTicksToPeriod(allowSeconds: true, shortForm: false, canUseDecimals: false));
				return true;
			}
			if (!CanQueueCast)
			{
				reason = "AbilityAlreadyQueued".Translate();
				return true;
			}
			if (!pawn.Drafted && def.disableGizmoWhileUndrafted && pawn.GetCaravan() == null)
			{
				reason = "AbilityDisabledUndrafted".Translate();
				return true;
			}
			if (pawn.Downed)
			{
				reason = "CommandDisabledUnconscious".TranslateWithBackup("CommandCallRoyalAidUnconscious").Formatted(pawn);
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
			reason = null;
			return false;
		}

		public virtual void AbilityTick()
		{
			VerbTracker.VerbsTick();
			if (def.warmupMote != null && Casting)
			{
				Vector3 vector = pawn.DrawPos + def.moteDrawOffset;
				vector += (verb.CurrentTarget.CenterVector3 - vector) * def.moteOffsetAmountTowardsTarget;
				if (warmupMote == null || warmupMote.Destroyed)
				{
					warmupMote = MoteMaker.MakeStaticMote(vector, pawn.Map, def.warmupMote);
				}
				else
				{
					warmupMote.exactPosition = vector;
					warmupMote.Maintain();
				}
			}
			if (verb.WarmingUp)
			{
				if (!(def.targetWorldCell ? CanApplyOn(pawn.CurJob.globalTarget) : CanApplyOn(verb.CurrentTarget)))
				{
					if (def.targetWorldCell)
					{
						pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
					verb.WarmupStance?.Interrupt();
					verb.Reset();
					preCastActions.Clear();
				}
				else
				{
					for (int num = preCastActions.Count - 1; num >= 0; num--)
					{
						if (preCastActions[num].ticksAwayFromCast >= verb.WarmupTicksLeft)
						{
							preCastActions[num].action(verb.CurrentTarget, verb.CurrentDestination);
							preCastActions.RemoveAt(num);
						}
					}
				}
			}
			if (pawn.Spawned && Casting)
			{
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
			if (cooldownTicks > 0)
			{
				cooldownTicks--;
				if (cooldownTicks == 0 && def.sendLetterOnCooldownComplete)
				{
					Find.LetterStack.ReceiveLetter("AbilityReadyLabel".Translate(def.LabelCap), "AbilityReadyText".Translate(pawn, def.label), LetterDefOf.NeutralEvent, new LookTargets(pawn));
				}
			}
			for (int num2 = maintainedEffecters.Count - 1; num2 >= 0; num2--)
			{
				Effecter first = maintainedEffecters[num2].First;
				if (first.ticksLeft > 0)
				{
					TargetInfo second = maintainedEffecters[num2].Second;
					first.EffectTick(second, second);
					first.ticksLeft--;
				}
				else
				{
					first.Cleanup();
					maintainedEffecters.RemoveAt(num2);
				}
			}
			wasCastingOnPrevTick = Casting;
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

		public virtual IEnumerable<Command> GetGizmos()
		{
			if (gizmo == null)
			{
				gizmo = (Command)Activator.CreateInstance(def.gizmoClass, this);
			}
			yield return gizmo;
			if (Prefs.DevMode && cooldownTicks > 0)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "Reset cooldown";
				command_Action.action = delegate
				{
					cooldownTicks = 0;
				};
				yield return command_Action;
			}
		}

		private void ApplyEffects(IEnumerable<CompAbilityEffect> effects, IEnumerable<LocalTargetInfo> targets, LocalTargetInfo dest)
		{
			foreach (LocalTargetInfo target in targets)
			{
				ApplyEffects(effects, target, dest);
			}
			if (HasCooldown)
			{
				StartCooldown(def.cooldownTicksRange.RandomInRange);
			}
		}

		public void StartCooldown(int ticks)
		{
			cooldownTicksDuration = ticks;
			cooldownTicks = cooldownTicksDuration;
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

		public IEnumerable<T> CompsOfType<T>() where T : AbilityComp
		{
			if (comps == null)
			{
				return null;
			}
			return comps.Where((AbilityComp c) => c is T).Cast<T>();
		}

		public T CompOfType<T>() where T : AbilityComp
		{
			if (comps == null)
			{
				return null;
			}
			return comps.FirstOrDefault((AbilityComp c) => c is T) as T;
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
					catch (Exception arg)
					{
						Log.Error("Could not instantiate or initialize an AbilityComp: " + arg);
						comps.Remove(abilityComp);
					}
				}
			}
			Verb_CastAbility verb_CastAbility = VerbTracker.PrimaryVerb as Verb_CastAbility;
			if (verb_CastAbility != null)
			{
				verb_CastAbility.ability = this;
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

		public string WorldMapExtraLabel(GlobalTargetInfo t)
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

		public void AddEffecterToMaintain(Effecter eff, IntVec3 pos, int ticks)
		{
			eff.ticksLeft = ticks;
			maintainedEffecters.Add(new Pair<Effecter, TargetInfo>(eff, new TargetInfo(pos, pawn.Map)));
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			if (def != null)
			{
				Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
				Scribe_Values.Look(ref cooldownTicks, "cooldownTicks", 0);
				Scribe_Values.Look(ref cooldownTicksDuration, "cooldownTicksDuration", 0);
				if (Scribe.mode == LoadSaveMode.PostLoadInit)
				{
					Initialize();
				}
			}
		}

		public string GetUniqueLoadID()
		{
			return pawn.ThingID + "_Ability_" + def.defName;
		}
	}
}
