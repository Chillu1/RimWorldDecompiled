using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class CompTurretGun : ThingComp, IAttackTargetSearcher
{
	private const int StartShootIntervalTicks = 10;

	private static readonly CachedTexture ToggleTurretIcon = new CachedTexture("UI/Gizmos/ToggleTurret");

	public Thing gun;

	protected int burstCooldownTicksLeft;

	protected int burstWarmupTicksLeft;

	protected LocalTargetInfo currentTarget = LocalTargetInfo.Invalid;

	private bool fireAtWill = true;

	private LocalTargetInfo lastAttackedTarget = LocalTargetInfo.Invalid;

	private int lastAttackTargetTick;

	public float curRotation;

	public Thing Thing => parent;

	public CompProperties_TurretGun Props => (CompProperties_TurretGun)props;

	public Verb CurrentEffectiveVerb => AttackVerb;

	public LocalTargetInfo LastAttackedTarget => lastAttackedTarget;

	public int LastAttackTargetTick => lastAttackTargetTick;

	public CompEquippable GunCompEq => gun.TryGetComp<CompEquippable>();

	public Verb AttackVerb => GunCompEq.PrimaryVerb;

	private bool WarmingUp => burstWarmupTicksLeft > 0;

	private bool CanShoot
	{
		get
		{
			if (parent is Pawn pawn)
			{
				if (!pawn.Spawned || pawn.Downed || pawn.Dead || !pawn.Awake())
				{
					return false;
				}
				if (pawn.stances.stunner.Stunned)
				{
					return false;
				}
				if (TurretDestroyed)
				{
					return false;
				}
				if (pawn.IsColonyMechPlayerControlled && !fireAtWill)
				{
					return false;
				}
			}
			CompCanBeDormant compCanBeDormant = parent.TryGetComp<CompCanBeDormant>();
			if (compCanBeDormant != null && !compCanBeDormant.Awake)
			{
				return false;
			}
			return true;
		}
	}

	public bool TurretDestroyed
	{
		get
		{
			if (parent is Pawn pawn && AttackVerb.verbProps.linkedBodyPartsGroup != null && AttackVerb.verbProps.ensureLinkedBodyPartsGroupAlwaysUsable && PawnCapacityUtility.CalculateNaturalPartsAverageEfficiency(pawn.health.hediffSet, AttackVerb.verbProps.linkedBodyPartsGroup) <= 0f)
			{
				return true;
			}
			return false;
		}
	}

	public bool AutoAttack => Props.autoAttack;

	public override void PostPostMake()
	{
		base.PostPostMake();
		MakeGun();
	}

	private void MakeGun()
	{
		gun = ThingMaker.MakeThing(Props.turretDef);
		UpdateGunVerbs();
	}

	private void UpdateGunVerbs()
	{
		List<Verb> allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
		for (int i = 0; i < allVerbs.Count; i++)
		{
			Verb verb = allVerbs[i];
			verb.caster = parent;
			verb.castCompleteCallback = delegate
			{
				burstCooldownTicksLeft = AttackVerb.verbProps.defaultCooldownTime.SecondsToTicks();
			};
		}
	}

	public override void CompTick()
	{
		if (!CanShoot)
		{
			return;
		}
		if (currentTarget.IsValid)
		{
			curRotation = (currentTarget.Cell.ToVector3Shifted() - parent.DrawPos).AngleFlat() + Props.angleOffset;
		}
		AttackVerb.VerbTick();
		if (AttackVerb.state == VerbState.Bursting)
		{
			return;
		}
		if (WarmingUp)
		{
			burstWarmupTicksLeft--;
			if (burstWarmupTicksLeft == 0)
			{
				AttackVerb.TryStartCastOn(currentTarget, surpriseAttack: false, canHitNonTargetPawns: true, preventFriendlyFire: false, nonInterruptingSelfCast: true);
				lastAttackTargetTick = Find.TickManager.TicksGame;
				lastAttackedTarget = currentTarget;
			}
			return;
		}
		if (burstCooldownTicksLeft > 0)
		{
			burstCooldownTicksLeft--;
		}
		if (burstCooldownTicksLeft <= 0 && parent.IsHashIntervalTick(10))
		{
			currentTarget = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(this, TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable);
			if (currentTarget.IsValid)
			{
				burstWarmupTicksLeft = 1;
			}
			else
			{
				ResetCurrentTarget();
			}
		}
	}

	private void ResetCurrentTarget()
	{
		currentTarget = LocalTargetInfo.Invalid;
		burstWarmupTicksLeft = 0;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
		if (parent is Pawn { IsColonyMechPlayerControlled: not false })
		{
			Command_Toggle command_Toggle = new Command_Toggle();
			command_Toggle.defaultLabel = "CommandToggleTurret".Translate();
			command_Toggle.defaultDesc = "CommandToggleTurretDesc".Translate();
			command_Toggle.isActive = () => fireAtWill;
			command_Toggle.icon = ToggleTurretIcon.Texture;
			command_Toggle.toggleAction = delegate
			{
				fireAtWill = !fireAtWill;
			};
			yield return command_Toggle;
		}
	}

	public override List<PawnRenderNode> CompRenderNodes()
	{
		if (!Props.renderNodeProperties.NullOrEmpty() && parent is Pawn pawn)
		{
			List<PawnRenderNode> list = new List<PawnRenderNode>();
			{
				foreach (PawnRenderNodeProperties renderNodeProperty in Props.renderNodeProperties)
				{
					PawnRenderNode_TurretGun pawnRenderNode_TurretGun = (PawnRenderNode_TurretGun)Activator.CreateInstance(renderNodeProperty.nodeClass, pawn, renderNodeProperty, pawn.Drawer.renderer.renderTree);
					pawnRenderNode_TurretGun.turretComp = this;
					list.Add(pawnRenderNode_TurretGun);
				}
				return list;
			}
		}
		return base.CompRenderNodes();
	}

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
	{
		if (Props.turretDef != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, "Turret".Translate(), Props.turretDef.LabelCap, "Stat_Thing_TurretDesc".Translate(), 5600, null, Gen.YieldSingle(new Dialog_InfoCard.Hyperlink(Props.turretDef)));
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref burstCooldownTicksLeft, "burstCooldownTicksLeft", 0);
		Scribe_Values.Look(ref burstWarmupTicksLeft, "burstWarmupTicksLeft", 0);
		Scribe_TargetInfo.Look(ref currentTarget, "currentTarget");
		Scribe_Deep.Look(ref gun, "gun");
		Scribe_Values.Look(ref fireAtWill, "fireAtWill", defaultValue: true);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (gun == null)
			{
				Log.Error("CompTurrentGun had null gun after loading. Recreating.");
				MakeGun();
			}
			else
			{
				UpdateGunVerbs();
			}
		}
	}
}
