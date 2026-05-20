using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class HediffComp_Immunizable : HediffComp_SeverityModifierBase
{
	private float severityPerDayNotImmuneRandomFactor = 1f;

	private static readonly Texture2D IconImmune = ContentFinder<Texture2D>.Get("UI/Icons/Medical/IconImmune");

	public HediffCompProperties_Immunizable Props => (HediffCompProperties_Immunizable)props;

	public override string CompLabelInBracketsExtra
	{
		get
		{
			if (!Hidden && FullyImmune)
			{
				return "DevelopedImmunityLower".Translate();
			}
			return null;
		}
	}

	public override string CompTipStringExtra
	{
		get
		{
			if (!Hidden && base.Def.PossibleToDevelopImmunityNaturally() && !FullyImmune)
			{
				return "Immunity".Translate() + ": " + NaturalImmunity.ToStringPercent("0.#");
			}
			return null;
		}
	}

	public float NaturalImmunity => base.Pawn.health.immunity.GetImmunity(base.Def, naturalImmunityOnly: true);

	public float Immunity => base.Pawn.health.immunity.GetImmunity(base.Def);

	public bool FullyImmune => Immunity >= 1f;

	public override TextureAndColor CompStateIcon
	{
		get
		{
			if (FullyImmune)
			{
				return IconImmune;
			}
			return TextureAndColor.None;
		}
	}

	private bool Hidden
	{
		get
		{
			if (!Prefs.DevMode || !DebugSettings.godMode)
			{
				return Props.hidden;
			}
			return false;
		}
	}

	private float SeverityFactorFromHediffs
	{
		get
		{
			float num = 1f;
			if (!Props.severityFactorsFromHediffs.NullOrEmpty())
			{
				for (int i = 0; i < Props.severityFactorsFromHediffs.Count; i++)
				{
					if (base.Pawn.health.hediffSet.GetFirstHediffOfDef(Props.severityFactorsFromHediffs[i].HediffDef) != null)
					{
						num *= Props.severityFactorsFromHediffs[i].Factor;
					}
				}
			}
			return num;
		}
	}

	public override void CopyFrom(HediffComp other)
	{
		if (other is HediffComp_Immunizable hediffComp_Immunizable)
		{
			Pawn pawn = parent.pawn;
			pawn.health.immunity.TryAddImmunityRecord(parent.def, parent.def);
			ImmunityRecord immunityRecord = pawn.health.immunity.GetImmunityRecord(parent.def);
			if (immunityRecord != null)
			{
				immunityRecord.immunity = hediffComp_Immunizable.Immunity;
			}
		}
	}

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		base.CompPostPostAdd(dinfo);
		severityPerDayNotImmuneRandomFactor = Props.severityPerDayNotImmuneRandomFactor.RandomInRange;
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Values.Look(ref severityPerDayNotImmuneRandomFactor, "severityPerDayNotImmuneRandomFactor", 1f);
	}

	public override float SeverityChangePerDay()
	{
		return (FullyImmune ? Props.severityPerDayImmune : (Props.severityPerDayNotImmune * severityPerDayNotImmuneRandomFactor)) * SeverityFactorFromHediffs;
	}

	public override string CompDebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(base.CompDebugString());
		if (severityPerDayNotImmuneRandomFactor != 1f)
		{
			stringBuilder.AppendLine("severityPerDayNotImmuneRandomFactor: " + severityPerDayNotImmuneRandomFactor.ToString("0.##"));
		}
		if (!base.Pawn.Dead)
		{
			ImmunityRecord immunityRecord = base.Pawn.health.immunity.GetImmunityRecord(base.Def);
			if (immunityRecord != null)
			{
				stringBuilder.AppendLine("immunity change per day: " + (immunityRecord.ImmunityChangePerTick(base.Pawn, sick: true, parent) * 60000f).ToString("F3"));
				stringBuilder.AppendLine("pawn immunity gain speed: " + StatDefOf.ImmunityGainSpeed.ValueToString(base.Pawn.GetStatValue(StatDefOf.ImmunityGainSpeed)));
			}
		}
		return stringBuilder.ToString();
	}
}
