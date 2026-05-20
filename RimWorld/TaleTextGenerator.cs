using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public static class TaleTextGenerator
{
	private const float TalelessChanceWithTales = 0.2f;

	public static TaggedString GenerateTextFromTale(TextGenerationPurpose purpose, Tale tale, int seed, RulePackDef extraInclude, List<Rule> extraRules = null, Dictionary<string, string> extraConstants = null)
	{
		return GenerateTextFromTale(purpose, tale, seed, new List<RulePackDef> { extraInclude }, extraRules, extraConstants);
	}

	public static TaggedString GenerateTextFromTale(TextGenerationPurpose purpose, Tale tale, int seed, List<RulePackDef> extraInclude = null, List<Rule> extraRules = null, Dictionary<string, string> extraConstants = null)
	{
		Rand.PushState();
		Rand.Seed = seed;
		string rootKeyword = null;
		GrammarRequest request = default(GrammarRequest);
		if (extraInclude != null)
		{
			request.Includes.AddRange(extraInclude);
		}
		if (extraRules != null)
		{
			request.Rules.AddRange(extraRules);
		}
		if (extraConstants != null)
		{
			request.Constants.AddRange(extraConstants);
		}
		switch (purpose)
		{
		case TextGenerationPurpose.ArtDescription:
			rootKeyword = "r_art_description";
			if (tale != null && !Rand.Chance(0.2f))
			{
				request.Includes.Add(RulePackDefOf.ArtDescriptionRoot_HasTale);
				request.IncludesBare.AddRange(tale.GetTextGenerationIncludes());
				request.Rules.AddRange(tale.GetTextGenerationRules(request.Constants));
			}
			else
			{
				request.Includes.Add(RulePackDefOf.ArtDescriptionRoot_Taleless);
				request.Includes.Add(RulePackDefOf.TalelessImages);
			}
			request.Includes.Add(RulePackDefOf.ArtDescriptionUtility_Global);
			break;
		case TextGenerationPurpose.ArtName:
			rootKeyword = "r_art_name";
			if (tale != null)
			{
				request.IncludesBare.AddRange(tale.GetTextGenerationIncludes());
				request.Rules.AddRange(tale.GetTextGenerationRules(request.Constants));
			}
			break;
		}
		string text = GrammarResolver.Resolve(rootKeyword, request, (tale != null) ? tale.def.defName : "null_tale");
		Rand.PopState();
		return text;
	}
}
