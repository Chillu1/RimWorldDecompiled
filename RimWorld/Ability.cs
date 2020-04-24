using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Ability : IVerbOwner, IExposable
	{
		public Pawn pawn;

		public AbilityDef def;

		public List<AbilityComp> comps;

		protected Command gizmo;

		private VerbTracker verbTracker;

		private int cooldownTicks;

		private int cooldownTicksDuration;

		private Mote warmupMote;

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
			foreach (CompAbilityEffect effectComp in effectComps)
			{
				if (!effectComp.CanApplyOn(target, null))
				{
					return false;
				}
			}
			return true;
		}

		public virtual bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
		{
			if (!EffectComps.Any())
			{
				return false;
			}
			ApplyEffects(EffectComps, GetAffectedTargets(target), dest);
			Find.BattleLog.Add(new BattleLogEntry_AbilityUsed(pawn, target.Thing, def, RulePackDefOf.Event_AbilityUsed));
			return true;
		}

		public IEnumerable<LocalTargetInfo> GetAffectedTargets(LocalTargetInfo target)
		{
			if (def.HasAreaOfEffect && def.canUseAoeToGetTargets)
			{
				foreach (LocalTargetInfo item in from t in GenRadial.RadialDistinctThingsAround(target.Cell, pawn.Map, def.EffectRadius, useCenter: true)
					where verb.targetParams.CanTarget(t)
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
				Job job = JobMaker.MakeJob(def.jobDef ?? JobDefOf.CastAbilityOnThing);
				job.verbToUse = verb;
				job.targetA = target;
				job.targetB = destination;
				pawn.jobs.TryTakeOrderedJob(job);
			}
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
			if (!pawn.Drafted && def.disableGizmoWhileUndrafted)
			{
				reason = "AbilityDisabledUndrafted".Translate();
				return true;
			}
			if (pawn.Downed)
			{
				reason = "CommandDisabledUnconscious".TranslateWithBackup("CommandCallRoyalAidUnconscious").Formatted(pawn);
				return true;
			}
			for (int i = 0; i < comps.Count; i++)
			{
				if (comps[i].GizmoDisabled(out reason))
				{
					return true;
				}
			}
			reason = null;
			return false;
		}

		public virtual void AbilityTick()
		{
			VerbTracker.VerbsTick();
			if (def.warmupMote != null && verb.WarmingUp)
			{
				if (warmupMote == null || warmupMote.Destroyed)
				{
					warmupMote = MoteMaker.MakeStaticMote(pawn.DrawPos + def.moteDrawOffset, pawn.Map, def.warmupMote);
				}
				else
				{
					warmupMote.Maintain();
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
	}
}
