using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace Verse;

public class Hediff_DeathRefusal : HediffWithComps
{
	protected int usesLeft;

	private TickTimer resurrectTimer = new TickTimer();

	private TickTimer warmupTimer = new TickTimer();

	private Sustainer resurrectSustainer;

	private bool resurrecting;

	private bool aiEnabled = true;

	private Effecter effecter;

	private Effecter resurrectAvailableEffecter;

	private Effecter resurrectUsedEffecter;

	private static readonly CachedTexture Icon = new CachedTexture("UI/Abilities/SelfResurrect");

	private const float ScarredChance = 0.1f;

	private static readonly float ResurrectDurationSeconds = 3f;

	private static readonly FloatRange AIWarmupSeconds = new FloatRange(1f, 2f);

	public bool PlayerControlled
	{
		get
		{
			if (pawn.IsColonist)
			{
				if (pawn.HostFaction != null)
				{
					return pawn.IsSlave;
				}
				return true;
			}
			return false;
		}
	}

	public bool InProgress => !resurrectTimer.Finished;

	public int UsesLeft => usesLeft;

	public bool AIEnabled
	{
		get
		{
			return aiEnabled;
		}
		set
		{
			aiEnabled = value;
		}
	}

	public virtual int MaxUses => 2;

	public override string LabelInBrackets => UsesLeft + " " + ((UsesLeft > 1) ? "DeathRefusalUsePlural".Translate() : "DeathRefusalUseSingular".Translate()).ToString();

	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModLister.CheckAnomaly("Death refusal"))
		{
			pawn.health.RemoveHediff(this);
			return;
		}
		base.PostAdd(dinfo);
		usesLeft = 1;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (!pawn.Dead || !PlayerControlled)
		{
			yield break;
		}
		Command_ActionWithLimitedUseCount cmdSelfResurrect = new Command_ActionWithLimitedUseCount();
		cmdSelfResurrect.defaultLabel = "CommandSelfResurrect".Translate();
		cmdSelfResurrect.defaultDesc = "CommandSelfResurrectDesc".Translate();
		cmdSelfResurrect.usesLeftGetter = () => usesLeft;
		cmdSelfResurrect.maxUsesGetter = () => MaxUses;
		cmdSelfResurrect.UpdateUsesLeft();
		cmdSelfResurrect.icon = Icon.Texture;
		cmdSelfResurrect.action = delegate
		{
			if (resurrectTimer.Finished)
			{
				Use();
				cmdSelfResurrect.UpdateUsesLeft();
			}
		};
		yield return cmdSelfResurrect;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref usesLeft, "usesLeft", 0);
		Scribe_Values.Look(ref resurrecting, "resurrecting", defaultValue: false);
		Scribe_Values.Look(ref aiEnabled, "aiEnabled", defaultValue: false);
		Scribe_Deep.Look(ref resurrectTimer, "resurrectTimer");
		Scribe_Deep.Look(ref warmupTimer, "warmupTimer");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			resurrectTimer.OnFinish = Resurrect;
			warmupTimer.OnFinish = Use;
			if (!resurrecting && pawn.Dead)
			{
				TryTriggerAIWarmupResurrection();
			}
		}
	}

	public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
	{
		base.Notify_PawnDied(dinfo, culprit);
		if (PlayerControlled && PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			Messages.Message("SelfResurrectText".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
		}
		TryTriggerAIWarmupResurrection();
		TryTriggerReadyEffect();
	}

	private void TryTriggerAIWarmupResurrection()
	{
		if (!PlayerControlled && !resurrecting && AIEnabled && usesLeft > 0)
		{
			resurrecting = true;
			warmupTimer.Start(GenTicks.TicksGame, AIWarmupSeconds.RandomInRange.SecondsToTicks(), Use);
		}
	}

	private void TryTriggerReadyEffect()
	{
		if (!resurrecting && usesLeft > 0 && resurrectAvailableEffecter == null && pawn.ParentHolder is Corpse corpse)
		{
			resurrectAvailableEffecter = EffecterDefOf.DeathRefusalAvailable.Spawn(corpse, corpse.MapHeld, Vector3.zero);
			pawn.MapHeld.effecterMaintainer.AddEffecterToMaintain(resurrectAvailableEffecter, corpse, 250);
		}
	}

	public override void TickInterval(int delta)
	{
		if (!resurrectTimer.Finished && pawn.ParentHolder is Corpse corpse)
		{
			if (effecter == null)
			{
				effecter = EffecterDefOf.CellPollution.Spawn(corpse, pawn.MapHeld, Vector3.zero);
				pawn.MapHeld.effecterMaintainer.AddEffecterToMaintain(effecter, corpse, 250);
			}
			if (resurrectSustainer == null)
			{
				SoundInfo info = SoundInfo.InMap(corpse, MaintenanceType.PerTickRare);
				resurrectSustainer = SoundDefOf.Pawn_SelfResurrection.TrySpawnSustainer(info);
			}
		}
	}

	public void TickRare()
	{
		if (!warmupTimer.Finished)
		{
			warmupTimer.TickIntervalDelta();
		}
		TryTriggerReadyEffect();
		if (!resurrectTimer.Finished)
		{
			resurrectTimer.TickIntervalDelta();
			if (resurrectSustainer != null && !resurrectSustainer.Ended)
			{
				resurrectSustainer?.Maintain();
			}
		}
		if (effecter != null)
		{
			effecter.ticksLeft = ((!resurrectTimer.Finished) ? (effecter.ticksLeft + 250) : 0);
		}
		if (resurrectAvailableEffecter != null)
		{
			resurrectAvailableEffecter.ticksLeft += 250;
		}
	}

	private void Resurrect()
	{
		resurrectSustainer?.End();
		resurrecting = false;
		pawn.Drawer.renderer.SetAnimation(null);
		ResurrectionUtility.TryResurrect(pawn, new ResurrectionParams
		{
			gettingScarsChance = 0.1f,
			canKidnap = false,
			canTimeoutOrFlee = false,
			useAvoidGridSmart = true,
			canSteal = false,
			invisibleStun = true
		});
		if (usesLeft == 0)
		{
			Severity = 0f;
		}
		if (pawn.Faction != Faction.OfPlayer && !pawn.Downed)
		{
			Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Weapon), PathEndMode.OnCell, TraverseParms.For(pawn), 5f);
			if (thing != null)
			{
				Job job = JobGiver_PickupDroppedWeapon.PickupWeaponJob(pawn, thing, ignoreForbidden: true);
				if (job != null)
				{
					pawn.jobs.StartJob(job, JobCondition.InterruptForced);
				}
			}
		}
		pawn.health.AddHediff(HediffDefOf.DeathRefusalSickness);
	}

	public void SetUseAmountDirect(int amount, bool ignoreLimit = false)
	{
		if (ignoreLimit)
		{
			usesLeft = amount;
		}
		else
		{
			usesLeft = Mathf.Clamp(amount, 0, MaxUses);
		}
	}

	private void Use()
	{
		Messages.Message("MessageUsingSelfResurrection".Translate(pawn), pawn, MessageTypeDefOf.NeutralEvent);
		resurrecting = true;
		usesLeft = Mathf.Max(usesLeft - 1, 0);
		resurrectTimer.Start(GenTicks.TicksGame, ResurrectDurationSeconds.SecondsToTicks(), Resurrect);
		resurrectAvailableEffecter?.ForceEnd();
		resurrectAvailableEffecter = null;
		if (pawn.ParentHolder is Corpse corpse)
		{
			resurrectUsedEffecter = EffecterDefOf.DeathRefusalUse.Spawn(corpse, corpse.MapHeld, Vector3.zero);
			pawn.MapHeld.effecterMaintainer.AddEffecterToMaintain(resurrectUsedEffecter, corpse, 1000);
			pawn.Drawer.renderer.SetAnimation(AnimationDefOf.DeathRefusalTwitches);
		}
	}

	public override void Notify_PawnCorpseDestroyed()
	{
		resurrectSustainer?.End();
	}
}
