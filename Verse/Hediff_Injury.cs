using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

public class Hediff_Injury : HediffWithComps
{
	public bool destroysBodyParts = true;

	private static readonly Color PermanentInjuryColor = new Color(0.72f, 0.72f, 0.72f);

	public override int UIGroupKey
	{
		get
		{
			int num = base.UIGroupKey;
			if (this.IsTended())
			{
				num = Gen.HashCombineInt(num, 152235495);
			}
			return num;
		}
	}

	public override string LabelBase
	{
		get
		{
			HediffComp_GetsPermanent hediffComp_GetsPermanent = this.TryGetComp<HediffComp_GetsPermanent>();
			if (hediffComp_GetsPermanent != null && hediffComp_GetsPermanent.IsPermanent)
			{
				if (base.Part != null && base.Part.def.delicate && !hediffComp_GetsPermanent.Props.instantlyPermanentLabel.NullOrEmpty())
				{
					return hediffComp_GetsPermanent.Props.instantlyPermanentLabel;
				}
				if (!hediffComp_GetsPermanent.Props.permanentLabel.NullOrEmpty())
				{
					return hediffComp_GetsPermanent.Props.permanentLabel;
				}
			}
			return base.LabelBase;
		}
	}

	public override string LabelInBrackets
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.LabelInBrackets);
			if (sourceHediffDef != null)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(sourceHediffDef.label);
			}
			else if (sourceDef != null)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(", ");
				}
				if (!sourceToolLabel.NullOrEmpty())
				{
					stringBuilder.Append("SourceToolLabel".Translate(sourceLabel, sourceToolLabel));
				}
				else if (sourceBodyPartGroup != null)
				{
					stringBuilder.Append("SourceToolLabel".Translate(sourceLabel, sourceBodyPartGroup.LabelShort));
				}
				else
				{
					stringBuilder.Append(sourceLabel);
				}
			}
			HediffComp_GetsPermanent hediffComp_GetsPermanent = this.TryGetComp<HediffComp_GetsPermanent>();
			if (hediffComp_GetsPermanent != null && hediffComp_GetsPermanent.IsPermanent && hediffComp_GetsPermanent.PainCategory != PainCategory.Painless)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(("PainCategory_" + hediffComp_GetsPermanent.PainCategory).Translate());
			}
			return stringBuilder.ToString();
		}
	}

	public override Color LabelColor
	{
		get
		{
			if (this.IsPermanent())
			{
				return PermanentInjuryColor;
			}
			return Color.white;
		}
	}

	public override string SeverityLabel
	{
		get
		{
			if (Severity == 0f)
			{
				return null;
			}
			return Severity.ToString("F1");
		}
	}

	public override float SummaryHealthPercentImpact
	{
		get
		{
			if (this.IsPermanent() || !Visible)
			{
				return 0f;
			}
			return Severity / (75f * pawn.HealthScale);
		}
	}

	public override float PainOffset
	{
		get
		{
			if (pawn.Dead || (base.Part != null && pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(base.Part)) || causesNoPain)
			{
				return 0f;
			}
			HediffComp_GetsPermanent hediffComp_GetsPermanent = this.TryGetComp<HediffComp_GetsPermanent>();
			float num = ((hediffComp_GetsPermanent == null || !hediffComp_GetsPermanent.IsPermanent) ? (Severity * def.injuryProps.painPerSeverity) : (Severity * def.injuryProps.averagePainPerSeverityPermanent * hediffComp_GetsPermanent.PainFactor));
			return num / pawn.HealthScale;
		}
	}

	public override float BleedRate
	{
		get
		{
			if (pawn.Dead)
			{
				return 0f;
			}
			if (BleedingStoppedDueToAge)
			{
				return 0f;
			}
			if (!pawn.health.CanBleed)
			{
				return 0f;
			}
			if ((base.Part != null && base.Part.def.IsSolid(base.Part, pawn.health.hediffSet.hediffs)) || this.IsTended() || this.IsPermanent())
			{
				return 0f;
			}
			if (base.Part != null)
			{
				Hediff directlyAddedPartFor = pawn.health.hediffSet.GetDirectlyAddedPartFor(base.Part);
				if (directlyAddedPartFor != null && !directlyAddedPartFor.def.organicAddedBodypart)
				{
					return 0f;
				}
			}
			float num = Severity * def.injuryProps.bleedRate * pawn.RaceProps.bleedRateFactor;
			if (base.Part != null)
			{
				num *= base.Part.def.bleedRate;
			}
			return num;
		}
	}

	private int AgeTicksToStopBleeding
	{
		get
		{
			float t = Mathf.Clamp(Mathf.InverseLerp(1f, 30f, Severity), 0f, 1f);
			return 90000 + Mathf.RoundToInt(Mathf.Lerp(0f, 90000f, t));
		}
	}

	private bool BleedingStoppedDueToAge => ageTicks >= AgeTicksToStopBleeding;

	public override void TickInterval(int delta)
	{
		bool bleedingStoppedDueToAge = BleedingStoppedDueToAge;
		base.TickInterval(delta);
		bool bleedingStoppedDueToAge2 = BleedingStoppedDueToAge;
		if (bleedingStoppedDueToAge != bleedingStoppedDueToAge2)
		{
			pawn.health.Notify_HediffChanged(this);
		}
	}

	public override void Heal(float amount)
	{
		Severity -= amount;
		if (comps != null)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].CompPostInjuryHeal(amount);
			}
		}
		pawn.health.Notify_HediffChanged(this);
	}

	public override bool TryMergeWith(Hediff other)
	{
		if (!(other is Hediff_Injury hediff_Injury) || hediff_Injury.def != def || hediff_Injury.Part != base.Part || hediff_Injury.IsTended() || hediff_Injury.IsPermanent() || this.IsTended() || this.IsPermanent() || !def.injuryProps.canMerge)
		{
			return false;
		}
		return base.TryMergeWith(other);
	}

	public override void PostAdd(DamageInfo? dinfo)
	{
		base.PostAdd(dinfo);
		if (base.Part != null && base.Part.coverageAbs <= 0f && (!dinfo.HasValue || dinfo.Value.Def != DamageDefOf.SurgicalCut))
		{
			Log.Error("Added injury to " + base.Part.def?.ToString() + " but it should be impossible to hit it. pawn=" + pawn.ToStringSafe() + " dinfo=" + dinfo.ToStringSafe());
		}
	}

	public override void PostRemoved()
	{
		base.PostRemoved();
		pawn.Drawer.renderer.WoundOverlays.ClearCache();
		PortraitsCache.SetDirty(pawn);
		GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref destroysBodyParts, "destroysBodyParts", defaultValue: true);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null)
		{
			Log.Error("Hediff_Injury has null part after loading.");
			pawn.health.hediffSet.hediffs.Remove(this);
		}
	}
}
