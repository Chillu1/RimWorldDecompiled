using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_ResurrectMech : CompAbilityEffect
{
	[LoadAlias("charges")]
	private int resurrectCharges = 30;

	private Dictionary<MechWeightClassDef, int> costsByWeightClass;

	private Gizmo gizmo;

	public new CompProperties_ResurrectMech Props => (CompProperties_ResurrectMech)props;

	public int ChargesRemaining => resurrectCharges;

	public override bool CanCast => resurrectCharges > 0;

	public override void Initialize(AbilityCompProperties props)
	{
		base.Initialize(props);
		costsByWeightClass = new Dictionary<MechWeightClassDef, int>();
		for (int i = 0; i < Props.costs.Count; i++)
		{
			costsByWeightClass[Props.costs[i].weightClass] = Props.costs[i].cost;
		}
	}

	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (!base.CanApplyOn(target, dest))
		{
			return false;
		}
		if (!target.HasThing || !(target.Thing is Corpse corpse))
		{
			return false;
		}
		if (!CanResurrect(corpse))
		{
			return false;
		}
		return true;
	}

	private bool TryGetResurrectCost(Corpse corpse, out int cost)
	{
		if (corpse.InnerPawn.RaceProps.mechWeightClass != null && costsByWeightClass.ContainsKey(corpse.InnerPawn.RaceProps.mechWeightClass))
		{
			cost = costsByWeightClass[corpse.InnerPawn.RaceProps.mechWeightClass];
			return true;
		}
		cost = -1;
		return false;
	}

	private bool CanResurrect(Corpse corpse)
	{
		if (!corpse.InnerPawn.RaceProps.IsMechanoid || corpse.InnerPawn.RaceProps.mechWeightClass == null || !corpse.InnerPawn.RaceProps.mechWeightClass.canResurrect)
		{
			return false;
		}
		if (corpse.InnerPawn.Faction != parent.pawn.Faction)
		{
			return false;
		}
		if (corpse.InnerPawn.kindDef.abilities != null && corpse.InnerPawn.kindDef.abilities.Contains(AbilityDefOf.ResurrectionMech))
		{
			return false;
		}
		if (corpse.timeOfDeath < Find.TickManager.TicksGame - Props.maxCorpseAgeTicks)
		{
			return false;
		}
		if (!TryGetResurrectCost(corpse, out var cost) || cost > resurrectCharges)
		{
			return false;
		}
		return true;
	}

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Corpse corpse = (Corpse)target.Thing;
		if (CanResurrect(corpse) && TryGetResurrectCost(corpse, out var cost))
		{
			Pawn innerPawn = corpse.InnerPawn;
			resurrectCharges -= cost;
			ResurrectionUtility.TryResurrect(innerPawn);
			if (Props.appliedEffecterDef != null)
			{
				Effecter effecter = Props.appliedEffecterDef.SpawnAttached(innerPawn, innerPawn.MapHeld);
				effecter.Trigger(innerPawn, innerPawn);
				effecter.Cleanup();
			}
			innerPawn.stances.stagger.StaggerFor(60);
		}
	}

	public override bool GizmoDisabled(out string reason)
	{
		reason = null;
		return false;
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (gizmo == null)
		{
			gizmo = new Gizmo_MechResurrectionCharges(this);
		}
		yield return gizmo;
		if (DebugSettings.ShowDevGizmos)
		{
			Command_Action add = new Command_Action
			{
				defaultLabel = "DEV: Add charge",
				action = delegate
				{
					resurrectCharges++;
				}
			};
			yield return add;
			new Command_Action
			{
				defaultLabel = "DEV: Remove charge",
				action = delegate
				{
					resurrectCharges--;
					resurrectCharges = Mathf.Max(resurrectCharges, 0);
				}
			};
			yield return add;
		}
	}

	public override IEnumerable<Mote> CustomWarmupMotes(LocalTargetInfo target)
	{
		foreach (LocalTargetInfo affectedTarget in parent.GetAffectedTargets(target))
		{
			Thing thing = affectedTarget.Thing;
			yield return MoteMaker.MakeAttachedOverlay(thing, ThingDefOf.Mote_MechResurrectWarmupOnTarget, Vector3.zero);
		}
	}

	public override void PostApplied(List<LocalTargetInfo> targets, Map map)
	{
		Vector3 zero = Vector3.zero;
		foreach (LocalTargetInfo target in targets)
		{
			zero += target.Cell.ToVector3Shifted();
		}
		zero /= (float)targets.Count;
		IntVec3 intVec = zero.ToIntVec3();
		EffecterDefOf.ApocrionAoeResolve.Spawn(intVec, map).EffectTick(new TargetInfo(intVec, map), new TargetInfo(intVec, map));
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref resurrectCharges, "resurrectCharges", 30);
	}
}
