using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Verse;
using Verse.Grammar;

namespace RimWorld;

internal static class IdeoDescriptionUtility
{
	internal class DescriptionGrammarCustomizer : GrammarRequest.ICustomizer
	{
		private readonly Dictionary<Rule, int> usedMemeRules = new Dictionary<Rule, int>();

		public string targetTag;

		public int PriorityBucket(Rule rule)
		{
			if (targetTag != null && targetTag.Equals(MemeGroupOf(rule)))
			{
				return 2;
			}
			return 1;
		}

		public void Notify_RuleUsed(Rule rule)
		{
			usedMemeRules.Increment(rule);
		}

		public bool ValidateRule(Rule rule)
		{
			if (!rule.usesLimit.HasValue)
			{
				return true;
			}
			return usedMemeRules.TryGetValue(rule, 0) < rule.usesLimit.Value;
		}
	}

	public class SegmentEvaluator
	{
		private readonly DescriptionGrammarCustomizer customizer;

		public readonly GrammarRequest request;

		internal SegmentEvaluator(DescriptionGrammarCustomizer customizer, GrammarRequest request)
		{
			this.customizer = customizer;
			this.request = request;
		}

		public string NextSegment(string segment, string targetTag, bool capitalizeFirstSentence)
		{
			customizer.targetTag = targetTag;
			string result = GrammarResolver.Resolve(segment, request, null, forceLog: false, null, null, null, capitalizeFirstSentence);
			customizer.targetTag = null;
			return result;
		}
	}

	private const string CustomPatternName = "r_pattern";

	private static readonly Regex TokenPattern = new Regex("%%(?<keyword>[^%]+)%%");

	private static string MemeGroupOf(Rule rule)
	{
		if (rule.tag == null || !rule.tag.StartsWith("meme_"))
		{
			return null;
		}
		return rule.tag;
	}

	private static SegmentEvaluator BuildGrammarRequest(Ideo ideo, IdeoStoryPatternDef pattern, Dictionary<string, string> tokens)
	{
		GrammarRequest request = default(GrammarRequest);
		DescriptionGrammarCustomizer customizer = (DescriptionGrammarCustomizer)(request.customizer = new DescriptionGrammarCustomizer());
		AddRandomPawn("founder", ideo, ref request);
		AddRandomPawn("believer", ideo, ref request);
		if (!ideo.memberName.NullOrEmpty())
		{
			tokens.AddDistinct("memberName", ideo.memberName);
		}
		if (!ideo.MemberNamePlural.NullOrEmpty())
		{
			tokens.AddDistinct("memberNamePlural", ideo.MemberNamePlural);
		}
		if (!ideo.adjective.NullOrEmpty())
		{
			tokens.AddDistinct("adjective", ideo.adjective);
		}
		foreach (MemeDef meme in ideo.memes)
		{
			if (meme.generalRules != null)
			{
				request.IncludesBare.Add(meme.generalRules);
			}
			if (meme.descriptionMaker?.rules != null)
			{
				request.IncludesBare.Add(meme.descriptionMaker.rules);
			}
			if (meme.descriptionMaker?.constants != null)
			{
				request.Constants.AddRange(meme.descriptionMaker?.constants);
			}
		}
		AddPreceptRole(ideo, "leader", PreceptDefOf.IdeoRole_Leader, tokens);
		AddPreceptRole(ideo, "moralist", PreceptDefOf.IdeoRole_Moralist, tokens);
		AddPreceptRules<Precept_Relic>(ideo, "relic", tokens);
		AddPreceptRules<Precept_Animal>(ideo, "animal", tokens);
		AddPreceptRules<Precept_Ritual>(ideo, "ritual", tokens);
		AddPreceptRules(ideo, "altar", tokens, (Precept_Building p) => p.ThingDef.isAltar);
		string worshipRoomLabel = ideo.WorshipRoomLabel;
		if (worshipRoomLabel != null)
		{
			tokens.AddDistinct("altarRoomLabel", worshipRoomLabel);
		}
		ideo.foundation.AddPlaceRules(ref request);
		if (ideo.foundation is IdeoFoundation_Deity ideoFoundation_Deity)
		{
			ideoFoundation_Deity.AddDeityRules(tokens, ref request);
		}
		if (pattern.rules != null)
		{
			request.IncludesBare.Add(pattern.rules);
		}
		request.Rules.Add(new Rule_String("foeSoldiers", GrammarResolver.Resolve("place_foeSoldiers", request, null, forceLog: false, null, null, null, capitalizeFirstSentence: false)));
		request.Rules.Add(new Rule_String("foeLeader", GrammarResolver.Resolve("place_foeLeader", request, null, forceLog: false, null, null, null, capitalizeFirstSentence: false)));
		request.Constants.SetOrAdd("foeLeader_gender", "Male");
		DecorateRule("memeConcept", ref request);
		DecorateRule("memeHyphenPrefix", ref request);
		DecorateRule("attributionJob", ref request);
		DecorateRule("attributionSource", ref request);
		DecorateRule("memeConference", ref request);
		return new SegmentEvaluator(customizer, request);
	}

	private static void DecorateRule(string keyword, ref GrammarRequest request)
	{
		if (request.HasRule(keyword))
		{
			string str = GrammarResolver.Resolve(keyword, request, null, forceLog: false, null, null, null, capitalizeFirstSentence: false);
			request.Rules.Add(new Rule_String(keyword + "_titleCase", Find.ActiveLanguageWorker.ToTitleCase(str)));
		}
	}

	private static void AddPreceptRules<T>(Ideo ideo, string rulePrefix, Dictionary<string, string> tokens, Func<T, bool> filter = null) where T : Precept
	{
		IEnumerable<T> source = ideo.PreceptsListForReading.OfType<T>();
		if (filter != null)
		{
			source = source.Where(filter);
		}
		List<T> list = source.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			T val = list[i];
			string text = $"{rulePrefix}{i}_";
			tokens.AddDistinct(text + "name", val.Label.ApplyTag(TagType.Name).Resolve());
			tokens.AddDistinct(text + "label", val.Label.ApplyTag(TagType.Name).Resolve());
			if (val is Precept_ThingDef precept_ThingDef)
			{
				tokens.AddDistinct(text + "thingDef_label", precept_ThingDef.ThingDef.label.ApplyTag(TagType.Name).Resolve());
			}
		}
	}

	private static void AddPreceptRole(Ideo ideo, string prefix, PreceptDef def, Dictionary<string, string> tokens)
	{
		foreach (Precept_Role item in ideo.PreceptsListForReading.OfType<Precept_Role>())
		{
			if (item.def == def)
			{
				tokens.AddDistinct(prefix + "Title", item.Label);
				break;
			}
		}
	}

	private static void AddRandomPawn(string key, Ideo ideo, ref GrammarRequest request)
	{
		Gender gender = ideo.SupremeGender;
		if (gender == Gender.None)
		{
			gender = ((Rand.Value < 0.5f) ? Gender.Male : Gender.Female);
		}
		Name name = PawnBioAndNameGenerator.GenerateFullPawnName(ThingDefOf.Human, null, null, null, null, ideo.culture, creepjoiner: false, gender);
		int num = 21;
		int chronologicalAge = num;
		string relationInfo = "";
		foreach (Rule item in GrammarUtility.RulesForPawn(key, name, null, PawnKindDefOf.Colonist, gender, null, num, chronologicalAge, relationInfo, everBeenColonistOrTameAnimal: false, everBeenQuestLodger: false, isFactionLeader: false, null, cubeInterest: false, string.Empty, request.Constants, addTags: false))
		{
			request.Rules.Add(item);
		}
	}

	public static List<string> ShuffleSegmentPreferences(IdeoStoryPatternDef pattern, List<MemeDef> memes, Dictionary<string, string> constants)
	{
		List<string> list = new List<string>();
		List<Rule> list2 = (from rule in memes.Where((MemeDef meme) => meme.descriptionMaker?.rules != null).SelectMany((MemeDef meme) => meme.descriptionMaker?.rules.Rules)
			where MemeGroupOf(rule) != null
			where pattern.segments.Contains(rule.keyword)
			where rule.ValidateConstraints(constants)
			select rule).ToList();
		List<string> source = list2.Select((Rule rule) => MemeGroupOf(rule)).Distinct().ToList();
		for (int num = 0; num < 100; num++)
		{
			list.Clear();
			list.AddRange(source.InRandomOrder().Take(pattern.segments.Count));
			while (list.Count < pattern.segments.Count)
			{
				list.Add(null);
			}
			list.Shuffle();
			bool flag = true;
			for (int num2 = 0; num2 < pattern.segments.Count; num2++)
			{
				string segment = pattern.segments[num2];
				string tag = list[num2];
				if (tag != null && !list2.Any((Rule rule) => rule.tag == tag && rule.keyword == segment))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		return list;
	}

	public static IdeoDescriptionResult ResolveDescription(Ideo ideo, IdeoStoryPatternDef pattern, bool force)
	{
		Dictionary<string, string> tokens = new Dictionary<string, string>();
		SegmentEvaluator segmentEvaluator = BuildGrammarRequest(ideo, pattern, tokens);
		if (!force && ideo.descriptionTemplate != null && TokensInTemplate(ideo.descriptionTemplate).TrueForAll((string t) => tokens.ContainsKey(t)))
		{
			return EvaluateTemplate(ideo.descriptionTemplate, tokens);
		}
		foreach (string key in tokens.Keys)
		{
			segmentEvaluator.request.Rules.Add(new Rule_String(key, "%%" + key + "%%"));
		}
		List<string> list = ShuffleSegmentPreferences(pattern, ideo.memes, segmentEvaluator.request.Constants);
		List<string> list2 = new List<string>();
		for (int num = 0; num < pattern.segments.Count; num++)
		{
			string text = pattern.segments[num];
			string targetTag = list[num];
			bool capitalizeFirstSentence = !pattern.noCapitalizeFirstSentence.Contains(text);
			string item = segmentEvaluator.NextSegment(text, targetTag, capitalizeFirstSentence);
			list2.Add(item);
		}
		string tokenTemplate;
		if (segmentEvaluator.request.HasRule("r_pattern"))
		{
			for (int num2 = 0; num2 < pattern.segments.Count; num2++)
			{
				string text2 = pattern.segments[num2];
				string output = list2[num2];
				segmentEvaluator.request.Rules.Add(new Rule_String($"r_segment{num2}", output));
				segmentEvaluator.request.Rules.Add(new Rule_String("r_" + text2, output));
			}
			tokenTemplate = GrammarResolver.Resolve("r_pattern", segmentEvaluator.request);
		}
		else
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string item2 in list2)
			{
				if (!item2.NullOrEmpty())
				{
					stringBuilder.Append(item2).Append(" ");
				}
			}
			tokenTemplate = stringBuilder.ToString().TrimEnd();
		}
		IdeoDescriptionResult ideoDescriptionResult = EvaluateTemplate(tokenTemplate, tokens);
		ideoDescriptionResult.text = GrammarResolver.ResolveAllFunctions(ideoDescriptionResult.text, segmentEvaluator.request.ConstantsAllowNull);
		return ideoDescriptionResult;
	}

	private static string ConvertTokensToRules(string template, List<string> tokensSeen = null)
	{
		return TokenPattern.Replace(template, Replacement);
		string Replacement(Match match)
		{
			string value = match.Groups["keyword"].Value;
			value = value.UncapitalizeFirst();
			tokensSeen?.AddUnique(value);
			return "[" + value + "]";
		}
	}

	private static List<string> TokensInTemplate(string template)
	{
		List<string> list = new List<string>();
		ConvertTokensToRules(template, list);
		return list;
	}

	private static IdeoDescriptionResult EvaluateTemplate(string tokenTemplate, Dictionary<string, string> tokens)
	{
		GrammarRequest request = default(GrammarRequest);
		request.Rules.AddRange(tokens.Select((KeyValuePair<string, string> kv) => new Rule_String(kv.Key, kv.Value)));
		request.Rules.Add(new Rule_String("r_result", ConvertTokensToRules(tokenTemplate)));
		return new IdeoDescriptionResult
		{
			template = tokenTemplate,
			text = GrammarResolver.Resolve("r_result", request)
		};
	}
}
