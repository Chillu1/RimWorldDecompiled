using RimWorld;

namespace Verse.AI;

public class MentalState : IExposable
{
	public Pawn pawn;

	public MentalStateDef def;

	protected int age;

	public bool causedByMood;

	public bool causedByDamage;

	public bool causedByPsycast;

	public Pawn causedByPawn;

	public int forceRecoverAfterTicks = -1;

	public Faction sourceFaction;

	protected const int MentalStateTickInterval = 30;

	public int Age => age;

	public virtual string InspectLine => def.baseInspectLine;

	protected virtual bool CanEndBeforeMaxDurationNow => true;

	public virtual bool AllowRestingInBed => true;

	public virtual void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_Values.Look(ref age, "age", 0);
		Scribe_Values.Look(ref causedByMood, "causedByMood", defaultValue: false);
		Scribe_Values.Look(ref causedByDamage, "causedByDamage", defaultValue: false);
		Scribe_Values.Look(ref causedByPsycast, "causedByPsycast", defaultValue: false);
		Scribe_References.Look(ref causedByPawn, "causedByPawn");
		Scribe_Values.Look(ref forceRecoverAfterTicks, "forceRecoverAfterTicks", 0);
		Scribe_References.Look(ref sourceFaction, "sourceFaction");
	}

	public virtual void PostStart(string reason)
	{
	}

	public virtual void PreStart()
	{
	}

	public virtual void PostEnd()
	{
		if (!def.recoveryMessage.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			TaggedString taggedString = def.recoveryMessage.Formatted(pawn.LabelShort, pawn.Named("PAWN"));
			if (!taggedString.NullOrEmpty())
			{
				Messages.Message(taggedString.AdjustedFor(pawn).CapitalizeFirst(), pawn, MessageTypeDefOf.SituationResolved);
			}
		}
	}

	public virtual void MentalStateTick(int delta)
	{
		if (pawn.IsHashIntervalTick(30, delta))
		{
			age += 30;
			if (age >= def.maxTicksBeforeRecovery || (age >= def.minTicksBeforeRecovery && CanEndBeforeMaxDurationNow && Rand.MTBEventOccurs(def.recoveryMtbDays, 60000f, 30f)) || (forceRecoverAfterTicks != -1 && age >= forceRecoverAfterTicks))
			{
				RecoverFromState();
			}
			else if (def.recoverFromSleep && !pawn.Awake())
			{
				RecoverFromState();
			}
		}
	}

	public void RecoverFromState()
	{
		if (pawn.MentalState != this)
		{
			Log.Error("Recovered from " + def?.ToString() + " but pawn's mental state is not this, it is " + pawn.MentalState);
		}
		if (!pawn.Dead)
		{
			pawn.mindState.mentalStateHandler.ClearMentalStateDirect();
			if (causedByMood && def.moodRecoveryThought != null && pawn.needs.mood != null)
			{
				pawn.needs.mood.thoughts.memories.TryGainMemory(def.moodRecoveryThought);
			}
			pawn.mindState.mentalBreaker.Notify_RecoveredFromMentalState();
			pawn.mindState.mentalFitGenerator.Notify_RecoveredFromMentalState();
			if (pawn.story != null && pawn.story.traits != null)
			{
				foreach (Trait allTrait in pawn.story.traits.allTraits)
				{
					if (!allTrait.Suppressed)
					{
						allTrait.Notify_MentalStateEndedOn(pawn, causedByMood);
					}
				}
			}
			if (def.IsAggro)
			{
				pawn.mindState.enemyTarget = null;
			}
		}
		if (def.stopsJobs && pawn.Spawned)
		{
			pawn.jobs.StopAll(ifLayingKeepLaying: true);
		}
		PostEnd();
	}

	public virtual bool ForceHostileTo(Thing t)
	{
		return false;
	}

	public virtual bool ForceHostileTo(Faction f)
	{
		return false;
	}

	public EffecterDef CurrentStateEffecter()
	{
		return def.stateEffecter;
	}

	public virtual RandomSocialMode SocialModeMax()
	{
		return RandomSocialMode.SuperActive;
	}

	public virtual TaggedString GetBeginLetterText()
	{
		if (def.beginLetter.NullOrEmpty())
		{
			return null;
		}
		return def.beginLetter.Formatted(pawn.NameShortColored, pawn.Named("PAWN")).AdjustedFor(pawn).Resolve()
			.CapitalizeFirst();
	}

	public virtual void Notify_AttackedTarget(LocalTargetInfo hitTarget)
	{
	}

	public virtual void Notify_SlaughteredTarget()
	{
	}

	public virtual void Notify_ReleasedTarget()
	{
	}
}
