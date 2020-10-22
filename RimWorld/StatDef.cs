using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StatDef : Def
	{
		public StatCategoryDef category;

		public Type workerClass = typeof(StatWorker);

		public string labelForFullStatList;

		public bool forInformationOnly;

		public float hideAtValue = -2.14748365E+09f;

		public bool alwaysHide;

		public bool showNonAbstract = true;

		public bool showIfUndefined = true;

		public bool showOnPawns = true;

		public bool showOnHumanlikes = true;

		public bool showOnNonWildManHumanlikes = true;

		public bool showOnAnimals = true;

		public bool showOnMechanoids = true;

		public bool showOnNonWorkTables = true;

		public bool showOnDefaultValue = true;

		public bool showOnUnhaulables = true;

		public bool showOnUntradeables = true;

		public List<string> showIfModsLoaded;

		public List<HediffDef> showIfHediffsPresent;

		public bool neverDisabled;

		public bool showZeroBaseValue;

		public int displayPriorityInCategory;

		public ToStringNumberSense toStringNumberSense = ToStringNumberSense.Absolute;

		public ToStringStyle toStringStyle;

		private ToStringStyle? toStringStyleUnfinalized;

		[MustTranslate]
		public string formatString;

		[MustTranslate]
		public string formatStringUnfinalized;

		public bool finalizeEquippedStatOffset = true;

		public float defaultBaseValue = 1f;

		public List<SkillNeed> skillNeedOffsets;

		public float noSkillOffset;

		public List<PawnCapacityOffset> capacityOffsets;

		public List<StatDef> statFactors;

		public bool applyFactorsIfNegative = true;

		public List<SkillNeed> skillNeedFactors;

		public float noSkillFactor = 1f;

		public List<PawnCapacityFactor> capacityFactors;

		public SimpleCurve postProcessCurve;

		public List<StatDef> postProcessStatFactors;

		public float minValue = -9999999f;

		public float maxValue = 9999999f;

		public float valueIfMissing;

		public bool roundValue;

		public float roundToFiveOver = float.MaxValue;

		public bool minifiedThingInherits;

		public bool supressDisabledError;

		public bool scenarioRandomizable;

		public List<StatPart> parts;

		[Unsaved(false)]
		private StatWorker workerInt;

		public StatWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					if (parts != null)
					{
						for (int i = 0; i < parts.Count; i++)
						{
							parts[i].parentStat = this;
						}
					}
					workerInt = (StatWorker)Activator.CreateInstance(workerClass);
					workerInt.InitSetStat(this);
				}
				return workerInt;
			}
		}

		public ToStringStyle ToStringStyleUnfinalized
		{
			get
			{
				if (!toStringStyleUnfinalized.HasValue)
				{
					return toStringStyle;
				}
				return toStringStyleUnfinalized.Value;
			}
		}

		public string LabelForFullStatList
		{
			get
			{
				if (!labelForFullStatList.NullOrEmpty())
				{
					return labelForFullStatList;
				}
				return label;
			}
		}

		public string LabelForFullStatListCap => LabelForFullStatList.CapitalizeFirst(this);

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (capacityFactors != null)
			{
				foreach (PawnCapacityFactor capacityFactor in capacityFactors)
				{
					if (capacityFactor.weight > 1f)
					{
						yield return defName + " has activity factor with weight > 1";
					}
				}
			}
			if (parts == null)
			{
				yield break;
			}
			for (int i = 0; i < parts.Count; i++)
			{
				foreach (string item2 in parts[i].ConfigErrors())
				{
					yield return defName + " has error in StatPart " + parts[i].ToString() + ": " + item2;
				}
			}
		}

		public string ValueToString(float val, ToStringNumberSense numberSense = ToStringNumberSense.Absolute, bool finalized = true)
		{
			return Worker.ValueToString(val, finalized, numberSense);
		}

		public static StatDef Named(string defName)
		{
			return DefDatabase<StatDef>.GetNamed(defName);
		}

		public override void PostLoad()
		{
			base.PostLoad();
			if (parts != null)
			{
				List<StatPart> partsCopy = parts.ToList();
				parts.SortBy((StatPart x) => 0f - x.priority, (StatPart x) => partsCopy.IndexOf(x));
			}
		}

		public T GetStatPart<T>() where T : StatPart
		{
			return parts.OfType<T>().FirstOrDefault();
		}

		public bool CanShowWithLoadedMods()
		{
			if (!showIfModsLoaded.NullOrEmpty())
			{
				for (int i = 0; i < showIfModsLoaded.Count; i++)
				{
					if (!ModsConfig.IsActive(showIfModsLoaded[i]))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
