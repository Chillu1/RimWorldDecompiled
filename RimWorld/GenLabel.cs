using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class GenLabel
	{
		private class LabelElement
		{
			public Thing thingTemplate;

			public int count;
		}

		private struct LabelRequest : IEquatable<LabelRequest>
		{
			public BuildableDef entDef;

			public ThingDef stuffDef;

			public int stackCount;

			public QualityCategory quality;

			public int health;

			public int maxHealth;

			public bool wornByCorpse;

			public static bool operator ==(LabelRequest lhs, LabelRequest rhs)
			{
				return lhs.Equals(rhs);
			}

			public static bool operator !=(LabelRequest lhs, LabelRequest rhs)
			{
				return !(lhs == rhs);
			}

			public override bool Equals(object obj)
			{
				if (!(obj is LabelRequest))
				{
					return false;
				}
				return Equals((LabelRequest)obj);
			}

			public bool Equals(LabelRequest other)
			{
				if (entDef == other.entDef && stuffDef == other.stuffDef && stackCount == other.stackCount && quality == other.quality && health == other.health && maxHealth == other.maxHealth)
				{
					return wornByCorpse == other.wornByCorpse;
				}
				return false;
			}

			public override int GetHashCode()
			{
				int seed = 0;
				seed = Gen.HashCombine(seed, entDef);
				seed = Gen.HashCombine(seed, stuffDef);
				ThingDef thingDef = entDef as ThingDef;
				if (thingDef != null)
				{
					seed = Gen.HashCombineInt(seed, stackCount);
					seed = Gen.HashCombineStruct(seed, quality);
					if (thingDef.useHitPoints)
					{
						seed = Gen.HashCombineInt(seed, health);
						seed = Gen.HashCombineInt(seed, maxHealth);
					}
					seed = Gen.HashCombineInt(seed, wornByCorpse ? 1 : 0);
				}
				return seed;
			}
		}

		private static Dictionary<LabelRequest, string> labelDictionary = new Dictionary<LabelRequest, string>();

		private const int LabelDictionaryMaxCount = 2000;

		private static List<ThingCount> tmpThingCounts = new List<ThingCount>();

		private static List<LabelElement> tmpThingsLabelElements = new List<LabelElement>();

		private static List<Pawn> tmpHumanlikes = new List<Pawn>();

		private static Dictionary<string, int> tmpHumanlikeLabels = new Dictionary<string, int>();

		private static Dictionary<string, int> tmpLabels = new Dictionary<string, int>();

		public static void ClearCache()
		{
			labelDictionary.Clear();
		}

		public static string ThingLabel(BuildableDef entDef, ThingDef stuffDef, int stackCount = 1)
		{
			LabelRequest key = default(LabelRequest);
			key.entDef = entDef;
			key.stuffDef = stuffDef;
			key.stackCount = stackCount;
			if (!labelDictionary.TryGetValue(key, out string value))
			{
				if (labelDictionary.Count > 2000)
				{
					labelDictionary.Clear();
				}
				value = NewThingLabel(entDef, stuffDef, stackCount);
				labelDictionary.Add(key, value);
			}
			return value;
		}

		private static string NewThingLabel(BuildableDef entDef, ThingDef stuffDef, int stackCount)
		{
			string text = (stuffDef != null) ? ((string)"ThingMadeOfStuffLabel".Translate(stuffDef.LabelAsStuff, entDef.label)) : entDef.label;
			if (stackCount > 1)
			{
				text = text + " x" + stackCount.ToStringCached();
			}
			return text;
		}

		public static string ThingLabel(Thing t, int stackCount, bool includeHp = true)
		{
			LabelRequest key = default(LabelRequest);
			key.entDef = t.def;
			key.stuffDef = t.Stuff;
			key.stackCount = stackCount;
			t.TryGetQuality(out key.quality);
			if (t.def.useHitPoints && includeHp)
			{
				key.health = t.HitPoints;
				key.maxHealth = t.MaxHitPoints;
			}
			Apparel apparel = t as Apparel;
			if (apparel != null)
			{
				key.wornByCorpse = apparel.WornByCorpse;
			}
			if (!labelDictionary.TryGetValue(key, out string value))
			{
				if (labelDictionary.Count > 2000)
				{
					labelDictionary.Clear();
				}
				value = NewThingLabel(t, stackCount, includeHp);
				labelDictionary.Add(key, value);
			}
			return value;
		}

		private static string NewThingLabel(Thing t, int stackCount, bool includeHp)
		{
			string text = ThingLabel(t.def, t.Stuff);
			QualityCategory qc;
			bool flag = t.TryGetQuality(out qc);
			int hitPoints = t.HitPoints;
			int maxHitPoints = t.MaxHitPoints;
			bool flag2 = (t.def.useHitPoints && hitPoints < maxHitPoints && t.def.stackLimit == 1) & includeHp;
			bool flag3 = (t as Apparel)?.WornByCorpse ?? false;
			if (flag | flag2 | flag3)
			{
				text += " (";
				if (flag)
				{
					text += qc.GetLabel();
				}
				if (flag2)
				{
					if (flag)
					{
						text += " ";
					}
					text += ((float)hitPoints / (float)maxHitPoints).ToStringPercent();
				}
				if (flag3)
				{
					if (flag | flag2)
					{
						text += " ";
					}
					text += "WornByCorpseChar".Translate();
				}
				text += ")";
			}
			if (stackCount > 1)
			{
				text = text + " x" + stackCount.ToStringCached();
			}
			return text;
		}

		public static string ThingsLabel(IEnumerable<Thing> things, string prefix = "  - ")
		{
			tmpThingCounts.Clear();
			IList<Thing> list = things as IList<Thing>;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					tmpThingCounts.Add(new ThingCount(list[i], list[i].stackCount));
				}
			}
			else
			{
				foreach (Thing thing in things)
				{
					tmpThingCounts.Add(new ThingCount(thing, thing.stackCount));
				}
			}
			string result = ThingsLabel(tmpThingCounts, prefix);
			tmpThingCounts.Clear();
			return result;
		}

		public static string ThingsLabel(List<ThingCount> things, string prefix = "  - ", bool ignoreStackLimit = false)
		{
			tmpThingsLabelElements.Clear();
			foreach (ThingCount thing in things)
			{
				LabelElement labelElement = tmpThingsLabelElements.Where((LabelElement elem) => ((thing.Thing.def.stackLimit > 1) | ignoreStackLimit) && elem.thingTemplate.def == thing.Thing.def && elem.thingTemplate.Stuff == thing.Thing.Stuff).FirstOrDefault();
				if (labelElement != null)
				{
					labelElement.count += thing.Count;
				}
				else
				{
					tmpThingsLabelElements.Add(new LabelElement
					{
						thingTemplate = thing.Thing,
						count = thing.Count
					});
				}
			}
			tmpThingsLabelElements.Sort(delegate(LabelElement lhs, LabelElement rhs)
			{
				int num = TransferableComparer_Category.Compare(lhs.thingTemplate.def, rhs.thingTemplate.def);
				return (num != 0) ? num : lhs.thingTemplate.MarketValue.CompareTo(rhs.thingTemplate.MarketValue);
			});
			StringBuilder stringBuilder = new StringBuilder();
			foreach (LabelElement tmpThingsLabelElement in tmpThingsLabelElements)
			{
				string str = "";
				if (tmpThingsLabelElement.thingTemplate.ParentHolder is Pawn_ApparelTracker)
				{
					str = " (" + "WornBy".Translate(((Pawn)tmpThingsLabelElement.thingTemplate.ParentHolder.ParentHolder).LabelShort, (Pawn)tmpThingsLabelElement.thingTemplate.ParentHolder.ParentHolder) + ")";
				}
				else if (tmpThingsLabelElement.thingTemplate.ParentHolder is Pawn_EquipmentTracker)
				{
					str = " (" + "EquippedBy".Translate(((Pawn)tmpThingsLabelElement.thingTemplate.ParentHolder.ParentHolder).LabelShort, (Pawn)tmpThingsLabelElement.thingTemplate.ParentHolder.ParentHolder) + ")";
				}
				if (tmpThingsLabelElement.count == 1)
				{
					stringBuilder.AppendLine(prefix + tmpThingsLabelElement.thingTemplate.LabelCap + str);
				}
				else
				{
					stringBuilder.AppendLine(prefix + ThingLabel(tmpThingsLabelElement.thingTemplate.def, tmpThingsLabelElement.thingTemplate.Stuff, tmpThingsLabelElement.count).CapitalizeFirst() + str);
				}
			}
			tmpThingsLabelElements.Clear();
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public static string BestKindLabel(Pawn pawn, bool mustNoteGender = false, bool mustNoteLifeStage = false, bool plural = false, int pluralCount = -1)
		{
			if (plural && pluralCount == 1)
			{
				plural = false;
			}
			bool genderNoted = false;
			bool flag = false;
			string text = null;
			switch (pawn.gender)
			{
			case Gender.None:
				if (plural && !pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelPlural != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelPlural;
					flag = true;
				}
				else if (!pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.label != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.label;
					flag = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text, pawn.gender, pluralCount);
					}
				}
				else
				{
					text = BestKindLabel(pawn.kindDef, Gender.None, out genderNoted, plural, pluralCount);
				}
				break;
			case Gender.Male:
				if (plural && !pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelMalePlural != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelMalePlural;
					flag = true;
					genderNoted = true;
				}
				else if (!pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelMale != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelMale;
					flag = true;
					genderNoted = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text, pawn.gender, pluralCount);
					}
				}
				else if (plural && !pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelPlural != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelPlural;
					flag = true;
				}
				else if (!pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.label != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.label;
					flag = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text, pawn.gender, pluralCount);
					}
				}
				else
				{
					text = BestKindLabel(pawn.kindDef, Gender.Male, out genderNoted, plural, pluralCount);
				}
				break;
			case Gender.Female:
				if (plural && !pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelFemalePlural != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelFemalePlural;
					flag = true;
					genderNoted = true;
				}
				else if (!pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelFemale != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelFemale;
					flag = true;
					genderNoted = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text, pawn.gender, pluralCount);
					}
				}
				else if (plural && !pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.labelPlural != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.labelPlural;
					flag = true;
				}
				else if (!pawn.RaceProps.Humanlike && pawn.ageTracker.CurKindLifeStage.label != null)
				{
					text = pawn.ageTracker.CurKindLifeStage.label;
					flag = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text, pawn.gender, pluralCount);
					}
				}
				else
				{
					text = BestKindLabel(pawn.kindDef, Gender.Female, out genderNoted, plural, pluralCount);
				}
				break;
			}
			if (mustNoteGender && !genderNoted && pawn.gender != 0)
			{
				text = "PawnMainDescGendered".Translate(pawn.GetGenderLabel(), text, pawn.Named("PAWN"));
			}
			if (mustNoteLifeStage && !flag && pawn.ageTracker != null && pawn.ageTracker.CurLifeStage.visible)
			{
				text = "PawnMainDescLifestageWrap".Translate(text, pawn.ageTracker.CurLifeStage.Adjective, pawn);
			}
			return text;
		}

		public static string BestKindLabel(PawnKindDef kindDef, Gender gender, bool plural = false, int pluralCount = -1)
		{
			bool genderNoted;
			return BestKindLabel(kindDef, gender, out genderNoted, plural, pluralCount);
		}

		public static string BestKindLabel(PawnKindDef kindDef, Gender gender, out bool genderNoted, bool plural = false, int pluralCount = -1)
		{
			if (plural && pluralCount == 1)
			{
				plural = false;
			}
			string text = null;
			genderNoted = false;
			switch (gender)
			{
			case Gender.None:
				if (plural && kindDef.labelPlural != null)
				{
					text = kindDef.labelPlural;
					break;
				}
				text = kindDef.label;
				if (plural)
				{
					text = Find.ActiveLanguageWorker.Pluralize(text, gender, pluralCount);
				}
				break;
			case Gender.Male:
				if (plural && kindDef.labelMalePlural != null)
				{
					text = kindDef.labelMalePlural;
					genderNoted = true;
				}
				else if (kindDef.labelMale != null)
				{
					text = kindDef.labelMale;
					genderNoted = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text, gender, pluralCount);
					}
				}
				else if (plural && kindDef.labelPlural != null)
				{
					text = kindDef.labelPlural;
				}
				else
				{
					text = kindDef.label;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text, gender, pluralCount);
					}
				}
				break;
			case Gender.Female:
				if (plural && kindDef.labelFemalePlural != null)
				{
					text = kindDef.labelFemalePlural;
					genderNoted = true;
				}
				else if (kindDef.labelFemale != null)
				{
					text = kindDef.labelFemale;
					genderNoted = true;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text, gender, pluralCount);
					}
				}
				else if (plural && kindDef.labelPlural != null)
				{
					text = kindDef.labelPlural;
				}
				else
				{
					text = kindDef.label;
					if (plural)
					{
						text = Find.ActiveLanguageWorker.Pluralize(text, gender, pluralCount);
					}
				}
				break;
			}
			return text;
		}

		public static string BestGroupLabel(List<Pawn> pawns, bool definite, out Pawn singlePawn)
		{
			singlePawn = null;
			if (!pawns.Any())
			{
				return "";
			}
			if (pawns.Count == 1)
			{
				singlePawn = pawns[0];
				if (!definite)
				{
					return pawns[0].LabelShort;
				}
				return pawns[0].LabelDefinite();
			}
			tmpHumanlikes.Clear();
			for (int i = 0; i < pawns.Count; i++)
			{
				if (!pawns[i].AnimalOrWildMan())
				{
					tmpHumanlikes.Add(pawns[i]);
				}
			}
			if (tmpHumanlikes.Any())
			{
				if (tmpHumanlikes.Count == 1)
				{
					singlePawn = tmpHumanlikes[0];
					if (!definite)
					{
						return tmpHumanlikes[0].LabelShort;
					}
					return tmpHumanlikes[0].LabelDefinite();
				}
				tmpHumanlikeLabels.Clear();
				for (int j = 0; j < tmpHumanlikes.Count; j++)
				{
					if (tmpHumanlikes[j].Faction != null)
					{
						string key = (!definite) ? tmpHumanlikes[j].Faction.def.pawnsPlural : Find.ActiveLanguageWorker.WithDefiniteArticle(tmpHumanlikes[j].Faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(tmpHumanlikes[j].Faction.def.pawnsPlural, tmpHumanlikes[j].Faction.def.pawnSingular), plural: true);
						if (!tmpHumanlikeLabels.ContainsKey(key))
						{
							tmpHumanlikeLabels.Add(key, 1);
						}
						else
						{
							tmpHumanlikeLabels[key]++;
						}
					}
					else
					{
						string key2 = (!definite) ? tmpHumanlikes[j].kindDef.GetLabelPlural() : Find.ActiveLanguageWorker.WithDefiniteArticle(tmpHumanlikes[j].kindDef.GetLabelPlural(), LanguageDatabase.activeLanguage.ResolveGender(tmpHumanlikes[j].kindDef.GetLabelPlural(), tmpHumanlikes[j].kindDef.label), plural: true);
						if (!tmpHumanlikeLabels.ContainsKey(key2))
						{
							tmpHumanlikeLabels.Add(key2, 1);
						}
						else
						{
							tmpHumanlikeLabels[key2]++;
						}
					}
				}
				int num = -1;
				string result = null;
				foreach (KeyValuePair<string, int> tmpHumanlikeLabel in tmpHumanlikeLabels)
				{
					if (tmpHumanlikeLabel.Value > num)
					{
						num = tmpHumanlikeLabel.Value;
						result = tmpHumanlikeLabel.Key;
					}
				}
				tmpHumanlikeLabels.Clear();
				tmpHumanlikes.Clear();
				return result;
			}
			tmpLabels.Clear();
			for (int k = 0; k < pawns.Count; k++)
			{
				string key3 = (!definite) ? pawns[k].kindDef.GetLabelPlural() : Find.ActiveLanguageWorker.WithDefiniteArticle(pawns[k].kindDef.GetLabelPlural(), LanguageDatabase.activeLanguage.ResolveGender(pawns[k].kindDef.GetLabelPlural(), pawns[k].kindDef.label), plural: true);
				if (!tmpLabels.ContainsKey(key3))
				{
					tmpLabels.Add(key3, 1);
				}
				else
				{
					tmpLabels[key3]++;
				}
			}
			int num2 = -1;
			string result2 = null;
			foreach (KeyValuePair<string, int> tmpLabel in tmpLabels)
			{
				if (tmpLabel.Value > num2)
				{
					num2 = tmpLabel.Value;
					result2 = tmpLabel.Key;
				}
			}
			tmpLabels.Clear();
			tmpHumanlikes.Clear();
			if ((float)num2 / (float)pawns.Count >= 0.5f)
			{
				return result2;
			}
			if (definite)
			{
				return Find.ActiveLanguageWorker.WithDefiniteArticle("AnimalsLower".Translate(), plural: true);
			}
			return "AnimalsLower".Translate();
		}
	}
}
