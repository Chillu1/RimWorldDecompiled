using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetMonumentRequiredResourcesString : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	[NoTranslate]
	public SlateRef<string> storeMarketValueAs;

	public SlateRef<MonumentMarker> monumentMarker;

	protected override bool TestRunInt(Slate slate)
	{
		DoWork(slate);
		return true;
	}

	protected override void RunInt()
	{
		DoWork(QuestGen.slate);
	}

	private void DoWork(Slate slate)
	{
		MonumentMarker value = monumentMarker.GetValue(slate);
		if (value == null)
		{
			if (!storeMarketValueAs.GetValue(slate).NullOrEmpty())
			{
				slate.Set(storeMarketValueAs.GetValue(slate), 0f);
			}
			return;
		}
		Dictionary<ThingDef, int> dictionary = new Dictionary<ThingDef, int>();
		List<Pair<List<StuffCategoryDef>, int>> list = new List<Pair<List<StuffCategoryDef>, int>>();
		List<List<Pair<ThingDef, int>>> list2 = new List<List<Pair<ThingDef, int>>>();
		int num = value.sketch.Entities.Where((SketchEntity x) => x is SketchTerrain sketchTerrain && sketchTerrain.treatSimilarAsSame).Count();
		foreach (SketchEntity entity in value.sketch.Entities)
		{
			if (!(entity is SketchBuildable sketchBuildable))
			{
				continue;
			}
			if (sketchBuildable.Buildable.MadeFromStuff && sketchBuildable.Stuff == null)
			{
				int num2 = FindStuffsIndexFor(sketchBuildable.Buildable, list);
				if (num2 < 0)
				{
					list.Add(new Pair<List<StuffCategoryDef>, int>(sketchBuildable.Buildable.stuffCategories, sketchBuildable.Buildable.CostStuffCount));
				}
				else
				{
					list[num2] = new Pair<List<StuffCategoryDef>, int>(list[num2].First, list[num2].Second + sketchBuildable.Buildable.CostStuffCount);
				}
				if (sketchBuildable.Buildable.CostList == null)
				{
					continue;
				}
				for (int num3 = 0; num3 < sketchBuildable.Buildable.CostList.Count; num3++)
				{
					ThingDefCountClass thingDefCountClass = sketchBuildable.Buildable.CostList[num3];
					if (!dictionary.TryGetValue(thingDefCountClass.thingDef, out var value2))
					{
						value2 = 0;
					}
					dictionary[thingDefCountClass.thingDef] = value2 + thingDefCountClass.count;
				}
				continue;
			}
			SketchTerrain st = sketchBuildable as SketchTerrain;
			if (st != null && st.treatSimilarAsSame)
			{
				foreach (TerrainDef item in DefDatabase<TerrainDef>.AllDefs.Where((TerrainDef x) => st.IsSameOrSimilar(x)))
				{
					if (item.CostList.NullOrEmpty())
					{
						continue;
					}
					List<Pair<ThingDef, int>> list3 = new List<Pair<ThingDef, int>>();
					foreach (ThingDefCountClass cost in item.CostList)
					{
						list3.Add(new Pair<ThingDef, int>(cost.thingDef, cost.count * num));
					}
					if (!list2.Any((List<Pair<ThingDef, int>> x) => x.SetsEqual(list3)))
					{
						list2.Add(list3);
					}
				}
				continue;
			}
			List<ThingDefCountClass> list4 = sketchBuildable.Buildable.CostListAdjusted(sketchBuildable.Stuff);
			for (int num4 = 0; num4 < list4.Count; num4++)
			{
				if (!dictionary.TryGetValue(list4[num4].thingDef, out var value3))
				{
					value3 = 0;
				}
				dictionary[list4[num4].thingDef] = value3 + list4[num4].count;
			}
		}
		float num5 = 0f;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Pair<List<StuffCategoryDef>, int> item2 in list)
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(string.Concat("  - " + "AnyOf".Translate() + ": " + item2.First.Select((StuffCategoryDef x) => x.label).ToCommaList() + " x", item2.Second.ToString()));
			num5 += GetCheapestStuffMarketValue(item2.First, item2.Second);
		}
		foreach (KeyValuePair<ThingDef, int> item3 in dictionary)
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append("  - " + GenLabel.ThingLabel(item3.Key, null, item3.Value).CapitalizeFirst());
			num5 += item3.Key.BaseMarketValue * (float)item3.Value;
		}
		if (list2.Any())
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append("  - " + "AnyOf".Translate() + ":");
			foreach (List<Pair<ThingDef, int>> item4 in list2)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("    - " + item4.Select((Pair<ThingDef, int> x) => x.First.label + " x" + x.Second).ToCommaList());
			}
			num5 += GetCheapestThingMarketValue(list2);
		}
		slate.Set(storeAs.GetValue(slate), stringBuilder.ToString());
		if (!storeMarketValueAs.GetValue(slate).NullOrEmpty())
		{
			slate.Set(storeMarketValueAs.GetValue(slate), num5);
		}
	}

	private int FindStuffsIndexFor(BuildableDef buildable, List<Pair<List<StuffCategoryDef>, int>> anyOf)
	{
		for (int i = 0; i < anyOf.Count; i++)
		{
			if (anyOf[i].First.SetsEqual(buildable.stuffCategories))
			{
				return i;
			}
		}
		return -1;
	}

	private float GetCheapestStuffMarketValue(List<StuffCategoryDef> categories, int count)
	{
		if (!categories.Any())
		{
			return 0f;
		}
		float num = float.MaxValue;
		for (int i = 0; i < categories.Count; i++)
		{
			foreach (ThingDef item in GenStuff.AllowedStuffs(categories))
			{
				int num2 = Mathf.Max(Mathf.RoundToInt((float)count / item.VolumePerUnit), 1);
				float num3 = item.BaseMarketValue * (float)num2;
				if (num3 < num)
				{
					num = num3;
				}
			}
		}
		return num;
	}

	private float GetCheapestThingMarketValue(List<List<Pair<ThingDef, int>>> costs)
	{
		if (!costs.Any())
		{
			return 0f;
		}
		float num = float.MaxValue;
		for (int i = 0; i < costs.Count; i++)
		{
			float num2 = 0f;
			for (int j = 0; j < costs[i].Count; j++)
			{
				num2 += costs[i][j].First.BaseMarketValue * (float)costs[i][j].Second;
			}
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}
}
