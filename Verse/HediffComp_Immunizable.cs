using RimWorld;
using System.Text;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class HediffComp_Immunizable : HediffComp_SeverityPerDay
	{
		private float severityPerDayNotImmuneRandomFactor = 1f;

		private static readonly Texture2D IconImmune = ContentFinder<Texture2D>.Get("UI/Icons/Medical/IconImmune");

		public HediffCompProperties_Immunizable Props => (HediffCompProperties_Immunizable)props;

		public override string CompLabelInBracketsExtra
		{
			get
			{
				if (FullyImmune)
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
				if (base.Def.PossibleToDevelopImmunityNaturally() && !FullyImmune)
				{
					return "Immunity".Translate() + ": " + (Mathf.Floor(Immunity * 100f) / 100f).ToStringPercent();
				}
				return null;
			}
		}

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

		protected override float SeverityChangePerDay()
		{
			if (!FullyImmune)
			{
				return Props.severityPerDayNotImmune * severityPerDayNotImmuneRandomFactor;
			}
			return Props.severityPerDayImmune;
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
					stringBuilder.AppendLine("  pawn immunity gain speed: " + StatDefOf.ImmunityGainSpeed.ValueToString(base.Pawn.GetStatValue(StatDefOf.ImmunityGainSpeed)));
				}
			}
			return stringBuilder.ToString();
		}
	}
}
