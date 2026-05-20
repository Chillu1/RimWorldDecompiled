using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;

namespace Verse.Grammar;

public static class GrammarUtility
{
	public static IEnumerable<Rule> RulesForPawn(string pawnSymbol, Pawn pawn, Dictionary<string, string> constants = null, bool addRelationInfoSymbol = true, bool addTags = true)
	{
		if (pawn == null)
		{
			Log.ErrorOnce("Tried to insert rule " + pawnSymbol + " for null pawn", 16015097);
			return Enumerable.Empty<Rule>();
		}
		TaggedString text = "";
		if (addRelationInfoSymbol)
		{
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);
		}
		return RulesForPawn(pawnSymbol, pawn.Name, pawn.story?.Title, pawn.kindDef, pawn.gender, pawn.Faction, pawn.ageTracker.AgeBiologicalYears, pawn.ageTracker.AgeChronologicalYears, text, PawnUtility.EverBeenColonistOrTameAnimal(pawn), PawnUtility.EverBeenQuestLodger(pawn), pawn.Faction != null && pawn.Faction.leader == pawn, pawn.royalty?.AllTitlesForReading, ModsConfig.AnomalyActive && pawn.health.hediffSet.HasHediff(HediffDefOf.CubeInterest), pawn.LabelNoParenthesis, constants, addTags);
	}

	public static IEnumerable<Rule> RulesForPawn(string pawnSymbol, Name name, string title, PawnKindDef kind, Gender gender, Faction faction, int age, int chronologicalAge, string relationInfo, bool everBeenColonistOrTameAnimal, bool everBeenQuestLodger, bool isFactionLeader, List<RoyalTitle> royalTitles, bool cubeInterest, string labelNoParenthesis, Dictionary<string, string> constants = null, bool addTags = true)
	{
		string prefix = "";
		if (!pawnSymbol.NullOrEmpty())
		{
			prefix = prefix + pawnSymbol + "_";
		}
		string kindLabel = gender switch
		{
			Gender.Female => kind.labelFemale.NullOrEmpty() ? kind.label : kind.labelFemale, 
			Gender.Male => kind.labelMale.NullOrEmpty() ? kind.label : kind.labelMale, 
			_ => kind.label, 
		};
		Gender kindGender = GrammarResolverSimple.ResolveGender(kindLabel, gender);
		string text = ((name == null) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(kindLabel, kindGender) : Find.ActiveLanguageWorker.WithIndefiniteArticle(name.ToStringFull, gender, plural: false, name: true));
		yield return new Rule_String(prefix + "nameFull", addTags ? text.ApplyTag(TagType.Name).Resolve() : text);
		string nameShort = ((name == null) ? kindLabel : name.ToStringShort);
		yield return new Rule_String(prefix + "label", addTags ? nameShort.ApplyTag(TagType.Name).Resolve() : nameShort);
		yield return new Rule_String(prefix + "labelNoParenthesis", addTags ? labelNoParenthesis.ApplyTag(TagType.Name).Resolve() : labelNoParenthesis);
		string nameShortDef = ((name == null) ? Find.ActiveLanguageWorker.WithDefiniteArticle(kindLabel, kindGender) : Find.ActiveLanguageWorker.WithDefiniteArticle(name.ToStringShort, gender, plural: false, name: true));
		yield return new Rule_String(prefix + "definite", addTags ? nameShortDef.ApplyTag(TagType.Name).Resolve() : nameShortDef);
		yield return new Rule_String(prefix + "nameDef", addTags ? nameShortDef.ApplyTag(TagType.Name).Resolve() : nameShortDef);
		nameShortDef = ((name == null) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(kindLabel, kindGender) : Find.ActiveLanguageWorker.WithIndefiniteArticle(name.ToStringShort, gender, plural: false, name: true));
		yield return new Rule_String(prefix + "indefinite", addTags ? nameShortDef.ApplyTag(TagType.Name).Resolve() : nameShortDef);
		yield return new Rule_String(prefix + "nameIndef", addTags ? nameShortDef.ApplyTag(TagType.Name).Resolve() : nameShortDef);
		yield return new Rule_String(prefix + "pronoun", ((name != null) ? gender : kindGender).GetPronoun());
		yield return new Rule_String(prefix + "possessive", ((name != null) ? gender : kindGender).GetPossessive());
		yield return new Rule_String(prefix + "objective", ((name != null) ? gender : kindGender).GetObjective());
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
			yield return new Rule_String(prefix + "kind", GenLabel.BestKindLabel(kind, kindGender));
			yield return new Rule_String(prefix + "kindPlural", GenLabel.BestKindLabel(kind, kindGender, plural: true));
		}
		if (title != null)
		{
			yield return new Rule_String(prefix + "title", title);
			Gender titleGender = LanguageDatabase.activeLanguage.ResolveGender(title, null, gender);
			yield return new Rule_String(prefix + "titleIndef", Find.ActiveLanguageWorker.WithIndefiniteArticle(title, titleGender));
			yield return new Rule_String(prefix + "titleDef", Find.ActiveLanguageWorker.WithDefiniteArticle(title, titleGender));
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
			constants[prefix + "genderResolved"] = ((name != null) ? gender : kindGender).ToString();
			constants[prefix + "cubeInterest"] = cubeInterest.ToString();
		}
	}

	public static IEnumerable<Rule> RulesForThing(string prefix, Thing thing)
	{
		prefix += "_";
		if (thing.TryGetQuality(out var qc))
		{
			yield return new Rule_String(prefix + "quality", qc.GetLabel().ToLower());
		}
	}

	public static IEnumerable<Rule> RulesForDef(string prefix, Def def)
	{
		if (def == null)
		{
			Log.ErrorOnce("Tried to insert rule " + prefix + " for null def", 79641686);
			yield break;
		}
		if (!prefix.NullOrEmpty())
		{
			prefix += "_";
		}
		yield return new Rule_String(prefix + "label", def.label);
		if (def is PawnKindDef pawnKindDef)
		{
			yield return new Rule_String(prefix + "labelPlural", pawnKindDef.GetLabelPlural());
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
			Log.ErrorOnce("Tried to insert rule " + prefix + " for null body part", 394876778);
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
		string output = ((!def.labelNoun.NullOrEmpty()) ? def.labelNoun : def.label);
		yield return new Rule_String(prefix + "labelNoun", output);
		string text = def.PrettyTextForPart(part);
		if (!text.NullOrEmpty())
		{
			yield return new Rule_String(prefix + "labelNounPretty", text);
		}
	}

	public static IEnumerable<Rule> RulesForFaction(string prefix, Faction faction, Dictionary<string, string> constants = null, bool addTags = true)
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
		yield return new Rule_String(prefix + "pawnSingular", faction.def.pawnSingular);
		yield return new Rule_String(prefix + "pawnSingularDef", Find.ActiveLanguageWorker.WithDefiniteArticle(faction.def.pawnSingular));
		yield return new Rule_String(prefix + "pawnSingularIndef", Find.ActiveLanguageWorker.WithIndefiniteArticle(faction.def.pawnSingular));
		yield return new Rule_String(prefix + "pawnsPlural", faction.def.pawnsPlural);
		yield return new Rule_String(prefix + "pawnsPluralDef", Find.ActiveLanguageWorker.WithDefiniteArticle(faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(faction.def.pawnsPlural, faction.def.pawnSingular), plural: true));
		yield return new Rule_String(prefix + "pawnsPluralIndef", Find.ActiveLanguageWorker.WithIndefiniteArticle(faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(faction.def.pawnsPlural, faction.def.pawnSingular), plural: true));
		yield return new Rule_String(prefix + "leaderTitle", faction.LeaderTitle);
		yield return new Rule_String(prefix + "royalFavorLabel", faction.def.royalFavorLabel);
		if (constants != null)
		{
			constants.Add(prefix + "temporary", faction.temporary.ToString());
			constants.Add(prefix + "hasLeader", (faction.leader != null) ? "True" : "False");
		}
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
			if (!(worldObject.Faction != null && addTags))
			{
				return str;
			}
			return str.ApplyTag(TagType.Settlement, worldObject.Faction.GetUniqueLoadID()).Resolve();
		}
	}

	public static IEnumerable<Rule> RulesForIdeo(string prefix, Ideo ideo)
	{
		if (ideo == null)
		{
			Log.ErrorOnce("Tried to insert rule " + prefix + " for null ideo", 453454453);
			yield break;
		}
		if (!prefix.NullOrEmpty())
		{
			prefix += "_";
		}
		yield return new Rule_String(prefix + "name", ideo.name.ApplyTag(ideo).Resolve());
		yield return new Rule_String(prefix + "memberName", ideo.memberName);
		yield return new Rule_String(prefix + "memberNamePlural", ideo.MemberNamePlural);
	}

	public static IEnumerable<Rule> RulesForPrecept(string prefix, Precept precept)
	{
		if (precept == null)
		{
			Log.ErrorOnce("Tried to insert rule " + prefix + " for null precet", 823453451);
			yield break;
		}
		if (!prefix.NullOrEmpty())
		{
			prefix += "_";
		}
		yield return new Rule_String(prefix + "name", precept.Label.ApplyTag(precept.ideo).Resolve());
	}
}
