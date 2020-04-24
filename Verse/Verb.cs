using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace Verse
{
	public abstract class Verb : ITargetingSource, IExposable, ILoadReferenceable
	{
		public VerbProperties verbProps;

		public VerbTracker verbTracker;

		public ManeuverDef maneuver;

		public Tool tool;

		public Thing caster;

		public string loadID;

		public VerbState state;

		protected LocalTargetInfo currentTarget;

		protected LocalTargetInfo currentDestination;

		protected int burstShotsLeft;

		protected int ticksToNextBurstShot;

		protected bool surpriseAttack;

		protected bool canHitNonTargetPawnsNow = true;

		public Action castCompleteCallback;

		private static List<IntVec3> tempLeanShootSources = new List<IntVec3>();

		private static List<IntVec3> tempDestList = new List<IntVec3>();

		public IVerbOwner DirectOwner => verbTracker.directOwner;

		public ImplementOwnerTypeDef ImplementOwnerType => verbTracker.directOwner.ImplementOwnerTypeDef;

		public CompEquippable EquipmentCompSource => DirectOwner as CompEquippable;

		public ThingWithComps EquipmentSource
		{
			get
			{
				if (EquipmentCompSource == null)
				{
					return null;
				}
				return EquipmentCompSource.parent;
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

		public virtual TargetingParameters targetParams => verbProps.targetParams;

		public virtual ITargetingSource DestinationSelector => null;

		protected virtual int ShotsPerBurst => 1;

		public virtual Texture2D UIIcon
		{
			get
			{
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

		public bool WarmingUp
		{
			get
			{
				if (CasterPawn == null || !CasterPawn.Spawned)
				{
					return false;
				}
				Stance_Warmup stance_Warmup = CasterPawn.stances.curStance as Stance_Warmup;
				if (stance_Warmup != null)
				{
					return stance_Warmup.verb == this;
				}
				return false;
			}
		}

		public virtual string ReportLabel => verbProps.label;

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
			Scribe_Values.Look(ref surpriseAttack, "surpriseAttack", defaultValue: false);
			Scribe_Values.Look(ref canHitNonTargetPawnsNow, "canHitNonTargetPawnsNow", defaultValue: false);
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

		public bool TryStartCastOn(LocalTargetInfo castTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true)
		{
			return TryStartCastOn(castTarg, LocalTargetInfo.Invalid, surpriseAttack, canHitNonTargetPawns);
		}

		public bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true)
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
			currentTarget = castTarg;
			currentDestination = destTarg;
			if (CasterIsPawn && verbProps.warmupTime > 0f)
			{
				if (!TryFindShootLineFromTo(caster.Position, castTarg, out ShootLine resultingLine))
				{
					return false;
				}
				CasterPawn.Drawer.Notify_WarmingCastAlongLine(resultingLine, caster.Position);
				float statValue = CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor);
				int ticks = (verbProps.warmupTime * statValue).SecondsToTicks();
				CasterPawn.stances.SetStance(new Stance_Warmup(ticks, castTarg, this));
			}
			else
			{
				WarmupComplete();
			}
			return true;
		}

		public virtual void WarmupComplete()
		{
			burstShotsLeft = ShotsPerBurst;
			state = VerbState.Bursting;
			TryCastNextBurstShot();
			if (CasterIsPawn && currentTarget.HasThing)
			{
				Pawn pawn = currentTarget.Thing as Pawn;
				if (pawn != null && pawn.IsColonistPlayerControlled)
				{
					CasterPawn.records.AccumulateStoryEvent(StoryEventDefOf.AttackedPlayer);
				}
			}
		}

		public void VerbTick()
		{
			if (state != VerbState.Bursting)
			{
				return;
			}
			if (!caster.Spawned)
			{
				Reset();
				return;
			}
			ticksToNextBurstShot--;
			if (ticksToNextBurstShot <= 0)
			{
				TryCastNextBurstShot();
			}
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
			return true;
		}

		protected void TryCastNextBurstShot()
		{
			LocalTargetInfo localTargetInfo = currentTarget;
			if (Available() && TryCastShot())
			{
				if (verbProps.muzzleFlashScale > 0.01f)
				{
					MoteMaker.MakeStaticMote(caster.Position, caster.Map, ThingDefOf.Mote_ShotFlash, verbProps.muzzleFlashScale);
				}
				if (verbProps.soundCast != null)
				{
					verbProps.soundCast.PlayOneShot(new TargetInfo(caster.Position, caster.Map));
				}
				if (verbProps.soundCastTail != null)
				{
					verbProps.soundCastTail.PlayOneShotOnCamera(caster.Map);
				}
				if (CasterIsPawn)
				{
					if (CasterPawn.thinker != null)
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
				ticksToNextBurstShot = verbProps.ticksBetweenBurstShots;
				if (CasterIsPawn)
				{
					CasterPawn.stances.SetStance(new Stance_Cooldown(verbProps.ticksBetweenBurstShots + 1, currentTarget, this));
				}
				return;
			}
			state = VerbState.Idle;
			if (CasterIsPawn)
			{
				CasterPawn.stances.SetStance(new Stance_Cooldown(verbProps.AdjustedCooldownTicks(this, CasterPawn), currentTarget, this));
			}
			if (castCompleteCallback != null)
			{
				castCompleteCallback();
			}
		}

		public virtual void OrderForceTarget(LocalTargetInfo target)
		{
			if (verbProps.IsMeleeAttack)
			{
				Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
				job.playerForced = true;
				Pawn pawn = target.Thing as Pawn;
				if (pawn != null)
				{
					job.killIncappedTarget = pawn.Downed;
				}
				CasterPawn.jobs.TryTakeOrderedJob(job);
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
			CasterPawn.jobs.TryTakeOrderedJob(job2);
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
				Stance_Warmup stance_Warmup = casterPawn.stances.curStance as Stance_Warmup;
				if (stance_Warmup != null && stance_Warmup.verb == this)
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
			bool flag = (thing as Pawn)?.Downed ?? false;
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

		public virtual bool ValidateTarget(LocalTargetInfo target)
		{
			return true;
		}

		public virtual void DrawHighlight(LocalTargetInfo target)
		{
			verbProps.DrawRadiusRing(caster.Position);
			if (!target.IsValid)
			{
				return;
			}
			GenDraw.DrawTargetHighlight(target);
			bool needLOSToCenter;
			float num = HighlightFieldRadiusAroundTarget(out needLOSToCenter);
			if (num > 0.2f && TryFindShootLineFromTo(caster.Position, target, out ShootLine resultingLine))
			{
				if (needLOSToCenter)
				{
					GenExplosion.RenderPredictedAreaOfEffect(resultingLine.Dest, num);
				}
				else
				{
					GenDraw.DrawFieldEdges((from x in GenRadial.RadialCellsAround(resultingLine.Dest, num, useCenter: true)
						where x.InBounds(Find.CurrentMap)
						select x).ToList());
				}
			}
		}

		public virtual void OnGUI(LocalTargetInfo target)
		{
			Texture2D icon = (!target.IsValid) ? TexCommand.CannotShoot : ((!(UIIcon != BaseContent.BadTex)) ? TexCommand.Attack : UIIcon);
			GenUI.DrawMouseAttachment(icon);
		}

		public virtual bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
		{
			if (targ.Thing != null && targ.Thing == caster)
			{
				return targetParams.canTargetSelf;
			}
			if (ApparelPreventsShooting(root, targ))
			{
				return false;
			}
			ShootLine resultingLine;
			return TryFindShootLineFromTo(root, targ, out resultingLine);
		}

		public bool ApparelPreventsShooting(IntVec3 root, LocalTargetInfo targ)
		{
			if (CasterIsPawn && CasterPawn.apparel != null)
			{
				List<Apparel> wornApparel = CasterPawn.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					if (!wornApparel[i].AllowVerbCast(root, caster.Map, targ, this))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool TryFindShootLineFromTo(IntVec3 root, LocalTargetInfo targ, out ShootLine resultingLine)
		{
			if (targ.HasThing && targ.Thing.Map != caster.Map)
			{
				resultingLine = default(ShootLine);
				return false;
			}
			if (verbProps.IsMeleeAttack || verbProps.range <= 1.42f)
			{
				resultingLine = new ShootLine(root, targ.Cell);
				return ReachabilityImmediate.CanReachImmediate(root, targ, caster.Map, PathEndMode.Touch, null);
			}
			CellRect cellRect = targ.HasThing ? targ.Thing.OccupiedRect() : CellRect.SingleCell(targ.Cell);
			float num = verbProps.EffectiveMinRange(targ, caster);
			float num2 = cellRect.ClosestDistSquaredTo(root);
			if (num2 > verbProps.range * verbProps.range || num2 < num * num)
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
				ShootLeanUtility.LeanShootingSourcesFromTo(root, cellRect.ClosestCellTo(root), caster.Map, tempLeanShootSources);
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

		private bool CanHitFromCellIgnoringRange(IntVec3 sourceCell, LocalTargetInfo targ, out IntVec3 goodDest)
		{
			if (targ.Thing != null)
			{
				if (targ.Thing.Map != caster.Map)
				{
					goodDest = IntVec3.Invalid;
					return false;
				}
				ShootLeanUtility.CalcShootableCellsOf(tempDestList, targ.Thing);
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
			string text = (verbProps == null) ? "null" : ((!verbProps.label.NullOrEmpty()) ? verbProps.label : ((HediffCompSource != null) ? HediffCompSource.Def.label : ((EquipmentSource != null) ? EquipmentSource.def.label : ((verbProps.AdjustedLinkedBodyPartsGroup(tool) == null) ? "unknown" : verbProps.AdjustedLinkedBodyPartsGroup(tool).defName))));
			if (tool != null)
			{
				text = text + "/" + loadID;
			}
			return GetType().ToString() + "(" + text + ")";
		}
	}
}
