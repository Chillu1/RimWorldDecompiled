using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class HediffComp_TendDuration : HediffComp_SeverityModifierBase
{
	public int tendTicksLeft = -1;

	public float tendQuality;

	private float totalTendQuality;

	public const float TendQualityRandomVariance = 0.25f;

	private static readonly Color UntendedColor = new ColorInt(116, 101, 72).ToColor;

	private static readonly Texture2D TendedIcon_Need_General = ContentFinder<Texture2D>.Get("UI/Icons/Medical/TendedNeed");

	private static readonly Texture2D TendedIcon_Well_General = ContentFinder<Texture2D>.Get("UI/Icons/Medical/TendedWell");

	private static readonly Texture2D TendedIcon_Well_Injury = ContentFinder<Texture2D>.Get("UI/Icons/Medical/BandageWell");

	public HediffCompProperties_TendDuration TProps => (HediffCompProperties_TendDuration)props;

	public override bool CompShouldRemove
	{
		get
		{
			if (base.CompShouldRemove)
			{
				return true;
			}
			if (TProps.disappearsAtTotalTendQuality >= 0)
			{
				return totalTendQuality >= (float)TProps.disappearsAtTotalTendQuality;
			}
			return false;
		}
	}

	public bool IsTended
	{
		get
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return false;
			}
			return tendTicksLeft > 0;
		}
	}

	public bool AllowTend
	{
		get
		{
			if (TProps.TendIsPermanent)
			{
				return !IsTended;
			}
			return TProps.TendTicksOverlap > tendTicksLeft;
		}
	}

	public override string CompTipStringExtra
	{
		get
		{
			if (parent.IsPermanent())
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (!IsTended)
			{
				if (!base.Pawn.Dead && parent.TendableNow())
				{
					stringBuilder.AppendLine("NeedsTendingNow".Translate());
				}
			}
			else
			{
				if (TProps.showTendQuality)
				{
					string text = null;
					text = ((parent.Part != null && parent.Part.def.IsSolid(parent.Part, base.Pawn.health.hediffSet.hediffs)) ? TProps.labelSolidTendedWell : ((parent.Part == null || parent.Part.depth != BodyPartDepth.Inside) ? TProps.labelTendedWell : TProps.labelTendedWellInner));
					if (text != null)
					{
						stringBuilder.AppendLine(text.CapitalizeFirst() + " (" + "quality".Translate() + " " + tendQuality.ToStringPercent("F0") + ")");
					}
					else
					{
						stringBuilder.AppendLine(string.Format("{0}: {1}", "TendQuality".Translate(), tendQuality.ToStringPercent()));
					}
					if (TProps.disappearsAtTotalTendQuality >= 0)
					{
						stringBuilder.AppendLine("DisappearsAtTotalTendQuality".Translate() + ": " + totalTendQuality.ToStringPercent() + " / " + GenText.ToStringPercent(TProps.disappearsAtTotalTendQuality));
					}
				}
				if (!base.Pawn.Dead && !TProps.TendIsPermanent && parent.TendableNow(ignoreTimer: true))
				{
					int num = tendTicksLeft - TProps.TendTicksOverlap;
					if (num < 0)
					{
						stringBuilder.AppendLine("CanTendNow".Translate());
					}
					else if ("NextTendIn".CanTranslate())
					{
						stringBuilder.AppendLine("NextTendIn".Translate(num.ToStringTicksToPeriod()));
					}
					else
					{
						stringBuilder.AppendLine("NextTreatmentIn".Translate(num.ToStringTicksToPeriod()));
					}
					stringBuilder.AppendLine("TreatmentExpiresIn".Translate(tendTicksLeft.ToStringTicksToPeriod()));
				}
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}
	}

	public override TextureAndColor CompStateIcon
	{
		get
		{
			if (parent is Hediff_Injury)
			{
				if (IsTended && !parent.IsPermanent())
				{
					Color color = Color.Lerp(UntendedColor, Color.white, Mathf.Clamp01(tendQuality));
					return new TextureAndColor(TendedIcon_Well_Injury, color);
				}
			}
			else if (!(parent is Hediff_MissingPart) && !parent.FullyImmune())
			{
				if (IsTended)
				{
					Color color2 = Color.Lerp(UntendedColor, Color.white, Mathf.Clamp01(tendQuality));
					return new TextureAndColor(TendedIcon_Well_General, color2);
				}
				return TendedIcon_Need_General;
			}
			return TextureAndColor.None;
		}
	}

	public override void CompExposeData()
	{
		Scribe_Values.Look(ref tendTicksLeft, "tendTicksLeft", -1);
		Scribe_Values.Look(ref tendQuality, "tendQuality", 0f);
		Scribe_Values.Look(ref totalTendQuality, "totalTendQuality", 0f);
	}

	public override float SeverityChangePerDay()
	{
		if (IsTended)
		{
			return TProps.severityPerDayTended * tendQuality;
		}
		return 0f;
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		base.CompPostTick(ref severityAdjustment);
		if (tendTicksLeft > 0 && !TProps.TendIsPermanent)
		{
			tendTicksLeft--;
		}
	}

	public override void CompTended(float quality, float maxQuality, int batchPosition = 0)
	{
		tendQuality = Mathf.Clamp(quality + Rand.Range(-0.25f, 0.25f), 0f, maxQuality);
		totalTendQuality += tendQuality;
		if (TProps.TendIsPermanent)
		{
			tendTicksLeft = 1;
		}
		else
		{
			tendTicksLeft = Mathf.Max(0, tendTicksLeft) + TProps.TendTicksFull;
		}
		if (batchPosition == 0 && base.Pawn.Spawned)
		{
			string text = "TextMote_Tended".Translate(parent.Label).CapitalizeFirst() + "\n" + "Quality".Translate() + " " + tendQuality.ToStringPercent();
			MoteMaker.ThrowText(base.Pawn.DrawPos, base.Pawn.Map, text, Color.white, 3.65f);
		}
		base.Pawn.health.Notify_HediffChanged(parent);
	}

	public override string CompDebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (IsTended)
		{
			stringBuilder.AppendLine("tendQuality: " + tendQuality.ToStringPercent());
			if (!TProps.TendIsPermanent)
			{
				stringBuilder.AppendLine("tendTicksLeft: " + tendTicksLeft);
			}
		}
		else
		{
			stringBuilder.AppendLine("untended");
		}
		stringBuilder.AppendLine("severity/day: " + SeverityChangePerDay());
		if (TProps.disappearsAtTotalTendQuality >= 0)
		{
			stringBuilder.AppendLine("totalTendQuality: " + totalTendQuality.ToString("F2") + " / " + TProps.disappearsAtTotalTendQuality);
		}
		return stringBuilder.ToString().Trim();
	}
}
