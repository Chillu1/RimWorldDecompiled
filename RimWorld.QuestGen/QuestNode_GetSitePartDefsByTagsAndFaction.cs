using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetSitePartDefsByTagsAndFaction : QuestNode
{
	public class SitePartOption
	{
		[NoTranslate]
		public string tag;

		public float chance = 1f;
	}

	public SlateRef<IEnumerable<SitePartOption>> sitePartsTags;

	[NoTranslate]
	public SlateRef<string> storeAs;

	[NoTranslate]
	public SlateRef<string> storeFactionAs;

	public SlateRef<Thing> mustBeHostileToFactionOf;

	private static List<string> tmpTags = new List<string>();

	protected override bool TestRunInt(Slate slate)
	{
		return TrySetVars(slate);
	}

	protected override void RunInt()
	{
		if (!TrySetVars(QuestGen.slate))
		{
			Log.Error("Could not resolve site parts.");
		}
	}

	private bool TrySetVars(Slate slate)
	{
		float points = slate.Get("points", 0f);
		Faction factionToUse = slate.Get<Faction>("enemyFaction");
		Pawn asker = slate.Get<Pawn>("asker");
		Thing mustBeHostileToFactionOfResolved = mustBeHostileToFactionOf.GetValue(slate);
		for (int i = 0; i < 2; i++)
		{
			tmpTags.Clear();
			foreach (SitePartOption item in sitePartsTags.GetValue(slate))
			{
				if (Rand.Chance(item.chance) && (i != 1 || !(item.chance < 1f)))
				{
					tmpTags.Add(item.tag);
				}
			}
			if (!SiteMakerHelper.TryFindSiteParams_MultipleSiteParts(tmpTags.Where((string x) => x != null).Select(delegate(string x)
			{
				IEnumerable<SitePartDef> enumerable = SiteMakerHelper.SitePartDefsWithTag(x);
				IEnumerable<SitePartDef> enumerable2 = enumerable.Where((SitePartDef y) => points >= y.minThreatPoints);
				return (!enumerable2.Any()) ? enumerable : enumerable2;
			}), out var siteParts, out var faction, factionToUse, disallowNonHostileFactions: true, delegate(Faction x)
			{
				if (asker != null && asker.Faction != null && asker.Faction == x)
				{
					return false;
				}
				return (mustBeHostileToFactionOfResolved == null || mustBeHostileToFactionOfResolved.Faction == null || (x != mustBeHostileToFactionOfResolved.Faction && x.HostileTo(mustBeHostileToFactionOfResolved.Faction))) ? true : false;
			}))
			{
				continue;
			}
			slate.Set(storeAs.GetValue(slate), siteParts);
			slate.Set("sitePartCount", siteParts.Count);
			if (QuestGen.Working)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				for (int num = 0; num < siteParts.Count; num++)
				{
					dictionary[siteParts[num].defName + "_exists"] = "True";
				}
				QuestGen.AddQuestDescriptionConstants(dictionary);
			}
			if (!storeFactionAs.GetValue(slate).NullOrEmpty())
			{
				slate.Set(storeFactionAs.GetValue(slate), faction);
			}
			return true;
		}
		return false;
	}
}
