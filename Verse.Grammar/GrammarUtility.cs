using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Grammar
{
	public static class GrammarUtility
	{
		public static IEnumerable<Rule> RulesForPawn(string pawnSymbol, Pawn pawn, Dictionary<string, string> constants = null, bool addRelationInfoSymbol = true, bool addTags = true)
		{
			if (pawn == null)
			{
				Log.ErrorOnce($"Tried to insert rule {pawnSymbol} for null pawn", 16015097);
				return Enumerable.Empty<Rule>();
			}
			TaggedString text = "";
			if (addRelationInfoSymbol)
			{
				PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);
			}
			return RulesForPawn(pawnSymbol, pawn.Name, (pawn.story != null) ? pawn.story.Title : null, pawn.kindDef, pawn.gender, pawn.Faction, pawn.ageTracker.AgeBiologicalYears, pawn.ageTracker.AgeChronologicalYears, text, PawnUtility.EverBeenColonistOrTameAnimal(pawn), PawnUtility.EverBeenQuestLodger(pawn), pawn.Faction != null && pawn.Faction.leader == pawn, (pawn.royalty != null) ? pawn.royalty.AllTitlesForReading : null, constants, addTags);
		}

		public static IEnumerable<Rule> RulesForPawn(string pawnSymbol, Name name, string title, PawnKindDef kind, Gender gender, Faction faction, int age, int chronologicalAge, string relationInfo, bool everBeenColonistOrTameAnimal, bool everBeenQuestLodger, bool isFactionLeader, List<RoyalTitle> royalTitles, Dictionary<string, string> constants = null, bool addTags = true)
		{
			string prefix = "";
			if (!pawnSymbol.NullOrEmpty())
			{
				prefix = prefix + pawnSymbol + "_";
			}
			yield return new Rule_String(output: ((name == null) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(kind.label, gender) : Find.ActiveLanguageWorker.WithIndefiniteArticle(name.ToStringFull, gender, plural: false, name: true)).ApplyTag(TagType.Name).Resolve(), keyword: prefix + "nameFull");
			string nameShort = (name == null) ? kind.label : name.ToStringShort;
			yield return new Rule_String(prefix + "label", addTags ? nameShort.ApplyTag(TagType.Name).Resolve() : nameShort);
			string nameShortDef2 = (name == null) ? Find.ActiveLanguageWorker.WithDefiniteArticle(kind.label, gender) : Find.ActiveLanguageWorker.WithDefiniteArticle(name.ToStringShort, gender, plural: false, name: true);
			yield return new Rule_String(prefix + "definite", addTags ? nameShortDef2.ApplyTag(TagType.Name).Resolve() : nameShortDef2);
			yield return new Rule_String(prefix + "nameDef", addTags ? nameShortDef2.ApplyTag(TagType.Name).Resolve() : nameShortDef2);
			nameShortDef2 = ((name == null) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(kind.label, gender) : Find.ActiveLanguageWorker.WithIndefiniteArticle(name.ToStringShort, gender, plural: false, name: true));
			yield return new Rule_String(prefix + "indefinite", addTags ? nameShortDef2.ApplyTag(TagType.Name).Resolve() : nameShortDef2);
			yield return new Rule_String(prefix + "nameIndef", addTags ? nameShortDef2.ApplyTag(TagType.Name).Resolve() : nameShortDef2);
			yield return new Rule_String(prefix + "pronoun", gender.GetPronoun());
			yield return new Rule_String(prefix + "possessive", gender.GetPossessive());
			yield return new Rule_String(prefix + "objective", gender.GetObjective());
			if (faction != null)
			{
				yield return new Rule_String(prefix + "factionName", addTags ? faction.Name.ApplyTag(faction).Resolve() : faction.Name);
				if (constants != null)
				{
					constants[prefix + "faction"] = faction.def.defName;
				}
			}
			if (constants != null && isFactionLeader)
			{
				constants[prefix + "factionLeader"] = "True";
			}
			if (kind != null)
			{
				yield return new Rule_String(prefix + "kind", GenLabel.BestKindLabel(kind, gender));
			}
			if (title != null)
			{
				yield return new Rule_String(prefix + "title", title);
				yield return new Rule_String(prefix + "titleIndef", Find.ActiveLanguageWorker.WithIndefiniteArticle(title, gender));
				yield return new Rule_String(prefix + "titleDef", Find.ActiveLanguageWorker.WithDefiniteArticle(title, gender));
			}
			if (royalTitles != null)
			{
				int royalTitleIndex = 0;
				RoyalTitle bestTitle = null;
				foreach (RoyalTitle royalTitle in royalTitles.OrderBy((RoyalTitle x) => x.def.index))
				{
					yield return new Rule_String(prefix + "royalTitle" + royalTitleIndex, royalTitle.def.GetLabelFor(gender));
					yield return new Rule_String(prefix + "royalTitle" + royalTitleIndex + "Indef", Find.ActiveLanguageWorker.WithIndefiniteArticle(royalTitle.def.GetLabelFor(gender)));
					yield return new Rule_String(prefix + "royalTitle" + royalTitleIndex + "Def", Find.ActiveLanguageWorker.WithDefiniteArticle(royalTitle.def.GetLabelFor(gender)));
					yield return new Rule_String(prefix + "royalTitleFaction" + royalTitleIndex, royalTitle.faction.Name.ApplyTag(royalTitle.faction).Resolve());
					if (royalTitle.faction == faction)
					{
						yield return new Rule_String(prefix + "royalTitleInCurrentFaction", royalTitle.def.GetLabelFor(gender));
						yield return new Rule_String(prefix + "royalTitleInCurrentFactionIndef", Find.ActiveLanguageWorker.WithIndefiniteArticle(royalTitle.def.GetLabelFor(gender)));
						yield return new Rule_String(prefix + "royalTitleInCurrentFactionDef", Find.ActiveLanguageWorker.WithDefiniteArticle(royalTitle.def.GetLabelFor(gender)));
						if (constants != null)
						{
							constants[prefix + "royalInCurrentFaction"] = "True";
						}
					}
					if (bestTitle == null || royalTitle.def.favorCost > bestTitle.def.favorCost)
					{
						bestTitle = royalTitle;
					}
					royalTitleIndex++;
				}
				if (bestTitle != null)
				{
					yield return new Rule_String(prefix + "bestRoyalTitle", bestTitle.def.GetLabelFor(gender));
					yield return new Rule_String(prefix + "bestRoyalTitleIndef", Find.ActiveLanguageWorker.WithIndefiniteArticle(bestTitle.def.GetLabelFor(gender)));
					yield return new Rule_String(prefix + "bestRoyalTitleDef", Find.ActiveLanguageWorker.WithDefiniteArticle(bestTitle.def.GetLabelFor(gender)));
					yield return new Rule_String(prefix + "bestRoyalTitleFaction", bestTitle.faction.Name);
				}
			}
			yield return new Rule_String(prefix + "age", age.ToString());
			yield return new Rule_String(prefix + "chronologicalAge", chronologicalAge.ToString());
			if (everBeenColonistOrTameAnimal)
			{
				yield return new Rule_String("formerlyColonistInfo", "PawnWasFormerlyColonist".Translate(nameShort));
				if (constants != null)
				{
					constants[prefix + "formerlyColonist"] = "True";
				}
			}
			else if (everBeenQuestLodger)
			{
				yield return new Rule_String("formerlyColonistInfo", "PawnWasFormerlyLodger".Translate(nameShort));
				if (constants != null)
				{
					constants[prefix + "formerlyColonist"] = "True";
				}
			}
			yield return new Rule_String(prefix + "relationInfo", relationInfo);
			if (constants != null && kind != null)
			{
				constants[prefix + "flesh"] = kind.race.race.FleshType.defName;
			}
			if (constants != null)
			{
				constants[prefix + "gender"] = gender.ToString();
			}
		}

		public static IEnumerable<Rule> RulesForDef(string prefix, Def def)
		{
			if (def == null)
			{
				Log.ErrorOnce($"Tried to insert rule {prefix} for null def", 79641686);
				yield break;
			}
			if (!prefix.NullOrEmpty())
			{
				prefix += "_";
			}
			yield return new Rule_String(prefix + "label", def.label);
			if (def is PawnKindDef)
			{
				yield return new Rule_String(prefix + "labelPlural", ((PawnKindDef)def).GetLabelPlural());
			}
			else
			{
				yield return new Rule_String(prefix + "labelPlural", Find.ActiveLanguageWorker.Pluralize(def.label));
			}
			yield return new Rule_String(prefix + "description", def.description);
			yield return new Rule_String(prefix + "definite", Find.ActiveLanguageWorker.WithDefiniteArticle(def.label));
			yield return new Rule_String(prefix + "indefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(def.label));
			yield return new Rule_String(prefix + "possessive", "Proits".Translate());
		}

		public static IEnumerable<Rule> RulesForBodyPartRecord(string prefix, BodyPartRecord part)
		{
			if (part == null)
			{
				Log.ErrorOnce($"Tried to insert rule {prefix} for null body part", 394876778);
				yield break;
			}
			if (!prefix.NullOrEmpty())
			{
				prefix += "_";
			}
			yield return new Rule_String(prefix + "label", part.Label);
			yield return new Rule_String(prefix + "definite", Find.ActiveLanguageWorker.WithDefiniteArticle(part.Label));
			yield return new Rule_String(prefix + "indefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(part.Label));
			yield return new Rule_String(prefix + "possessive", "Proits".Translate());
		}

		public static IEnumerable<Rule> RulesForHediffDef(string prefix, HediffDef def, BodyPartRecord part)
		{
			foreach (Rule item in RulesForDef(prefix, def))
			{
				yield return item;
			}
			if (!prefix.NullOrEmpty())
			{
				prefix += "_";
			}
			yield return new Rule_String(prefix + "label", def.label);
			string output = (!def.labelNoun.NullOrEmpty()) ? def.labelNoun : def.label;
			yield return new Rule_String(prefix + "labelNoun", output);
			string text = def.PrettyTextForPart(part);
			if (!text.NullOrEmpty())
			{
				yield return new Rule_String(prefix + "labelNounPretty", text);
			}
		}

		public static IEnumerable<Rule> RulesForFaction(string prefix, Faction faction, bool addTags = true)
		{
			if (!prefix.NullOrEmpty())
			{
				prefix += "_";
			}
			if (faction == null)
			{
				yield return new Rule_String(prefix + "name", "FactionUnaffiliated".Translate());
				yield break;
			}
			yield return new Rule_String(prefix + "name", addTags ? faction.Name.ApplyTag(faction).Resolve() : faction.Name);
			yield return new Rule_String(prefix + "pawnsPlural", faction.def.pawnsPlural);
			yield return new Rule_String(prefix + "pawnsPluralDef", Find.ActiveLanguageWorker.WithDefiniteArticle(faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(faction.def.pawnsPlural, faction.def.pawnSingular), plural: true));
			yield return new Rule_String(prefix + "pawnsPluralIndef", Find.ActiveLanguageWorker.WithIndefiniteArticle(faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(faction.def.pawnsPlural, faction.def.pawnSingular), plural: true));
			yield return new Rule_String(prefix + "leaderTitle", faction.LeaderTitle);
			yield return new Rule_String(prefix + "royalFavorLabel", faction.def.royalFavorLabel);
		}

		public static IEnumerable<Rule> RulesForWorldObject(string prefix, WorldObject worldObject, bool addTags = true)
		{
			if (!prefix.NullOrEmpty())
			{
				prefix += "_";
			}
			yield return new Rule_String(prefix + "label", PossiblyWithTag(worldObject.Label));
			yield return new Rule_String(prefix + "definite", PossiblyWithTag(Find.ActiveLanguageWorker.WithDefiniteArticle(worldObject.Label, plural: false, worldObject.HasName)));
			yield return new Rule_String(prefix + "indefinite", PossiblyWithTag(Find.ActiveLanguageWorker.WithIndefiniteArticle(worldObject.Label, plural: false, worldObject.HasName)));
			string PossiblyWithTag(string str)
			{
				if (!((worldObject.Faction != null) & addTags))
				{
					return str;
				}
				return str.ApplyTag(TagType.Settlement, worldObject.Faction.GetUniqueLoadID()).Resolve();
			}
		}
	}
}
