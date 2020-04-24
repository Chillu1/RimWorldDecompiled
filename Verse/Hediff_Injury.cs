using System.Text;
using UnityEngine;

namespace Verse
{
	public class Hediff_Injury : HediffWithComps
	{
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
					if (base.Part.def.delicate && !hediffComp_GetsPermanent.Props.instantlyPermanentLabel.NullOrEmpty())
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
				else if (source != null)
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(source.label);
					if (sourceBodyPartGroup != null)
					{
						stringBuilder.Append(" ");
						stringBuilder.Append(sourceBodyPartGroup.LabelShort);
					}
				}
				HediffComp_GetsPermanent hediffComp_GetsPermanent = this.TryGetComp<HediffComp_GetsPermanent>();
				if (hediffComp_GetsPermanent != null && hediffComp_GetsPermanent.IsPermanent && hediffComp_GetsPermanent.PainCategory != 0)
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(("PainCategory_" + hediffComp_GetsPermanent.PainCategory.ToString()).Translate());
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
				if (pawn.Dead || pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(base.Part) || causesNoPain)
				{
					return 0f;
				}
				HediffComp_GetsPermanent hediffComp_GetsPermanent = this.TryGetComp<HediffComp_GetsPermanent>();
				if (hediffComp_GetsPermanent != null && hediffComp_GetsPermanent.IsPermanent)
				{
					return Severity * def.injuryProps.averagePainPerSeverityPermanent * hediffComp_GetsPermanent.PainFactor;
				}
				return Severity * def.injuryProps.painPerSeverity;
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
				if (base.Part.def.IsSolid(base.Part, pawn.health.hediffSet.hediffs) || this.IsTended() || this.IsPermanent())
				{
					return 0f;
				}
				if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(base.Part))
				{
					return 0f;
				}
				float num = Severity * def.injuryProps.bleedRate;
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

		public override void Tick()
		{
			bool bleedingStoppedDueToAge = BleedingStoppedDueToAge;
			base.Tick();
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
			Hediff_Injury hediff_Injury = other as Hediff_Injury;
			if (hediff_Injury == null || hediff_Injury.def != def || hediff_Injury.Part != base.Part || hediff_Injury.IsTended() || hediff_Injury.IsPermanent() || this.IsTended() || this.IsPermanent() || !def.injuryProps.canMerge)
			{
				return false;
			}
			return base.TryMergeWith(other);
		}

		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);
			if (base.Part != null && base.Part.coverageAbs <= 0f)
			{
				Log.Error("Added injury to " + base.Part.def + " but it should be impossible to hit it. pawn=" + pawn.ToStringSafe() + " dinfo=" + dinfo.ToStringSafe());
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null)
			{
				Log.Error("Hediff_Injury has null part after loading.");
				pawn.health.hediffSet.hediffs.Remove(this);
			}
		}
	}
}
