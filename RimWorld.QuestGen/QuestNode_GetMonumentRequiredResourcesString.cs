using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen
{
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
			List<Pair<List<StuffCategoryDef>, int>> list2 = new List<Pair<List<StuffCategoryDef>, int>>();
			List<List<Pair<ThingDef, int>>> list3 = new List<List<Pair<ThingDef, int>>>();
			SketchTerrain sketchTerrain;
			int num = value.sketch.Entities.Where((SketchEntity x) => (sketchTerrain = (x as SketchTerrain)) != null && sketchTerrain.treatSimilarAsSame).Count();
			foreach (SketchEntity entity in value.sketch.Entities)
			{
				SketchBuildable sketchBuildable = entity as SketchBuildable;
				if (sketchBuildable == null)
				{
					continue;
				}
				if (sketchBuildable.Buildable.MadeFromStuff && sketchBuildable.Stuff == null)
				{
					int num2 = FindStuffsIndexFor(sketchBuildable.Buildable, list2);
					if (num2 < 0)
					{
						list2.Add(new Pair<List<StuffCategoryDef>, int>(sketchBuildable.Buildable.stuffCategories, sketchBuildable.Buildable.costStuffCount));
					}
					else
					{
						list2[num2] = new Pair<List<StuffCategoryDef>, int>(list2[num2].First, list2[num2].Second + sketchBuildable.Buildable.costStuffCount);
					}
					if (sketchBuildable.Buildable.costList == null)
					{
						continue;
					}
					for (int i = 0; i < sketchBuildable.Buildable.costList.Count; i++)
					{
						ThingDefCountClass thingDefCountClass = sketchBuildable.Buildable.costList[i];
						if (!dictionary.TryGetValue(thingDefCountClass.thingDef, out int value2))
						{
							value2 = 0;
						}
						dictionary[thingDefCountClass.thingDef] = value2 + thingDefCountClass.count;
					}
					continue;
				}
				SketchTerrain st;
				if ((st = (sketchBuildable as SketchTerrain)) != null && st.treatSimilarAsSame)
				{
					foreach (TerrainDef item in DefDatabase<TerrainDef>.AllDefs.Where((TerrainDef x) => st.IsSameOrSimilar(x)))
					{
						if (item.costList.NullOrEmpty())
						{
							continue;
						}
						List<Pair<ThingDef, int>> list = new List<Pair<ThingDef, int>>();
						foreach (ThingDefCountClass cost in item.costList)
						{
							list.Add(new Pair<ThingDef, int>(cost.thingDef, cost.count * num));
						}
						if (!list3.Any((List<Pair<ThingDef, int>> x) => x.ListsEqualIgnoreOrder(list)))
						{
							list3.Add(list);
						}
					}
					continue;
				}
				List<ThingDefCountClass> list4 = sketchBuildable.Buildable.CostListAdjusted(sketchBuildable.Stuff);
				for (int j = 0; j < list4.Count; j++)
				{
					if (!dictionary.TryGetValue(list4[j].thingDef, out int value3))
					{
						value3 = 0;
					}
					dictionary[list4[j].thingDef] = value3 + list4[j].count;
				}
			}
			float num3 = 0f;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pair<List<StuffCategoryDef>, int> item2 in list2)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append((string)("  - " + "AnyOf".Translate() + ": " + item2.First.Select((StuffCategoryDef x) => x.label).ToCommaList() + " x") + item2.Second);
				num3 += GetCheapestStuffMarketValue(item2.First, item2.Second);
			}
			foreach (KeyValuePair<ThingDef, int> item3 in dictionary)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("  - " + GenLabel.ThingLabel(item3.Key, null, item3.Value).CapitalizeFirst());
				num3 += item3.Key.BaseMarketValue * (float)item3.Value;
			}
			if (list3.Any())
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("  - " + "AnyOf".Translate() + ":");
				foreach (List<Pair<ThingDef, int>> item4 in list3)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("    - " + item4.Select((Pair<ThingDef, int> x) => x.First.label + " x" + x.Second).ToCommaList());
				}
				num3 += GetCheapestThingMarketValue(list3);
			}
			slate.Set(storeAs.GetValue(slate), stringBuilder.ToString());
			if (!storeMarketValueAs.GetValue(slate).NullOrEmpty())
			{
				slate.Set(storeMarketValueAs.GetValue(slate), num3);
			}
		}

		private int FindStuffsIndexFor(BuildableDef buildable, List<Pair<List<StuffCategoryDef>, int>> anyOf)
		{
			for (int i = 0; i < anyOf.Count; i++)
			{
				if (anyOf[i].First.ListsEqualIgnoreOrder(buildable.stuffCategories))
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
}
