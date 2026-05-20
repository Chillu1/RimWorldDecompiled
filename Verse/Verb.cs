using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace Verse;

public abstract class Verb : ITargetingSource, IExposable, ILoadReferenceable
{
	public VerbProperties verbProps;

	public VerbTracker verbTracker;

	public ManeuverDef maneuver;

	public Tool tool;

	public Thing caster;

	public MechanitorControlGroup controlGroup;

	public string loadID;

	public VerbState state;

	protected LocalTargetInfo currentTarget;

	protected LocalTargetInfo currentDestination;

	protected int burstShotsLeft;

	protected int ticksToNextBurstShot;

	protected int lastShotTick = -999999;

	protected bool surpriseAttack;

	protected bool canHitNonTargetPawnsNow = true;

	public bool preventFriendlyFire;

	protected bool nonInterruptingSelfCast;

	public Action castCompleteCallback;

	private Texture2D commandIconCached;

	private readonly List<Tuple<Effecter, TargetInfo, TargetInfo>> maintainedEffecters = new List<Tuple<Effecter, TargetInfo, TargetInfo>>();

	private int? cachedTicksBetweenBurstShots;

	private int? cachedBurstShotCount;

	private static readonly List<IntVec3> tempLeanShootSources = new List<IntVec3>();

	private static readonly List<IntVec3> tempDestList = new List<IntVec3>();

	public IVerbOwner DirectOwner => verbTracker.directOwner;

	public ImplementOwnerTypeDef ImplementOwnerType => verbTracker.directOwner.ImplementOwnerTypeDef;

	public CompEquippable EquipmentCompSource => DirectOwner as CompEquippable;

	public CompApparelReloadable ReloadableCompSource => DirectOwner as CompApparelReloadable;

	public CompApparelVerbOwner_Charged VerbOwner_ChargedCompSource => DirectOwner as CompApparelVerbOwner_Charged;

	public ThingWithComps EquipmentSource
	{
		get
		{
			if (EquipmentCompSource != null)
			{
				return EquipmentCompSource.parent;
			}
			if (ReloadableCompSource != null)
			{
				return ReloadableCompSource.parent;
			}
			if (VerbOwner_ChargedCompSource != null)
			{
				return VerbOwner_ChargedCompSource.parent;
			}
			return null;
		}
	}

	public HediffComp_VerbGiver HediffCompSource => DirectOwner as HediffComp_VerbGiver;

	public Hediff HediffSource
	{
		get
		{
			if (HediffCompSource == null)
			{
				return null;
			}
			return HediffCompSource.parent;
		}
	}

	public Pawn_MeleeVerbs_TerrainSource TerrainSource => DirectOwner as Pawn_MeleeVerbs_TerrainSource;

	public TerrainDef TerrainDefSource
	{
		get
		{
			if (TerrainSource == null)
			{
				return null;
			}
			return TerrainSource.def;
		}
	}

	public virtual Thing Caster => caster;

	public virtual Pawn CasterPawn => caster as Pawn;

	public virtual Verb GetVerb => this;

	public virtual bool CasterIsPawn => caster is Pawn;

	public virtual bool Targetable => verbProps.targetable;

	public virtual bool MultiSelect => false;

	public virtual bool HidePawnTooltips => false;

	public LocalTargetInfo CurrentTarget => currentTarget;

	public LocalTargetInfo CurrentDestination => currentDestination;

	public int LastShotTick => lastShotTick;

	public virtual TargetingParameters targetParams => verbProps.targetParams;

	public virtual ITargetingSource DestinationSelector => null;

	protected virtual int ShotsPerBurst => 1;

	public virtual Texture2D UIIcon
	{
		get
		{
			if (verbProps.commandIcon != null)
			{
				if (commandIconCached == null)
				{
					commandIconCached = ContentFinder<Texture2D>.Get(verbProps.commandIcon);
				}
				return commandIconCached;
			}
			if (EquipmentSource != null)
			{
				return EquipmentSource.def.uiIcon;
			}
			return BaseContent.BadTex;
		}
	}

	public bool Bursting => burstShotsLeft > 0;

	public virtual bool IsMeleeAttack => verbProps.IsMeleeAttack;

	public bool BuggedAfterLoading => verbProps == null;

	public bool WarmingUp => WarmupStance != null;

	public Stance_Warmup WarmupStance
	{
		get
		{
			if (CasterPawn == null || !CasterPawn.Spawned)
			{
				return null;
			}
			if (!(CasterPawn.stances.curStance is Stance_Warmup stance_Warmup) || stance_Warmup.verb != this)
			{
				return null;
			}
			return stance_Warmup;
		}
	}

	public virtual float WarmupTime => verbProps.warmupTime;

	public int WarmupTicksLeft
	{
		get
		{
			if (WarmupStance == null)
			{
				return 0;
			}
			return WarmupStance.ticksLeft;
		}
	}

	public float WarmupProgress => 1f - WarmupTicksLeft.TicksToSeconds() / WarmupTime;

	public virtual string ReportLabel => verbProps.label;

	public virtual float EffectiveRange => verbProps.AdjustedRange(this, Caster);

	public virtual float? AimAngleOverride => null;

	public bool NonInterruptingSelfCast
	{
		get
		{
			if (!verbProps.nonInterruptingSelfCast)
			{
				return nonInterruptingSelfCast;
			}
			return true;
		}
	}

	public int TicksBetweenBurstShots
	{
		get
		{
			if (!cachedTicksBetweenBurstShots.HasValue)
			{
				float num = verbProps.ticksBetweenBurstShots;
				if (EquipmentSource != null && EquipmentSource.TryGetComp<CompUniqueWeapon>(out var comp))
				{
					foreach (WeaponTraitDef item in comp.TraitsListForReading)
					{
						num /= item.burstShotSpeedMultiplier;
					}
				}
				cachedTicksBetweenBurstShots = Mathf.RoundToInt(num);
			}
			return cachedTicksBetweenBurstShots.Value;
		}
	}

	public int BurstShotCount
	{
		get
		{
			if (!cachedBurstShotCount.HasValue)
			{
				float num = verbProps.burstShotCount;
				if (EquipmentSource != null && EquipmentSource.TryGetComp<CompUniqueWeapon>(out var comp))
				{
					foreach (WeaponTraitDef item in comp.TraitsListForReading)
					{
						num *= item.burstShotCountMultiplier;
					}
				}
				cachedBurstShotCount = Mathf.CeilToInt(num);
			}
			return cachedBurstShotCount.Value;
		}
	}

	public bool IsStillUsableBy(Pawn pawn)
	{
		if (!Available())
		{
			return false;
		}
		if (!DirectOwner.VerbsStillUsableBy(pawn))
		{
			return false;
		}
		if (verbProps.GetDamageFactorFor(this, pawn) == 0f)
		{
			return false;
		}
		if (pawn.IsSubhuman && verbProps.category == VerbCategory.Ignite)
		{
			return false;
		}
		return true;
	}

	public virtual bool IsUsableOn(Thing target)
	{
		return true;
	}

	public virtual void ExposeData()
	{
		Scribe_Values.Look(ref loadID, "loadID");
		Scribe_Values.Look(ref state, "state", VerbState.Idle);
		Scribe_TargetInfo.Look(ref currentTarget, "currentTarget");
		Scribe_TargetInfo.Look(ref currentDestination, "currentDestination");
		Scribe_Values.Look(ref burstShotsLeft, "burstShotsLeft", 0);
		Scribe_Values.Look(ref ticksToNextBurstShot, "ticksToNextBurstShot", 0);
		Scribe_Values.Look(ref lastShotTick, "lastShotTick", 0);
		Scribe_Values.Look(ref surpriseAttack, "surpriseAttack", defaultValue: false);
		Scribe_Values.Look(ref canHitNonTargetPawnsNow, "canHitNonTargetPawnsNow", defaultValue: false);
		Scribe_Values.Look(ref preventFriendlyFire, "preventFriendlyFire", defaultValue: false);
		Scribe_Values.Look(ref nonInterruptingSelfCast, "nonInterruptingSelfCast", defaultValue: false);
	}

	public string GetUniqueLoadID()
	{
		return "Verb_" + loadID;
	}

	public static string CalculateUniqueLoadID(IVerbOwner owner, Tool tool, ManeuverDef maneuver)
	{
		return string.Format("{0}_{1}_{2}", owner.UniqueVerbOwnerID(), (tool != null) ? tool.id : "NT", (maneuver != null) ? maneuver.defName : "NM");
	}

	public static string CalculateUniqueLoadID(IVerbOwner owner, int index)
	{
		return $"{owner.UniqueVerbOwnerID()}_{index}";
	}

	public bool TryStartCastOn(LocalTargetInfo castTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
	{
		return TryStartCastOn(castTarg, LocalTargetInfo.Invalid, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire, nonInterruptingSelfCast);
	}

	public virtual bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
	{
		if (caster == null)
		{
			Log.Error("Verb " + GetUniqueLoadID() + " needs caster to work (possibly lost during saving/loading).");
			return false;
		}
		if (!caster.Spawned)
		{
			return false;
		}
		if (state == VerbState.Bursting || !CanHitTarget(castTarg))
		{
			return false;
		}
		if (CausesTimeSlowdown(castTarg))
		{
			Find.TickManager.slower.SignalForceNormalSpeed();
		}
		this.surpriseAttack = surpriseAttack;
		canHitNonTargetPawnsNow = canHitNonTargetPawns;
		this.preventFriendlyFire = preventFriendlyFire;
		this.nonInterruptingSelfCast = nonInterruptingSelfCast;
		currentTarget = castTarg;
		currentDestination = destTarg;
		if (CasterIsPawn && WarmupTime > 0f)
		{
			if (!TryFindShootLineFromTo(caster.Position, castTarg, out var resultingLine))
			{
				return false;
			}
			CasterPawn.Drawer.Notify_WarmingCastAlongLine(resultingLine, caster.Position);
			float statValue = CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor);
			int ticks = (WarmupTime * statValue).SecondsToTicks();
			CasterPawn.stances.SetStance(new Stance_Warmup(ticks, castTarg, this));
			if (verbProps.stunTargetOnCastStart && castTarg.Pawn != null)
			{
				castTarg.Pawn.stances.stunner.StunFor(ticks, null, addBattleLog: false);
			}
		}
		else
		{
			if (verbTracker.directOwner is Ability ability)
			{
				ability.lastCastTick = Find.TickManager.TicksGame;
			}
			WarmupComplete();
		}
		return true;
	}

	public virtual void WarmupComplete()
	{
		burstShotsLeft = ShotsPerBurst;
		state = VerbState.Bursting;
		TryCastNextBurstShot();
	}

	public void VerbTick()
	{
		if (state == VerbState.Bursting)
		{
			if (!caster.Spawned || (caster is Pawn pawn && pawn.stances.stunner.Stunned))
			{
				Reset();
			}
			else
			{
				ticksToNextBurstShot--;
				if (ticksToNextBurstShot <= 0)
				{
					TryCastNextBurstShot();
				}
				BurstingTick();
			}
		}
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

	public virtual void BurstingTick()
	{
	}

	public void AddEffecterToMaintain(Effecter eff, IntVec3 pos, int ticks, Map map = null)
	{
		eff.ticksLeft = ticks;
		TargetInfo targetInfo = new TargetInfo(pos, map ?? caster.Map);
		maintainedEffecters.Add(new Tuple<Effecter, TargetInfo, TargetInfo>(eff, targetInfo, targetInfo));
	}

	public void AddEffecterToMaintain(Effecter eff, IntVec3 posA, IntVec3 posB, int ticks, Map map = null)
	{
		eff.ticksLeft = ticks;
		TargetInfo item = new TargetInfo(posA, map ?? caster.Map);
		TargetInfo item2 = new TargetInfo(posB, map ?? caster.Map);
		maintainedEffecters.Add(new Tuple<Effecter, TargetInfo, TargetInfo>(eff, item, item2));
	}

	public virtual bool Available()
	{
		if (verbProps.consumeFuelPerShot > 0f)
		{
			CompRefuelable compRefuelable = caster.TryGetComp<CompRefuelable>();
			if (compRefuelable != null && compRefuelable.Fuel < verbProps.consumeFuelPerShot)
			{
				return false;
			}
		}
		CompApparelVerbOwner compApparelVerbOwner = EquipmentSource?.GetComp<CompApparelVerbOwner>();
		if (compApparelVerbOwner != null && !compApparelVerbOwner.CanBeUsed(out var reason))
		{
			return false;
		}
		if (CasterIsPawn && EquipmentSource != null && EquipmentUtility.RolePreventsFromUsing(CasterPawn, EquipmentSource, out reason))
		{
			return false;
		}
		return true;
	}

	protected void TryCastNextBurstShot()
	{
		LocalTargetInfo localTargetInfo = currentTarget;
		if (Available() && TryCastShot())
		{
			if (verbProps.muzzleFlashScale > 0.01f)
			{
				FleckMaker.Static(caster.Position, caster.Map, FleckDefOf.ShotFlash, verbProps.muzzleFlashScale);
			}
			if (verbProps.soundCast != null)
			{
				verbProps.soundCast.PlayOneShot(new TargetInfo(caster.Position, caster.MapHeld));
			}
			if (verbProps.soundCastTail != null)
			{
				verbProps.soundCastTail.PlayOneShotOnCamera(caster.Map);
			}
			if (CasterIsPawn)
			{
				CasterPawn.Notify_UsedVerb(CasterPawn, this);
				if (CasterPawn.thinker != null && localTargetInfo == CasterPawn.mindState.enemyTarget)
				{
					CasterPawn.mindState.Notify_EngagedTarget();
				}
				if (CasterPawn.mindState != null)
				{
					CasterPawn.mindState.Notify_AttackedTarget(localTargetInfo);
				}
				if (CasterPawn.MentalState != null)
				{
					CasterPawn.MentalState.Notify_AttackedTarget(localTargetInfo);
				}
				if (TerrainDefSource != null)
				{
					CasterPawn.meleeVerbs.Notify_UsedTerrainBasedVerb();
				}
				if (CasterPawn.health != null)
				{
					CasterPawn.health.Notify_UsedVerb(this, localTargetInfo);
				}
				if (EquipmentSource != null)
				{
					EquipmentSource.Notify_UsedWeapon(CasterPawn);
				}
				if (!CasterPawn.Spawned)
				{
					Reset();
					return;
				}
			}
			if (verbProps.consumeFuelPerShot > 0f)
			{
				caster.TryGetComp<CompRefuelable>()?.ConsumeFuel(verbProps.consumeFuelPerShot);
			}
			burstShotsLeft--;
		}
		else
		{
			burstShotsLeft = 0;
		}
		if (burstShotsLeft > 0)
		{
			ticksToNextBurstShot = TicksBetweenBurstShots;
			if (CasterIsPawn && !NonInterruptingSelfCast)
			{
				CasterPawn.stances.SetStance(new Stance_Cooldown(TicksBetweenBurstShots + 1, currentTarget, this));
			}
			return;
		}
		state = VerbState.Idle;
		if (CasterIsPawn && !NonInterruptingSelfCast)
		{
			CasterPawn.stances.SetStance(new Stance_Cooldown(verbProps.AdjustedCooldownTicks(this, CasterPawn), currentTarget, this));
		}
		if (castCompleteCallback != null)
		{
			castCompleteCallback();
		}
		if (verbProps.consumeFuelPerBurst > 0f)
		{
			caster.TryGetComp<CompRefuelable>()?.ConsumeFuel(verbProps.consumeFuelPerBurst);
		}
	}

	public virtual void OrderForceTarget(LocalTargetInfo target)
	{
		if (verbProps.IsMeleeAttack)
		{
			Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
			job.playerForced = true;
			if (target.Thing is Pawn pawn)
			{
				job.killIncappedTarget = pawn.Downed;
			}
			CasterPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			return;
		}
		float num = verbProps.EffectiveMinRange(target, CasterPawn);
		if ((float)CasterPawn.Position.DistanceToSquared(target.Cell) < num * num && CasterPawn.Position.AdjacentTo8WayOrInside(target.Cell))
		{
			Messages.Message("MessageCantShootInMelee".Translate(), CasterPawn, MessageTypeDefOf.RejectInput, historical: false);
			return;
		}
		Job job2 = JobMaker.MakeJob(verbProps.ai_IsWeapon ? JobDefOf.AttackStatic : JobDefOf.UseVerbOnThing);
		job2.verbToUse = this;
		job2.targetA = target;
		job2.endIfCantShootInMelee = true;
		CasterPawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);
	}

	protected abstract bool TryCastShot();

	public void Notify_PickedUp()
	{
		Reset();
	}

	public virtual void Reset()
	{
		state = VerbState.Idle;
		currentTarget = null;
		currentDestination = null;
		burstShotsLeft = 0;
		ticksToNextBurstShot = 0;
		castCompleteCallback = null;
		surpriseAttack = false;
		preventFriendlyFire = false;
	}

	public virtual void Notify_EquipmentLost()
	{
		if (!CasterIsPawn)
		{
			return;
		}
		Pawn casterPawn = CasterPawn;
		if (casterPawn.Spawned)
		{
			if (casterPawn.stances.curStance is Stance_Warmup stance_Warmup && stance_Warmup.verb == this)
			{
				casterPawn.stances.CancelBusyStanceSoft();
			}
			if (casterPawn.CurJob != null && casterPawn.CurJob.def == JobDefOf.AttackStatic)
			{
				casterPawn.jobs.EndCurrentJob(JobCondition.Incompletable);
			}
		}
	}

	public virtual float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
	{
		needLOSToCenter = false;
		return 0f;
	}

	private bool CausesTimeSlowdown(LocalTargetInfo castTarg)
	{
		if (!verbProps.CausesTimeSlowdown)
		{
			return false;
		}
		if (!castTarg.HasThing)
		{
			return false;
		}
		Thing thing = castTarg.Thing;
		if (thing.def.category != ThingCategory.Pawn && (thing.def.building == null || !thing.def.building.IsTurret))
		{
			return false;
		}
		Pawn pawn = thing as Pawn;
		bool flag = pawn?.Downed ?? false;
		if ((CasterPawn != null && CasterPawn.Faction == Faction.OfPlayer && CasterPawn.IsShambler) || (pawn != null && pawn.Faction == Faction.OfPlayer && pawn.IsShambler))
		{
			return false;
		}
		if (thing.Faction != Faction.OfPlayer || !caster.HostileTo(Faction.OfPlayer))
		{
			if (caster.Faction == Faction.OfPlayer && thing.HostileTo(Faction.OfPlayer))
			{
				return !flag;
			}
			return false;
		}
		return true;
	}

	public virtual bool CanHitTarget(LocalTargetInfo targ)
	{
		if (caster == null || !caster.Spawned)
		{
			return false;
		}
		if (targ == caster)
		{
			return true;
		}
		return CanHitTargetFrom(caster.Position, targ);
	}

	public virtual bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (CasterIsPawn && target.Thing is Pawn p && (p.InSameExtraFaction(caster as Pawn, ExtraFactionType.HomeFaction) || p.InSameExtraFaction(caster as Pawn, ExtraFactionType.MiniFaction)))
		{
			return false;
		}
		if (CasterIsPawn && target.Thing is Pawn victim && HistoryEventUtility.IsKillingInnocentAnimal(CasterPawn, victim) && !new HistoryEvent(HistoryEventDefOf.KilledInnocentAnimal, CasterPawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo())
		{
			return false;
		}
		if (CasterIsPawn && target.Thing is Pawn pawn && CasterPawn.Ideo != null && CasterPawn.Ideo.IsVeneratedAnimal(pawn) && !new HistoryEvent(HistoryEventDefOf.HuntedVeneratedAnimal, CasterPawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo())
		{
			return false;
		}
		return true;
	}

	public virtual void DrawHighlight(LocalTargetInfo target)
	{
		verbProps.DrawRadiusRing(caster.Position, this);
		if (target.IsValid)
		{
			GenDraw.DrawTargetHighlight(target);
			DrawHighlightFieldRadiusAroundTarget(target);
		}
	}

	protected void DrawHighlightFieldRadiusAroundTarget(LocalTargetInfo target)
	{
		bool needLOSToCenter;
		float num = HighlightFieldRadiusAroundTarget(out needLOSToCenter);
		if (!(num > 0.2f) || !TryFindShootLineFromTo(caster.Position, target, out var resultingLine))
		{
			return;
		}
		if (needLOSToCenter)
		{
			GenExplosion.RenderPredictedAreaOfEffect(resultingLine.Dest, num, verbProps.explosionRadiusRingColor);
			return;
		}
		GenDraw.DrawFieldEdges((from x in GenRadial.RadialCellsAround(resultingLine.Dest, num, useCenter: true)
			where x.InBounds(Find.CurrentMap)
			select x).ToList(), verbProps.explosionRadiusRingColor);
	}

	public virtual void OnGUI(LocalTargetInfo target)
	{
		Texture2D icon = ((!target.IsValid) ? TexCommand.CannotShoot : ((!(UIIcon != BaseContent.BadTex)) ? TexCommand.Attack : UIIcon));
		GenUI.DrawMouseAttachment(icon);
	}

	public virtual bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
	{
		if (targ.Thing != null && targ.Thing == caster)
		{
			return targetParams.canTargetSelf;
		}
		if (targ.Pawn != null && targ.Pawn.IsPsychologicallyInvisible() && caster.HostileTo(targ.Pawn))
		{
			return false;
		}
		if (ApparelPreventsShooting())
		{
			return false;
		}
		ShootLine resultingLine;
		return TryFindShootLineFromTo(root, targ, out resultingLine);
	}

	public bool ApparelPreventsShooting()
	{
		return FirstApparelPreventingShooting() != null;
	}

	public Apparel FirstApparelPreventingShooting()
	{
		if (CasterIsPawn && CasterPawn.apparel != null)
		{
			List<Apparel> wornApparel = CasterPawn.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				if (!wornApparel[i].AllowVerbCast(this))
				{
					return wornApparel[i];
				}
			}
		}
		return null;
	}

	public bool TryFindShootLineFromTo(IntVec3 root, LocalTargetInfo targ, out ShootLine resultingLine, bool ignoreRange = false)
	{
		if (targ.HasThing && targ.Thing.Map != caster.Map)
		{
			resultingLine = default(ShootLine);
			return false;
		}
		if (verbProps.IsMeleeAttack || EffectiveRange <= 1.42f)
		{
			resultingLine = new ShootLine(root, targ.Cell);
			return ReachabilityImmediate.CanReachImmediate(root, targ, caster.Map, PathEndMode.Touch, null);
		}
		CellRect occupiedRect = (targ.HasThing ? targ.Thing.OccupiedRect() : CellRect.SingleCell(targ.Cell));
		if (!ignoreRange && OutOfRange(root, targ, occupiedRect))
		{
			resultingLine = new ShootLine(root, targ.Cell);
			return false;
		}
		if (!verbProps.requireLineOfSight)
		{
			resultingLine = new ShootLine(root, targ.Cell);
			return true;
		}
		IntVec3 goodDest;
		if (CasterIsPawn)
		{
			if (CanHitFromCellIgnoringRange(root, targ, out goodDest))
			{
				resultingLine = new ShootLine(root, goodDest);
				return true;
			}
			ShootLeanUtility.LeanShootingSourcesFromTo(root, occupiedRect.ClosestCellTo(root), caster.Map, tempLeanShootSources);
			for (int i = 0; i < tempLeanShootSources.Count; i++)
			{
				IntVec3 intVec = tempLeanShootSources[i];
				if (CanHitFromCellIgnoringRange(intVec, targ, out goodDest))
				{
					resultingLine = new ShootLine(intVec, goodDest);
					return true;
				}
			}
		}
		else
		{
			foreach (IntVec3 item in caster.OccupiedRect())
			{
				if (CanHitFromCellIgnoringRange(item, targ, out goodDest))
				{
					resultingLine = new ShootLine(item, goodDest);
					return true;
				}
			}
		}
		resultingLine = new ShootLine(root, targ.Cell);
		return false;
	}

	public bool OutOfRange(IntVec3 root, LocalTargetInfo targ, CellRect occupiedRect)
	{
		float num = verbProps.EffectiveMinRange(targ, caster);
		float num2 = occupiedRect.ClosestDistSquaredTo(root);
		if (num2 > EffectiveRange * EffectiveRange || num2 < num * num)
		{
			return true;
		}
		return false;
	}

	private bool CanHitFromCellIgnoringRange(IntVec3 sourceCell, LocalTargetInfo targ, out IntVec3 goodDest)
	{
		if (targ.Thing != null)
		{
			if (targ.Thing.Map != caster.Map)
			{
				goodDest = IntVec3.Invalid;
				return false;
			}
			ShootLeanUtility.CalcShootableCellsOf(tempDestList, targ.Thing, sourceCell);
			for (int i = 0; i < tempDestList.Count; i++)
			{
				if (CanHitCellFromCellIgnoringRange(sourceCell, tempDestList[i], targ.Thing.def.Fillage == FillCategory.Full))
				{
					goodDest = tempDestList[i];
					return true;
				}
			}
		}
		else if (CanHitCellFromCellIgnoringRange(sourceCell, targ.Cell))
		{
			goodDest = targ.Cell;
			return true;
		}
		goodDest = IntVec3.Invalid;
		return false;
	}

	private bool CanHitCellFromCellIgnoringRange(IntVec3 sourceSq, IntVec3 targetLoc, bool includeCorners = false)
	{
		if (verbProps.mustCastOnOpenGround && (!targetLoc.Standable(caster.Map) || caster.Map.thingGrid.CellContains(targetLoc, ThingCategory.Pawn)))
		{
			return false;
		}
		if (verbProps.requireLineOfSight)
		{
			if (!includeCorners)
			{
				if (!GenSight.LineOfSight(sourceSq, targetLoc, caster.Map, skipFirstCell: true))
				{
					return false;
				}
			}
			else if (!GenSight.LineOfSightToEdges(sourceSq, targetLoc, caster.Map, skipFirstCell: true))
			{
				return false;
			}
		}
		return true;
	}

	public override string ToString()
	{
		string text = ((verbProps == null) ? "null" : ((!verbProps.label.NullOrEmpty()) ? verbProps.label : ((HediffCompSource != null) ? HediffCompSource.Def.label : ((EquipmentSource != null) ? EquipmentSource.def.label : ((verbProps.AdjustedLinkedBodyPartsGroup(tool) == null) ? "unknown" : verbProps.AdjustedLinkedBodyPartsGroup(tool).defName)))));
		if (tool != null)
		{
			text = text + "/" + loadID;
		}
		return $"{GetType()}({text})";
	}
}
