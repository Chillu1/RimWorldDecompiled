using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class HediffDef : Def
	{
		public Type hediffClass = typeof(Hediff);

		public List<HediffCompProperties> comps;

		public float initialSeverity = 0.5f;

		public float lethalSeverity = -1f;

		public List<HediffStage> stages;

		public bool tendable;

		public bool isBad = true;

		public ThingDef spawnThingOnRemoved;

		public float chanceToCauseNoPain;

		public bool makesSickThought;

		public bool makesAlert = true;

		public NeedDef causesNeed;

		public NeedDef disablesNeed;

		public float minSeverity;

		public float maxSeverity = float.MaxValue;

		public bool scenarioCanAdd;

		public List<HediffGiver> hediffGivers;

		public bool cureAllAtOnceIfCuredByItem;

		public TaleDef taleOnVisible;

		public bool everCurableByItem = true;

		public string battleStateLabel;

		public string labelNounPretty;

		public List<string> tags;

		public bool priceImpact;

		public float priceOffset;

		public bool chronic;

		public SimpleCurve removeOnRedressChanceByDaysCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(1f, 0f)
		};

		public bool removeOnQuestLodgers;

		public bool displayWound;

		public Color defaultLabelColor = Color.white;

		public InjuryProps injuryProps;

		public AddedBodyPartProps addedPartProps;

		[MustTranslate]
		public string labelNoun;

		private bool alwaysAllowMothballCached;

		private bool alwaysAllowMothball;

		private Hediff concreteExampleInt;

		public bool IsAddiction => typeof(Hediff_Addiction).IsAssignableFrom(hediffClass);

		public bool AlwaysAllowMothball
		{
			get
			{
				if (!alwaysAllowMothballCached)
				{
					alwaysAllowMothball = true;
					if (comps != null && comps.Count > 0)
					{
						alwaysAllowMothball = false;
					}
					if (stages != null)
					{
						for (int i = 0; i < stages.Count; i++)
						{
							HediffStage hediffStage = stages[i];
							if (hediffStage.deathMtbDays > 0f || (hediffStage.hediffGivers != null && hediffStage.hediffGivers.Count > 0))
							{
								alwaysAllowMothball = false;
							}
						}
					}
					alwaysAllowMothballCached = true;
				}
				return alwaysAllowMothball;
			}
		}

		public Hediff ConcreteExample
		{
			get
			{
				if (concreteExampleInt == null)
				{
					concreteExampleInt = HediffMaker.Debug_MakeConcreteExampleHediff(this);
				}
				return concreteExampleInt;
			}
		}

		public bool HasComp(Type compClass)
		{
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (comps[i].compClass == compClass)
					{
						return true;
					}
				}
			}
			return false;
		}

		public HediffCompProperties CompPropsFor(Type compClass)
		{
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (comps[i].compClass == compClass)
					{
						return comps[i];
					}
				}
			}
			return null;
		}

		public T CompProps<T>() where T : HediffCompProperties
		{
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					T val = comps[i] as T;
					if (val != null)
					{
						return val;
					}
				}
			}
			return null;
		}

		public bool PossibleToDevelopImmunityNaturally()
		{
			HediffCompProperties_Immunizable hediffCompProperties_Immunizable = CompProps<HediffCompProperties_Immunizable>();
			if (hediffCompProperties_Immunizable != null && (hediffCompProperties_Immunizable.immunityPerDayNotSick > 0f || hediffCompProperties_Immunizable.immunityPerDaySick > 0f))
			{
				return true;
			}
			return false;
		}

		public string PrettyTextForPart(BodyPartRecord bodyPart)
		{
			if (labelNounPretty.NullOrEmpty())
			{
				return null;
			}
			return string.Format(labelNounPretty, label, bodyPart.Label);
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (hediffClass == null)
			{
				yield return "hediffClass is null";
			}
			if (!comps.NullOrEmpty() && !typeof(HediffWithComps).IsAssignableFrom(hediffClass))
			{
				yield return "has comps but hediffClass is not HediffWithComps or subclass thereof";
			}
			if (minSeverity > initialSeverity)
			{
				yield return "minSeverity is greater than initialSeverity";
			}
			if (maxSeverity < initialSeverity)
			{
				yield return "maxSeverity is lower than initialSeverity";
			}
			if (!tendable && HasComp(typeof(HediffComp_TendDuration)))
			{
				yield return "has HediffComp_TendDuration but tendable = false";
			}
			if (string.IsNullOrEmpty(description))
			{
				yield return "Hediff with defName " + defName + " has no description!";
			}
			if (comps != null)
			{
				for (int m = 0; m < comps.Count; m++)
				{
					foreach (string item2 in comps[m].ConfigErrors(this))
					{
						yield return comps[m] + ": " + item2;
					}
				}
			}
			if (stages != null)
			{
				if (!typeof(Hediff_Addiction).IsAssignableFrom(hediffClass))
				{
					for (int m = 0; m < stages.Count; m++)
					{
						if (m >= 1 && stages[m].minSeverity <= stages[m - 1].minSeverity)
						{
							yield return "stages are not in order of minSeverity";
						}
					}
				}
				for (int m = 0; m < stages.Count; m++)
				{
					if (stages[m].makeImmuneTo != null && !stages[m].makeImmuneTo.Any((HediffDef im) => im.HasComp(typeof(HediffComp_Immunizable))))
					{
						yield return "makes immune to hediff which doesn't have comp immunizable";
					}
					if (stages[m].hediffGivers != null)
					{
						for (int j = 0; j < stages[m].hediffGivers.Count; j++)
						{
							foreach (string item3 in stages[m].hediffGivers[j].ConfigErrors())
							{
								yield return item3;
							}
						}
					}
				}
			}
			if (hediffGivers != null)
			{
				for (int m = 0; m < hediffGivers.Count; m++)
				{
					foreach (string item4 in hediffGivers[m].ConfigErrors())
					{
						yield return item4;
					}
				}
			}
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			if (stages != null && stages.Count == 1)
			{
				foreach (StatDrawEntry item in stages[0].SpecialDisplayStats())
				{
					yield return item;
				}
			}
		}

		public static HediffDef Named(string defName)
		{
			return DefDatabase<HediffDef>.GetNamed(defName);
		}
	}
}
