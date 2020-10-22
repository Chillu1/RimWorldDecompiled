using System.Text;

namespace Verse
{
	public class Hediff_MissingPart : HediffWithComps
	{
		public HediffDef lastInjury;

		private bool isFreshInt;

		public override float SummaryHealthPercentImpact
		{
			get
			{
				if (!IsFreshNonSolidExtremity)
				{
					return 0f;
				}
				if (base.Part.def.tags.NullOrEmpty() && base.Part.parts.NullOrEmpty() && !base.Bleeding)
				{
					return 0f;
				}
				return (float)base.Part.def.hitPoints / (75f * pawn.HealthScale);
			}
		}

		public override bool ShouldRemove => false;

		public override string LabelBase
		{
			get
			{
				if (lastInjury != null && lastInjury.injuryProps.useRemovedLabel)
				{
					return "RemovedBodyPart".Translate();
				}
				if (lastInjury == null || base.Part.depth == BodyPartDepth.Inside)
				{
					bool solid = base.Part.def.IsSolid(base.Part, pawn.health.hediffSet.hediffs);
					return HealthUtility.GetGeneralDestroyedPartLabel(base.Part, IsFreshNonSolidExtremity, solid);
				}
				if (base.Part.def.socketed && !lastInjury.injuryProps.destroyedOutLabel.NullOrEmpty())
				{
					return lastInjury.injuryProps.destroyedOutLabel;
				}
				return lastInjury.injuryProps.destroyedLabel;
			}
		}

		public override string LabelInBrackets
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.LabelInBrackets);
				if (IsFreshNonSolidExtremity)
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append("FreshMissingBodyPart".Translate());
				}
				return stringBuilder.ToString();
			}
		}

		public override float BleedRate
		{
			get
			{
				if (pawn.Dead || !IsFreshNonSolidExtremity || ParentIsMissing)
				{
					return 0f;
				}
				return base.Part.def.GetMaxHealth(pawn) * def.injuryProps.bleedRate * base.Part.def.bleedRate;
			}
		}

		public override float PainOffset
		{
			get
			{
				if (pawn.Dead || causesNoPain || !IsFreshNonSolidExtremity || ParentIsMissing)
				{
					return 0f;
				}
				return base.Part.def.GetMaxHealth(pawn) * def.injuryProps.painPerSeverity;
			}
		}

		private bool ParentIsMissing
		{
			get
			{
				for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
				{
					Hediff_MissingPart hediff_MissingPart = pawn.health.hediffSet.hediffs[i] as Hediff_MissingPart;
					if (hediff_MissingPart != null && hediff_MissingPart.Part == base.Part.parent)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool IsFresh
		{
			get
			{
				if (isFreshInt)
				{
					return !TicksAfterNoLongerFreshPassed;
				}
				return false;
			}
			set
			{
				isFreshInt = value;
			}
		}

		public bool IsFreshNonSolidExtremity
		{
			get
			{
				if (Current.ProgramState == ProgramState.Entry)
				{
					return false;
				}
				if (!IsFresh || base.Part.depth == BodyPartDepth.Inside || base.Part.def.IsSolid(base.Part, pawn.health.hediffSet.hediffs) || ParentIsMissing)
				{
					return false;
				}
				return true;
			}
		}

		private bool TicksAfterNoLongerFreshPassed => ageTicks >= 90000;

		public override bool TendableNow(bool ignoreTimer = false)
		{
			return IsFreshNonSolidExtremity;
		}

		public override void Tick()
		{
			bool ticksAfterNoLongerFreshPassed = TicksAfterNoLongerFreshPassed;
			base.Tick();
			bool ticksAfterNoLongerFreshPassed2 = TicksAfterNoLongerFreshPassed;
			if (ticksAfterNoLongerFreshPassed != ticksAfterNoLongerFreshPassed2)
			{
				pawn.health.Notify_HediffChanged(this);
			}
		}

		public override void Tended_NewTemp(float quality, float maxQuality, int batchPosition = 0)
		{
			base.Tended_NewTemp(quality, maxQuality, batchPosition);
			IsFresh = false;
			pawn.health.Notify_HediffChanged(this);
		}

		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);
			if (Current.ProgramState != ProgramState.Playing || PawnGenerator.IsBeingGenerated(pawn))
			{
				IsFresh = false;
			}
			pawn.health.RestorePart(base.Part, this, checkStateChange: false);
			for (int i = 0; i < base.Part.parts.Count; i++)
			{
				Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(def, pawn);
				hediff_MissingPart.IsFresh = false;
				hediff_MissingPart.lastInjury = lastInjury;
				hediff_MissingPart.Part = base.Part.parts[i];
				pawn.health.hediffSet.AddDirect(hediff_MissingPart);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref lastInjury, "lastInjury");
			Scribe_Values.Look(ref isFreshInt, "isFresh", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null)
			{
				Log.Error("Hediff_MissingPart has null part after loading.");
				pawn.health.hediffSet.hediffs.Remove(this);
			}
		}
	}
}
