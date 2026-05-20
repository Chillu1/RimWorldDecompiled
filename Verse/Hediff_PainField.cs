using System;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

public class Hediff_PainField : HediffWithComps
{
	public float painSeverity;

	public float goal;

	public float vel;

	private const float SeveritySmoothTime = 0.25f;

	private HediffComp_PainField painComp;

	private HediffComp_Link linkComp;

	private float psychicSensitivityCached = -1f;

	public HediffComp_PainField PainComp => painComp ?? (painComp = GetComp<HediffComp_PainField>());

	public HediffComp_Link LinkComp => linkComp ?? (linkComp = GetComp<HediffComp_Link>());

	public Pawn SourcePawn => LinkComp.OtherPawn;

	public override float PainOffset => Severity;

	public override string SeverityLabel
	{
		get
		{
			if (Severity == 0f)
			{
				return null;
			}
			return Severity.ToStringPercent();
		}
	}

	public override bool ShouldRemove
	{
		get
		{
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (comps[i].CompShouldRemove)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public override float Severity => painSeverity;

	public override string Description
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(def.Description);
			if (SourcePawn == null)
			{
				return stringBuilder.ToString();
			}
			CompActivity comp = SourcePawn.GetComp<CompActivity>();
			if (comp != null)
			{
				float f = PainComp.Props.activityMultiplier.Evaluate(comp.ActivityLevel);
				stringBuilder.Append(string.Format("\n{0}: x{1}", "ActivityLevelDesc".Translate().CapitalizeFirst(), f.ToStringPercent("0")));
			}
			if (PainComp.Props.disableWhenSuppressed && (SourcePawn.health.hediffSet.IsPsychicallySuppressed || pawn.health.hediffSet.IsPsychicallySuppressed))
			{
				stringBuilder.Append(string.Format("\n{0}: x0", "PsychicallySuppressedDesc".Translate().CapitalizeFirst()));
			}
			return stringBuilder.ToString();
		}
	}

	public override void PostMake()
	{
		base.PostMake();
		if (PainComp == null)
		{
			Log.ErrorOnce("Attempted to create a pain field hediff but missing the HediffComp_PainField component", 94756930);
		}
		if (LinkComp == null)
		{
			Log.ErrorOnce("Attempted to create a pain field hediff but missing the Hediff_Link component", 47836289);
		}
	}

	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModLister.CheckAnomaly("Pain field"))
		{
			pawn.health.RemoveHediff(this);
		}
		else
		{
			base.PostAdd(dinfo);
		}
	}

	public override void PostTickInterval(int delta)
	{
		float num = painSeverity;
		goal = CalculateSeverity(delta);
		base.PostTickInterval(delta);
		painSeverity = Mathf.SmoothDamp(painSeverity, goal, ref vel, 0.25f, 10f, 1f / 60f);
		painSeverity = Mathf.Clamp01(painSeverity);
		if (Math.Abs(num - painSeverity) > Mathf.Epsilon)
		{
			pawn.health.hediffSet.DirtyPainCache();
			pawn.health.CheckForStateChange(null, this);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref painSeverity, "painSeverity", 0f);
		Scribe_Values.Look(ref goal, "goal", 0f);
		Scribe_Values.Look(ref vel, "vel", 0f);
	}

	private float CalculateSeverity(int delta)
	{
		if (SourcePawn == null)
		{
			return 0f;
		}
		if (PainComp.Props.disableWhenSuppressed && (SourcePawn.health.hediffSet.IsPsychicallySuppressed || pawn.health.hediffSet.IsPsychicallySuppressed))
		{
			return 0f;
		}
		if (psychicSensitivityCached < 0f || pawn.IsHashIntervalTick(300, delta))
		{
			psychicSensitivityCached = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
		}
		float num = pawn.PositionHeld.DistanceTo(SourcePawn.PositionHeld);
		float num2 = 0f;
		if (num < PainComp.Props.painDistance + 0.1f)
		{
			num2 = PainComp.Props.painInRange;
		}
		if (SourcePawn.TryGetComp<CompActivity>(out var comp))
		{
			if (comp.IsActive)
			{
				num2 = Mathf.Max(PainComp.Props.activatedMinimum, num2);
			}
			num2 *= PainComp.Props.activityMultiplier.Evaluate(comp.ActivityLevel);
		}
		num2 *= psychicSensitivityCached * PainComp.Props.psychicSensitivityMultiplier;
		return Mathf.Clamp01(num2);
	}
}
