using System.Collections.Generic;
using Verse.Grammar;

namespace Verse;

public static class PlayLogEntryUtility
{
	public static IEnumerable<Rule> RulesForOptionalWeapon(string prefix, ThingDef weaponDef, ThingDef projectileDef)
	{
		if (weaponDef == null)
		{
			yield break;
		}
		foreach (Rule item in GrammarUtility.RulesForDef(prefix, weaponDef))
		{
			yield return item;
		}
		ThingDef thingDef = projectileDef;
		if (thingDef == null && !weaponDef.Verbs.NullOrEmpty())
		{
			thingDef = weaponDef.Verbs[0].defaultProjectile;
		}
		if (thingDef == null)
		{
			yield break;
		}
		foreach (Rule item2 in GrammarUtility.RulesForDef(prefix + "_projectile", thingDef))
		{
			yield return item2;
		}
	}

	public static IEnumerable<Rule> RulesForDamagedParts(string prefix, BodyDef body, List<BodyPartRecord> bodyParts, List<bool> bodyPartsDestroyed, Dictionary<string, string> constants)
	{
		if (bodyParts != null)
		{
			int destroyedIndex = 0;
			int damagedIndex = 0;
			int i = 0;
			while (i < bodyParts.Count)
			{
				yield return new Rule_String(string.Format(prefix + "{0}_label", i), bodyParts[i].Label);
				yield return new Rule_String(string.Format(prefix + "{0}_definite", i), Find.ActiveLanguageWorker.WithDefiniteArticle(bodyParts[i].Label));
				yield return new Rule_String(string.Format(prefix + "{0}_indefinite", i), Find.ActiveLanguageWorker.WithIndefiniteArticle(bodyParts[i].Label));
				constants[string.Format(prefix + "{0}_destroyed", i)] = bodyPartsDestroyed[i].ToString();
				constants[string.Format(prefix + "{0}_gender", i)] = LanguageDatabase.activeLanguage.ResolveGender(bodyParts[i].Label).ToString();
				if (bodyPartsDestroyed[i])
				{
					yield return new Rule_String(string.Format(prefix + "_destroyed{0}_label", destroyedIndex), bodyParts[i].Label);
					yield return new Rule_String(string.Format(prefix + "_destroyed{0}_definite", destroyedIndex), Find.ActiveLanguageWorker.WithDefiniteArticle(bodyParts[i].Label));
					yield return new Rule_String(string.Format(prefix + "_destroyed{0}_indefinite", destroyedIndex), Find.ActiveLanguageWorker.WithIndefiniteArticle(bodyParts[i].Label));
					constants[$"{prefix}_destroyed{destroyedIndex}_outside"] = (bodyParts[i].depth == BodyPartDepth.Outside).ToString();
					constants[$"{prefix}_destroyed{destroyedIndex}_gender"] = LanguageDatabase.activeLanguage.ResolveGender(bodyParts[i].Label).ToString();
					destroyedIndex++;
				}
				else
				{
					yield return new Rule_String(string.Format(prefix + "_damaged{0}_label", damagedIndex), bodyParts[i].Label);
					yield return new Rule_String(string.Format(prefix + "_damaged{0}_definite", damagedIndex), Find.ActiveLanguageWorker.WithDefiniteArticle(bodyParts[i].Label));
					yield return new Rule_String(string.Format(prefix + "_damaged{0}_indefinite", damagedIndex), Find.ActiveLanguageWorker.WithIndefiniteArticle(bodyParts[i].Label));
					constants[$"{prefix}_damaged{damagedIndex}_outside"] = (bodyParts[i].depth == BodyPartDepth.Outside).ToString();
					constants[$"{prefix}_damaged{damagedIndex}_gender"] = LanguageDatabase.activeLanguage.ResolveGender(bodyParts[i].Label).ToString();
					damagedIndex++;
				}
				int num = i + 1;
				i = num;
			}
			constants[prefix + "_count"] = bodyParts.Count.ToString();
			constants[prefix + "_destroyed_count"] = destroyedIndex.ToString();
			constants[prefix + "_damaged_count"] = damagedIndex.ToString();
		}
		else
		{
			constants[prefix + "_count"] = "0";
			constants[prefix + "_destroyed_count"] = "0";
			constants[prefix + "_damaged_count"] = "0";
		}
	}
}
