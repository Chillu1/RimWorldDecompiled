using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public static class GrammarResolverSimple
{
	private static bool formatterWorking;

	private static bool symbolParserWorking;

	private static StringBuilder tmpResultBuffer = new StringBuilder();

	private static StringBuilder tmpSymbolBuffer = new StringBuilder();

	private static StringBuilder tmpSymbolBuffer_objectLabel = new StringBuilder();

	private static StringBuilder tmpSymbolBuffer_subSymbol = new StringBuilder();

	private static StringBuilder tmpSymbolBuffer_function = new StringBuilder();

	private static StringBuilder tmpSymbolBuffer_args = new StringBuilder();

	private static StringBuilder tmpSymbolBuffer_functionArgs = new StringBuilder();

	private static List<string> tmpArgsLabels = new List<string>();

	private static List<object> tmpArgsObjects = new List<object>();

	private static StringBuilder tmpArg = new StringBuilder();

	private static List<string> numCaseArgs = new List<string>();

	private static List<string> replaceArgs = new List<string>();

	private static List<string> functionArgs = new List<string>();

	public static TaggedString Formatted(TaggedString str, List<string> argsLabelsArg, List<object> argsObjectsArg)
	{
		if (str.NullOrEmpty())
		{
			return str;
		}
		bool flag;
		StringBuilder stringBuilder;
		List<string> list;
		List<object> list2;
		if (formatterWorking)
		{
			flag = false;
			stringBuilder = new StringBuilder();
			list = argsLabelsArg.ToList();
			list2 = argsObjectsArg.ToList();
		}
		else
		{
			flag = true;
			stringBuilder = tmpResultBuffer;
			list = tmpArgsLabels;
			list.Clear();
			list.AddRange(argsLabelsArg);
			list2 = tmpArgsObjects;
			list2.Clear();
			list2.AddRange(argsObjectsArg);
		}
		if (flag)
		{
			formatterWorking = true;
		}
		try
		{
			stringBuilder.Length = 0;
			TryResolveInner(str, 0, stringBuilder, list, list2);
			string str2 = GenText.CapitalizeSentences(stringBuilder.ToString(), capitalizeFirstSentence: false);
			str2 = Find.ActiveLanguageWorker.PostProcessed(str2);
			return str2;
		}
		finally
		{
			if (flag)
			{
				formatterWorking = false;
			}
		}
	}

	private static int TryResolveInner(TaggedString str, int strOffset, StringBuilder resultBuffer, List<string> argsLabels, List<object> argsObjects, bool recursive = false)
	{
		bool flag = false;
		StringBuilder stringBuilder;
		StringBuilder stringBuilder2;
		StringBuilder stringBuilder3;
		StringBuilder stringBuilder4;
		StringBuilder stringBuilder5;
		StringBuilder stringBuilder6;
		if (symbolParserWorking)
		{
			stringBuilder = new StringBuilder();
			stringBuilder2 = new StringBuilder();
			stringBuilder3 = new StringBuilder();
			stringBuilder4 = new StringBuilder();
			stringBuilder5 = new StringBuilder();
			stringBuilder6 = new StringBuilder();
		}
		else
		{
			flag = true;
			symbolParserWorking = true;
			stringBuilder = tmpSymbolBuffer;
			stringBuilder2 = tmpSymbolBuffer_objectLabel;
			stringBuilder3 = tmpSymbolBuffer_subSymbol;
			stringBuilder4 = tmpSymbolBuffer_function;
			stringBuilder5 = tmpSymbolBuffer_args;
			stringBuilder6 = tmpSymbolBuffer_functionArgs;
		}
		int num = 0;
		int num2 = strOffset;
		while (num2 < str.Length)
		{
			char c = str[num2];
			if (c == '{')
			{
				stringBuilder.Length = 0;
				stringBuilder4.Length = 0;
				stringBuilder2.Length = 0;
				stringBuilder3.Length = 0;
				stringBuilder5.Length = 0;
				stringBuilder6.Length = 0;
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				bool flag5 = false;
				num2++;
				bool flag6 = num2 < str.Length && str[num2] == '{';
				if (!flag6)
				{
					bool flag7 = false;
					int num3 = num2;
					int num4 = 0;
					while (num3 < str.Length)
					{
						char c2 = str[num3];
						if (c2 == '{' || c2 == '}' || c2 == '?' || (c2 == ' ' && flag7))
						{
							break;
						}
						if (c2 != ' ')
						{
							flag7 = true;
						}
						if (c2 == ':')
						{
							flag5 = true;
							break;
						}
						stringBuilder4.Append(c2);
						num3++;
						num4++;
					}
					if (flag5)
					{
						num2 = num3 + 1;
						num += num4 + 1;
					}
				}
				while (num2 < str.Length)
				{
					char c3 = str[num2];
					if (c3 == '}')
					{
						flag2 = true;
						num++;
						break;
					}
					if (c3 == '{' && !flag6)
					{
						if (flag4 || flag5)
						{
							int num5 = TryResolveInner(str, num2, flag5 ? stringBuilder6 : stringBuilder5, argsLabels, argsObjects, recursive: true);
							num2 += num5;
							num += num5;
						}
						else
						{
							Log.ErrorOnce("Tried to use nested symbol for something but a symbol argument, this is not supported. For string: " + str, str.GetHashCode() ^ 0xB9D4932);
						}
					}
					else
					{
						stringBuilder.Append(c3);
						if (!flag5)
						{
							if (c3 == '_' && !flag3)
							{
								flag3 = true;
							}
							else if (c3 == '?' && !flag4)
							{
								flag4 = true;
							}
							else if (flag4)
							{
								stringBuilder5.Append(c3);
							}
							else if (flag3)
							{
								stringBuilder3.Append(c3);
							}
							else
							{
								stringBuilder2.Append(c3);
							}
						}
						else
						{
							stringBuilder6.Append(c3);
						}
					}
					num2++;
					num++;
				}
				if (!flag2)
				{
					Log.ErrorOnce("Could not find matching '}' in \"" + str + "\".", str.GetHashCode() ^ 0xB9D492D);
				}
				else if (flag6)
				{
					resultBuffer.Append(stringBuilder);
				}
				else if (flag5)
				{
					resultBuffer.Append(ResolveFunction(stringBuilder4.ToString(), stringBuilder6.ToString(), str));
				}
				else
				{
					if (flag4)
					{
						while (stringBuilder3.Length != 0 && stringBuilder3[stringBuilder3.Length - 1] == ' ')
						{
							stringBuilder3.Length--;
						}
					}
					string text = stringBuilder2.ToString();
					bool flag8 = false;
					int result = -1;
					if (int.TryParse(text, out result))
					{
						if (result >= 0 && result < argsObjects.Count && TryResolveSymbol(argsObjects[result], stringBuilder3.ToString(), stringBuilder5.ToString(), out var resolvedStr, str))
						{
							flag8 = true;
							resultBuffer.Append(resolvedStr.RawText);
						}
					}
					else
					{
						for (int i = 0; i < argsLabels.Count; i++)
						{
							if (argsLabels[i] == text)
							{
								if (TryResolveSymbol(argsObjects[i], stringBuilder3.ToString(), stringBuilder5.ToString(), out var resolvedStr2, str))
								{
									flag8 = true;
									resultBuffer.Append(resolvedStr2.RawText);
								}
								break;
							}
						}
						for (int j = 0; j < argsLabels.Count; j++)
						{
							if (argsLabels[j] == text + "_" + stringBuilder3 && argsObjects[j] is Gender gender)
							{
								flag8 = true;
								resultBuffer.Append(ResolveGenderSymbol(gender, animal: false, stringBuilder5.ToString(), str));
								break;
							}
						}
					}
					if (!flag8)
					{
						Log.ErrorOnce("Could not resolve symbol \"" + stringBuilder?.ToString() + "\" for string \"" + str + "\".", str.GetHashCode() ^ stringBuilder.ToString().GetHashCode() ^ 0x346E76FE);
					}
				}
				if (recursive)
				{
					break;
				}
			}
			else
			{
				resultBuffer.Append(c);
			}
			num2++;
			num++;
		}
		if (flag)
		{
			symbolParserWorking = false;
		}
		return num;
	}

	private static bool TryResolveSymbol(object obj, string subSymbol, string symbolArgs, out TaggedString resolvedStr, string fullStringForReference)
	{
		if (obj is Pawn pawn)
		{
			switch (subSymbol)
			{
			case "":
				resolvedStr = ((pawn.Name != null) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name) : ((TaggedString)pawn.KindLabelIndefinite()));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "nameFull":
				resolvedStr = ((pawn.Name != null) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Name.ToStringFull, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name) : ((TaggedString)pawn.KindLabelIndefinite()));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "nameFullDef":
				resolvedStr = ((pawn.Name != null) ? Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringFull, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name) : ((TaggedString)pawn.KindLabelDefinite()));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "parentage":
				resolvedStr = pawn.GetParentage();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "label":
				resolvedStr = pawn.LabelNoCountColored;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelNoParenthesis":
				resolvedStr = pawn.LabelNoParenthesis;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelShort":
				resolvedStr = ((pawn.Name != null) ? pawn.Name.ToStringShort.ApplyTag(TagType.Name) : ((TaggedString)pawn.KindLabel));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "definite":
				resolvedStr = ((pawn.Name != null) ? Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name) : ((TaggedString)pawn.KindLabelDefinite()));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "nameDef":
				resolvedStr = ((pawn.Name != null) ? Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name) : ((TaggedString)pawn.KindLabelDefinite()));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "indefinite":
				resolvedStr = ((pawn.Name != null) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name) : ((TaggedString)pawn.KindLabelIndefinite()));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "nameIndef":
				resolvedStr = ((pawn.Name != null) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name) : ((TaggedString)pawn.KindLabelIndefinite()));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "pronoun":
				resolvedStr = pawn.gender.GetPronoun();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "possessive":
				resolvedStr = pawn.gender.GetPossessive();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "objective":
				resolvedStr = pawn.gender.GetObjective();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "genderNoun":
				resolvedStr = pawn.gender.GetGenderNoun();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "factionName":
				resolvedStr = ((pawn.Faction != null) ? pawn.Faction.Name.ApplyTag(pawn.Faction) : ((TaggedString)""));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "factionPawnSingular":
				resolvedStr = ((pawn.Faction != null) ? pawn.Faction.def.pawnSingular : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "factionPawnSingularDef":
				resolvedStr = ((pawn.Faction != null) ? Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Faction.def.pawnSingular) : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "factionPawnSingularIndef":
				resolvedStr = ((pawn.Faction != null) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Faction.def.pawnSingular) : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "factionPawnsPlural":
				resolvedStr = ((pawn.Faction != null) ? pawn.Faction.def.pawnsPlural : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "factionPawnsPluralDef":
				resolvedStr = ((pawn.Faction != null) ? Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(pawn.Faction.def.pawnsPlural, pawn.Faction.def.pawnSingular), plural: true) : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "factionPawnsPluralIndef":
				resolvedStr = ((pawn.Faction != null) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(pawn.Faction.def.pawnsPlural, pawn.Faction.def.pawnSingular), plural: true) : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "factionRoyalFavorLabel":
				resolvedStr = ((pawn.Faction != null) ? pawn.Faction.def.royalFavorLabel : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "kind":
				resolvedStr = pawn.KindLabel;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "kindDef":
				resolvedStr = pawn.KindLabelDefinite();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "kindIndef":
				resolvedStr = pawn.KindLabelIndefinite();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "kindPlural":
				resolvedStr = pawn.GetKindLabelPlural();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "kindPluralDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.GetKindLabelPlural(), pawn.gender, plural: true);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "kindPluralIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.GetKindLabelPlural(), pawn.gender, plural: true);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "kindBase":
				resolvedStr = GenLabel.BestKindLabel(pawn.kindDef, pawn.gender);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "kindBaseDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.kindDef.label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "kindBaseIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.kindDef.label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "kindBasePlural":
				resolvedStr = GenLabel.BestKindLabel(pawn.kindDef, pawn.gender, plural: true, 2);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "kindBasePluralDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.kindDef.GetLabelPlural(), LanguageDatabase.activeLanguage.ResolveGender(pawn.kindDef.GetLabelPlural(), pawn.kindDef.label), plural: true);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "kindBasePluralIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.kindDef.GetLabelPlural(), LanguageDatabase.activeLanguage.ResolveGender(pawn.kindDef.GetLabelPlural(), pawn.kindDef.label), plural: true);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "race":
				resolvedStr = pawn.def.label;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "raceDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.def.label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "raceIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.def.label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "lifeStage":
				resolvedStr = pawn.ageTracker.CurLifeStage.label;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "lifeStageDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.ageTracker.CurLifeStage.label, pawn.gender);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "lifeStageIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.ageTracker.CurLifeStage.label, pawn.gender);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "lifeStageAdjective":
				resolvedStr = pawn.ageTracker.CurLifeStage.Adjective;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "legalStatus":
				resolvedStr = pawn.LegalStatus;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "title":
				resolvedStr = ((pawn.story != null) ? pawn.story.Title : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "titleDef":
				resolvedStr = ((pawn.story != null) ? Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.story.Title, ResolveGender(pawn.story.Title, pawn.gender)) : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "titleIndef":
				resolvedStr = ((pawn.story != null) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.story.Title, ResolveGender(pawn.story.Title, pawn.gender)) : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "bestRoyalTitle":
				resolvedStr = PawnResolveBestRoyalTitle(pawn);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "bestRoyalTitleIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(PawnResolveBestRoyalTitle(pawn));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "bestRoyalTitleDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(PawnResolveBestRoyalTitle(pawn));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "royalTitleInCurrentFaction":
				resolvedStr = PawnResolveRoyalTitleInCurrentFaction(pawn);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "royalTitleInCurrentFactionIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(PawnResolveRoyalTitleInCurrentFaction(pawn));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "royalTitleInCurrentFactionDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(PawnResolveRoyalTitleInCurrentFaction(pawn));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "age":
				resolvedStr = pawn.ageTracker.AgeBiologicalYears.ToString();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "age_numCase":
				resolvedStr = ResolveNumCase(pawn.ageTracker.AgeBiologicalYears.ToString(), symbolArgs, fullStringForReference);
				return true;
			case "chronologicalAge":
				resolvedStr = pawn.ageTracker.AgeChronologicalYears.ToString();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "ageFull":
				resolvedStr = pawn.ageTracker.AgeNumberString;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "relationInfo":
			{
				resolvedStr = "";
				TaggedString text = resolvedStr;
				PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);
				resolvedStr = text.RawText;
				return true;
			}
			case "relationInfoInParentheses":
				resolvedStr = "";
				PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref resolvedStr, pawn);
				if (!resolvedStr.NullOrEmpty())
				{
					resolvedStr = "(" + resolvedStr + ")";
				}
				return true;
			case "gender":
				resolvedStr = ResolveGenderSymbol(pawn.gender, pawn.RaceProps.Animal, symbolArgs, fullStringForReference);
				return true;
			case "genderResolved":
				resolvedStr = ResolveGenderSymbol((pawn.Name != null) ? pawn.gender : ResolveGender(pawn.KindLabel, pawn.gender), pawn.RaceProps.Animal, symbolArgs, fullStringForReference);
				return true;
			case "humanlike":
				resolvedStr = ResolveHumanlikeSymbol(pawn.RaceProps.Humanlike, symbolArgs, fullStringForReference);
				return true;
			case "xenotype":
				resolvedStr = pawn.genes.XenotypeLabel;
				return true;
			default:
				resolvedStr = "";
				return false;
			}
		}
		if (obj is Thing thing)
		{
			switch (subSymbol)
			{
			case "":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(thing.Label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "label":
				resolvedStr = thing.Label;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelCap":
				resolvedStr = thing.LabelCap;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelNoParenthesis":
				resolvedStr = thing.LabelNoParenthesis;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelNoParenthesisDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(thing.LabelNoParenthesis);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelNoParenthesisIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(thing.LabelNoParenthesis);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelPlural":
				resolvedStr = Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelPluralDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount), thing.LabelNoCount), plural: true);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelPluralIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount), thing.LabelNoCount), plural: true);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelShort":
				resolvedStr = thing.LabelShort;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelShortDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(thing.LabelShort);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelShortIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(thing.LabelShort);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "definite":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(thing.Label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "indefinite":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(thing.Label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "pronoun":
				resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(thing.LabelNoCount).GetPronoun();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "possessive":
				resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(thing.LabelNoCount).GetPossessive();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "objective":
				resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(thing.LabelNoCount).GetObjective();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "factionName":
				resolvedStr = ((thing.Faction != null) ? thing.Faction.Name : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "gender":
				resolvedStr = ResolveGenderSymbol(LanguageDatabase.activeLanguage.ResolveGender(thing.LabelNoCount), animal: false, symbolArgs, fullStringForReference);
				return true;
			case "quality":
			{
				resolvedStr = (thing.TryGetQuality(out var qc) ? qc.GetLabel() : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			}
			default:
				resolvedStr = "";
				return false;
			}
		}
		if (obj is Hediff hediff)
		{
			if (subSymbol != null && subSymbol.Length == 0)
			{
				resolvedStr = hediff.Label;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			}
			if (subSymbol == "label")
			{
				resolvedStr = hediff.Label;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			}
			if (subSymbol == "labelNoun")
			{
				resolvedStr = ((!hediff.def.labelNoun.NullOrEmpty()) ? hediff.def.labelNoun : hediff.Label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			}
		}
		if (obj is WorldObject worldObject)
		{
			switch (subSymbol)
			{
			case "":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(worldObject.Label, plural: false, worldObject.HasName);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "label":
				resolvedStr = worldObject.Label;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelCap":
				resolvedStr = worldObject.LabelCap;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelPlural":
				resolvedStr = Find.ActiveLanguageWorker.Pluralize(worldObject.Label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelPluralDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(Find.ActiveLanguageWorker.Pluralize(worldObject.Label), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(worldObject.Label), worldObject.Label), plural: true, worldObject.HasName);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelPluralIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(worldObject.Label), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(worldObject.Label), worldObject.Label), plural: true, worldObject.HasName);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "definite":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(worldObject.Label, plural: false, worldObject.HasName);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "indefinite":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(worldObject.Label, plural: false, worldObject.HasName);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "pronoun":
				resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(worldObject.Label).GetPronoun();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "possessive":
				resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(worldObject.Label).GetPossessive();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "objective":
				resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(worldObject.Label).GetObjective();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "factionName":
				resolvedStr = ((worldObject.Faction != null) ? worldObject.Faction.Name.ApplyTag(worldObject.Faction) : ((TaggedString)""));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "gender":
				resolvedStr = ResolveGenderSymbol(LanguageDatabase.activeLanguage.ResolveGender(worldObject.Label), animal: false, symbolArgs, fullStringForReference);
				return true;
			default:
				resolvedStr = "";
				return false;
			}
		}
		if (obj is Faction faction)
		{
			switch (subSymbol)
			{
			case "":
				resolvedStr = faction.Name.ApplyTag(faction);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "name":
				resolvedStr = faction.Name.ApplyTag(faction);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "pawnSingular":
				resolvedStr = faction.def.pawnSingular;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "pawnSingularDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(faction.def.pawnSingular);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "pawnSingularIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(faction.def.pawnSingular);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "pawnsPlural":
				resolvedStr = faction.def.pawnsPlural;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "pawnsPluralDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(faction.def.pawnsPlural, faction.def.pawnSingular), plural: true);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "pawnsPluralIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(faction.def.pawnsPlural, faction.def.pawnSingular), plural: true);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "royalFavorLabel":
				resolvedStr = faction.def.royalFavorLabel;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "leaderNameDef":
				resolvedStr = ((faction.leader != null && faction.leader.Name != null) ? Find.ActiveLanguageWorker.WithDefiniteArticle(faction.leader.Name.ToStringShort, faction.leader.gender, plural: false, name: true).ApplyTag(TagType.Name) : ((TaggedString)""));
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "leaderPossessive":
				resolvedStr = ((faction.leader != null) ? faction.leader.gender.GetPossessive() : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "leaderObjective":
				resolvedStr = ((faction.leader != null) ? faction.leader.gender.GetObjective() : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "leaderPronoun":
				resolvedStr = ((faction.leader != null) ? faction.leader.gender.GetPronoun() : "");
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			default:
				resolvedStr = "";
				return false;
			}
		}
		if (obj is Ideo ideo)
		{
			switch (subSymbol)
			{
			case "":
				resolvedStr = ideo.name.ApplyTag(ideo);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "name":
				resolvedStr = ideo.name.ApplyTag(ideo);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "memberName":
				resolvedStr = ideo.memberName;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "memberNamePlural":
				resolvedStr = ideo.MemberNamePlural;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "memberNameIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(ideo.memberName);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "memberNameDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(ideo.memberName);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "adjective":
				resolvedStr = ideo.adjective;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			default:
				resolvedStr = "";
				return false;
			}
		}
		if (obj is Precept precept)
		{
			switch (subSymbol)
			{
			case "":
			case "label":
			case "name":
				resolvedStr = precept.Label;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(precept.Label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelCap":
				resolvedStr = precept.LabelCap;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelCapIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(precept.Label).CapitalizeFirst();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(precept.Label, plural: false, !precept.usesDefiniteArticle);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelCapDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(precept.Label, plural: false, !precept.usesDefiniteArticle).CapitalizeFirst();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			default:
				resolvedStr = "";
				return false;
			}
		}
		if (obj is Def def)
		{
			if (def is PawnKindDef pawnKindDef)
			{
				switch (subSymbol)
				{
				case "labelPlural":
					resolvedStr = pawnKindDef.GetLabelPlural();
					EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "labelPluralDef":
					resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawnKindDef.GetLabelPlural(), LanguageDatabase.activeLanguage.ResolveGender(pawnKindDef.GetLabelPlural(), pawnKindDef.label), plural: true);
					EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				case "labelPluralIndef":
					resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawnKindDef.GetLabelPlural(), LanguageDatabase.activeLanguage.ResolveGender(pawnKindDef.GetLabelPlural(), pawnKindDef.label), plural: true);
					EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
					return true;
				}
			}
			switch (subSymbol)
			{
			case "":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(def.label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "label":
				resolvedStr = def.label;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelPlural":
				resolvedStr = Find.ActiveLanguageWorker.Pluralize(def.label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelPluralDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(Find.ActiveLanguageWorker.Pluralize(def.label), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(def.label), def.label), plural: true);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "labelPluralIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(def.label), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(def.label), def.label), plural: true);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "definite":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(def.label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "indefinite":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(def.label);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "pronoun":
				resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(def.label).GetPronoun();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "possessive":
				resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(def.label).GetPossessive();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "objective":
				resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(def.label).GetObjective();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "gender":
				resolvedStr = ResolveGenderSymbol(LanguageDatabase.activeLanguage.ResolveGender(def.label), animal: false, symbolArgs, fullStringForReference);
				return true;
			default:
				resolvedStr = "";
				return false;
			}
		}
		if (obj is RoyalTitle royalTitle)
		{
			if (subSymbol == null || subSymbol.Length != 0)
			{
				if (!(subSymbol == "label"))
				{
					if (subSymbol == "indefinite")
					{
						resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticlePostProcessed(royalTitle.Label);
						EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
						return true;
					}
					resolvedStr = "";
					return false;
				}
				resolvedStr = royalTitle.Label;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			}
			resolvedStr = royalTitle.Label;
			EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
			return true;
		}
		if (obj is string text2)
		{
			switch (subSymbol)
			{
			case "":
				resolvedStr = text2;
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "plural":
				resolvedStr = Find.ActiveLanguageWorker.Pluralize(text2);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "pluralDef":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(Find.ActiveLanguageWorker.Pluralize(text2), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(text2), text2), plural: true);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "pluralIndef":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(text2), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(text2), text2), plural: true);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "definite":
				resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(text2);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "indefinite":
				resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(text2);
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "pronoun":
				resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(text2).GetPronoun();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "possessive":
				resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(text2).GetPossessive();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "objective":
				resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(text2).GetObjective();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "gender":
				resolvedStr = ResolveGenderSymbol(LanguageDatabase.activeLanguage.ResolveGender(text2), animal: false, symbolArgs, fullStringForReference);
				return true;
			case "replace":
				resolvedStr = ResolveReplace(text2, symbolArgs);
				return true;
			case "numCase":
				resolvedStr = ResolveNumCase(text2, symbolArgs, fullStringForReference);
				return true;
			default:
				resolvedStr = "";
				return false;
			}
		}
		if (obj is int || obj is long || obj is float)
		{
			int num = (int)((obj is int) ? ((int)obj) : ((obj is float) ? ((int)(float)obj) : ((long)obj)));
			float f = (obj as float?) ?? ((float)num);
			switch (subSymbol)
			{
			case "":
				resolvedStr = num.ToString();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "ordinal":
				resolvedStr = Find.ActiveLanguageWorker.OrdinalNumber(num).ToString();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "multiple":
				resolvedStr = ResolveMultipleSymbol(num, symbolArgs, fullStringForReference);
				return true;
			case "numCase":
				resolvedStr = ResolveNumCase(num.ToString(), symbolArgs, fullStringForReference);
				return true;
			case "percentage":
				resolvedStr = f.ToStringPercent();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "percentageEmptyZero":
				resolvedStr = f.ToStringPercentEmptyZero();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			case "time":
				resolvedStr = num.ToStringTicksToPeriod();
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				return true;
			default:
				resolvedStr = "";
				return false;
			}
		}
		if (obj is TaggedString)
		{
			EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
			resolvedStr = ((TaggedString)obj).RawText;
		}
		if (subSymbol.NullOrEmpty())
		{
			EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
			if (obj == null)
			{
				resolvedStr = "";
			}
			else
			{
				resolvedStr = obj.ToString();
			}
			return true;
		}
		resolvedStr = "";
		return false;
	}

	private static void EnsureNoArgs(string subSymbol, string symbolArgs, string fullStringForReference)
	{
		if (!symbolArgs.NullOrEmpty())
		{
			Log.ErrorOnce("Symbol \"" + subSymbol + "\" doesn't expect any args but \"" + symbolArgs + "\" args were provided. Full string: \"" + fullStringForReference + "\".", subSymbol.GetHashCode() ^ symbolArgs.GetHashCode() ^ fullStringForReference.GetHashCode() ^ 0x391B4B8E);
		}
	}

	public static string ResolveGenderSymbol(Gender gender, bool animal, string args, string fullStringForReference)
	{
		if (args.NullOrEmpty())
		{
			return gender.GetLabel(animal);
		}
		switch (GetArgsCount(args))
		{
		case 2:
			return gender switch
			{
				Gender.Male => GetArg(args, 0), 
				Gender.Female => GetArg(args, 1), 
				Gender.None => GetArg(args, 0), 
				_ => "", 
			};
		case 3:
			return gender switch
			{
				Gender.Male => GetArg(args, 0), 
				Gender.Female => GetArg(args, 1), 
				Gender.None => GetArg(args, 2), 
				_ => "", 
			};
		default:
			Log.ErrorOnce("Invalid args count in \"" + fullStringForReference + "\" for symbol \"gender\".", args.GetHashCode() ^ fullStringForReference.GetHashCode() ^ 0x2EF21A43);
			return "";
		}
	}

	private static string ResolveHumanlikeSymbol(bool humanlike, string args, string fullStringForReference)
	{
		if (GetArgsCount(args) == 2)
		{
			if (humanlike)
			{
				return GetArg(args, 0);
			}
			return GetArg(args, 1);
		}
		Log.ErrorOnce("Invalid args count in \"" + fullStringForReference + "\" for symbol \"humanlike\".", args.GetHashCode() ^ fullStringForReference.GetHashCode() ^ 0x355A4AD5);
		return "";
	}

	private static string ResolveMultipleSymbol(int count, string args, string fullStringForReference)
	{
		if (GetArgsCount(args) == 2)
		{
			if (count > 1)
			{
				return GetArg(args, 0);
			}
			return GetArg(args, 1);
		}
		Log.ErrorOnce("Invalid args count in \"" + fullStringForReference + "\" for symbol \"multiple\".", args.GetHashCode() ^ fullStringForReference.GetHashCode() ^ 0xDC89D8D);
		return "";
	}

	public static Gender ResolveGender(string word, Gender defaultGender)
	{
		return LanguageDatabase.activeLanguage.ResolveGender(word, null, defaultGender);
	}

	public static int GetArgsCount(string args, char delimiter = ':')
	{
		int num = 1;
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == delimiter)
			{
				num++;
			}
		}
		return num;
	}

	public static string GetArg(string args, int argIndex, char delimiter = ':')
	{
		tmpArg.Length = 0;
		int num = 0;
		foreach (char c in args)
		{
			if (c == delimiter)
			{
				num++;
			}
			else if (num == argIndex)
			{
				tmpArg.Append(c);
			}
			else if (num > argIndex)
			{
				break;
			}
		}
		while (tmpArg.Length != 0 && tmpArg[0] == ' ')
		{
			tmpArg.Remove(0, 1);
		}
		while (tmpArg.Length != 0 && tmpArg[tmpArg.Length - 1] == ' ')
		{
			tmpArg.Length--;
		}
		return tmpArg.ToString();
	}

	public static string PawnResolveBestRoyalTitle(Pawn pawn)
	{
		if (pawn.royalty == null)
		{
			return "";
		}
		RoyalTitle royalTitle = null;
		foreach (RoyalTitle item in pawn.royalty.AllTitlesForReading.OrderBy((RoyalTitle x) => x.def.index))
		{
			if (royalTitle == null || item.def.favorCost > royalTitle.def.favorCost)
			{
				royalTitle = item;
			}
		}
		if (royalTitle == null)
		{
			return "";
		}
		return royalTitle.def.GetLabelFor(pawn.gender);
	}

	public static string PawnResolveRoyalTitleInCurrentFaction(Pawn pawn)
	{
		if (pawn.royalty != null)
		{
			foreach (RoyalTitle item in pawn.royalty.AllTitlesForReading.OrderBy((RoyalTitle x) => x.def.index))
			{
				if (item.faction == pawn.Faction)
				{
					return item.def.GetLabelFor(pawn.gender);
				}
			}
		}
		return "";
	}

	public static string ResolveNumCase(string number, string args, string fullStringForReference)
	{
		LanguageWorker activeLanguageWorker = Find.ActiveLanguageWorker;
		int num = LanguageDatabase.activeLanguage.info.totalNumCaseCount ?? activeLanguageWorker.TotalNumCaseCount;
		if (GetArgsCount(args) != num)
		{
			Log.Error("Invalid argument count for _numCase, expected " + num + " arguments. Full string: " + fullStringForReference);
			return "";
		}
		numCaseArgs.Clear();
		for (int i = 0; i < num; i++)
		{
			numCaseArgs.Add(GetArg(args, i));
		}
		if (float.TryParse(number, out var result))
		{
			return activeLanguageWorker.ResolveNumCase(result, numCaseArgs);
		}
		return "";
	}

	public static string ResolveReplace(string symbol, string args)
	{
		LanguageWorker activeLanguageWorker = Find.ActiveLanguageWorker;
		int argsCount = GetArgsCount(args);
		replaceArgs.Clear();
		replaceArgs.Add(symbol);
		for (int i = 0; i < argsCount; i++)
		{
			replaceArgs.Add(GetArg(args, i));
		}
		return activeLanguageWorker.ResolveReplace(replaceArgs);
	}

	public static string ResolveFunction(string name, string args, string fullStringForReference)
	{
		functionArgs.Clear();
		int argsCount = GetArgsCount(args, ';');
		for (int i = 0; i < argsCount; i++)
		{
			functionArgs.Add(GetArg(args, i, ';'));
		}
		return Find.ActiveLanguageWorker.ResolveFunction(name, functionArgs, fullStringForReference);
	}

	public static List<string> TryParseNumCase(string str)
	{
		int num = str.IndexOf("{0_numCase", StringComparison.Ordinal);
		if (num != -1)
		{
			int i = num + 10;
			bool flag = false;
			bool flag2 = false;
			string text = "";
			for (; i < str.Length; i++)
			{
				if (str[i] == '?')
				{
					flag = true;
					continue;
				}
				if (str[i] == '}')
				{
					flag2 = true;
					break;
				}
				if (flag)
				{
					text += str[i];
				}
			}
			if (!flag2)
			{
				return null;
			}
			int argsCount = GetArgsCount(text);
			if (argsCount > 0)
			{
				List<string> list = new List<string>();
				for (int j = 0; j < argsCount; j++)
				{
					list.Add(GetArg(text, j));
				}
				return list;
			}
		}
		return null;
	}
}
