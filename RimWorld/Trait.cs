using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Trait : IExposable
	{
		public Pawn pawn;

		public TraitDef def;

		private int degree;

		private bool scenForced;

		public int Degree => degree;

		public TraitDegreeData CurrentData => def.DataAtDegree(degree);

		public string Label => CurrentData.GetLabelFor(pawn);

		public string LabelCap => CurrentData.GetLabelCapFor(pawn);

		public bool ScenForced => scenForced;

		public Trait()
		{
		}

		public Trait(TraitDef def, int degree = 0, bool forced = false)
		{
			this.def = def;
			this.degree = degree;
			scenForced = forced;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref degree, "degree", 0);
			Scribe_Values.Look(ref scenForced, "scenForced", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && def == null)
			{
				def = DefDatabase<TraitDef>.GetRandom();
				degree = PawnGenerator.RandomTraitDegree(def);
			}
		}

		public float OffsetOfStat(StatDef stat)
		{
			float num = 0f;
			TraitDegreeData currentData = CurrentData;
			if (currentData.statOffsets != null)
			{
				for (int i = 0; i < currentData.statOffsets.Count; i++)
				{
					if (currentData.statOffsets[i].stat == stat)
					{
						num += currentData.statOffsets[i].value;
					}
				}
			}
			return num;
		}

		public float MultiplierOfStat(StatDef stat)
		{
			float num = 1f;
			TraitDegreeData currentData = CurrentData;
			if (currentData.statFactors != null)
			{
				for (int i = 0; i < currentData.statFactors.Count; i++)
				{
					if (currentData.statFactors[i].stat == stat)
					{
						num *= currentData.statFactors[i].value;
					}
				}
			}
			return num;
		}

		public string TipString(Pawn pawn)
		{
			StringBuilder stringBuilder = new StringBuilder();
			TraitDegreeData currentData = CurrentData;
			stringBuilder.Append(currentData.description.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn).Resolve());
			bool num = CurrentData.skillGains.Count > 0;
			bool flag = GetPermaThoughts().Any();
			bool flag2 = currentData.statOffsets != null;
			bool flag3 = currentData.statFactors != null;
			if (num || flag || flag2 || flag3)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
			}
			if (num)
			{
				foreach (KeyValuePair<SkillDef, int> skillGain in CurrentData.skillGains)
				{
					if (skillGain.Value != 0)
					{
						string value = "    " + skillGain.Key.skillLabel.CapitalizeFirst() + ":   " + skillGain.Value.ToString("+##;-##");
						stringBuilder.AppendLine(value);
					}
				}
			}
			if (flag)
			{
				foreach (ThoughtDef permaThought in GetPermaThoughts())
				{
					stringBuilder.AppendLine("    " + "PermanentMoodEffect".Translate() + " " + permaThought.stages[0].baseMoodEffect.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Offset));
				}
			}
			if (flag2)
			{
				for (int i = 0; i < currentData.statOffsets.Count; i++)
				{
					StatModifier statModifier = currentData.statOffsets[i];
					string valueToStringAsOffset = statModifier.ValueToStringAsOffset;
					string value2 = "    " + statModifier.stat.LabelCap + " " + valueToStringAsOffset;
					stringBuilder.AppendLine(value2);
				}
			}
			if (flag3)
			{
				for (int j = 0; j < currentData.statFactors.Count; j++)
				{
					StatModifier statModifier2 = currentData.statFactors[j];
					string toStringAsFactor = statModifier2.ToStringAsFactor;
					string value3 = "    " + statModifier2.stat.LabelCap + " " + toStringAsFactor;
					stringBuilder.AppendLine(value3);
				}
			}
			if (currentData.hungerRateFactor != 1f)
			{
				string t = currentData.hungerRateFactor.ToStringByStyle(ToStringStyle.PercentOne, ToStringNumberSense.Factor);
				string value4 = "    " + "HungerRate".Translate() + " " + t;
				stringBuilder.AppendLine(value4);
			}
			if (ModsConfig.RoyaltyActive)
			{
				List<MeditationFocusDef> allowedMeditationFocusTypes = CurrentData.allowedMeditationFocusTypes;
				if (!allowedMeditationFocusTypes.NullOrEmpty())
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("EnablesMeditationFocusType".Translate() + ":\n" + allowedMeditationFocusTypes.Select((MeditationFocusDef f) => f.LabelCap.RawText).ToLineList("  - "));
				}
			}
			if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] == '\n')
			{
				if (stringBuilder.Length > 1 && stringBuilder[stringBuilder.Length - 2] == '\r')
				{
					stringBuilder.Remove(stringBuilder.Length - 2, 2);
				}
				else
				{
					stringBuilder.Remove(stringBuilder.Length - 1, 1);
				}
			}
			return stringBuilder.ToString();
		}

		public override string ToString()
		{
			return "Trait(" + def.ToString() + "-" + degree + ")";
		}

		private IEnumerable<ThoughtDef> GetPermaThoughts()
		{
			TraitDegreeData degree = CurrentData;
			List<ThoughtDef> allThoughts = DefDatabase<ThoughtDef>.AllDefsListForReading;
			for (int i = 0; i < allThoughts.Count; i++)
			{
				if (allThoughts[i].IsSituational && allThoughts[i].Worker is ThoughtWorker_AlwaysActive && allThoughts[i].requiredTraits != null && allThoughts[i].requiredTraits.Contains(def) && (!allThoughts[i].RequiresSpecificTraitsDegree || allThoughts[i].requiredTraitsDegree == degree.degree))
				{
					yield return allThoughts[i];
				}
			}
		}

		private bool AllowsWorkType(WorkTypeDef workDef)
		{
			return (def.disabledWorkTags & workDef.workTags) == 0;
		}

		public void Notify_MentalStateEndedOn(Pawn pawn, bool causedByMood)
		{
			if (causedByMood)
			{
				Notify_MentalStateEndedOn(pawn);
			}
		}

		public void Notify_MentalStateEndedOn(Pawn pawn)
		{
			TraitDegreeData currentData = CurrentData;
			if (!currentData.mentalBreakInspirationGainSet.NullOrEmpty() && !(Rand.Value > currentData.mentalBreakInspirationGainChance))
			{
				pawn.mindState.inspirationHandler.TryStartInspiration_NewTemp(currentData.mentalBreakInspirationGainSet.RandomElement(), currentData.mentalBreakInspirationGainReasonText);
			}
		}

		public IEnumerable<WorkTypeDef> GetDisabledWorkTypes()
		{
			for (int j = 0; j < def.disabledWorkTypes.Count; j++)
			{
				yield return def.disabledWorkTypes[j];
			}
			List<WorkTypeDef> workTypeDefList = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int j = 0; j < workTypeDefList.Count; j++)
			{
				WorkTypeDef workTypeDef = workTypeDefList[j];
				if (!AllowsWorkType(workTypeDef))
				{
					yield return workTypeDef;
				}
			}
		}
	}
}
