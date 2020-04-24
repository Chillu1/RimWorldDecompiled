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
			List<Pair<List<StuffCategoryDef>, int>> list = new List<Pair<List<StuffCategoryDef>, int>>();
			foreach (SketchEntity entity in value.sketch.Entities)
			{
				SketchBuildable sketchBuildable = entity as SketchBuildable;
				if (sketchBuildable != null)
				{
					if (sketchBuildable.Buildable.MadeFromStuff && sketchBuildable.Stuff == null)
					{
						int num = FindStuffsIndexFor(sketchBuildable.Buildable, list);
						if (num < 0)
						{
							list.Add(new Pair<List<StuffCategoryDef>, int>(sketchBuildable.Buildable.stuffCategories, sketchBuildable.Buildable.costStuffCount));
						}
						else
						{
							list[num] = new Pair<List<StuffCategoryDef>, int>(list[num].First, list[num].Second + sketchBuildable.Buildable.costStuffCount);
						}
						if (sketchBuildable.Buildable.costList != null)
						{
							for (int i = 0; i < sketchBuildable.Buildable.costList.Count; i++)
							{
								ThingDefCountClass thingDefCountClass = sketchBuildable.Buildable.costList[i];
								if (!dictionary.TryGetValue(thingDefCountClass.thingDef, out int value2))
								{
									value2 = 0;
								}
								dictionary[thingDefCountClass.thingDef] = value2 + thingDefCountClass.count;
							}
						}
					}
					else
					{
						List<ThingDefCountClass> list2 = sketchBuildable.Buildable.CostListAdjusted(sketchBuildable.Stuff);
						for (int j = 0; j < list2.Count; j++)
						{
							if (!dictionary.TryGetValue(list2[j].thingDef, out int value3))
							{
								value3 = 0;
							}
							dictionary[list2[j].thingDef] = value3 + list2[j].count;
						}
					}
				}
			}
			float num2 = 0f;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pair<List<StuffCategoryDef>, int> item in list)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append((string)("  - " + "AnyOf".Translate() + ": " + item.First.Select((StuffCategoryDef x) => x.label).ToCommaList() + " x") + item.Second);
				num2 += GetCheapestStuffMarketValue(item.First, item.Second);
			}
			foreach (KeyValuePair<ThingDef, int> item2 in dictionary)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("  - " + GenLabel.ThingLabel(item2.Key, null, item2.Value).CapitalizeFirst());
				num2 += item2.Key.BaseMarketValue * (float)item2.Value;
			}
			slate.Set(storeAs.GetValue(slate), stringBuilder.ToString());
			if (!storeMarketValueAs.GetValue(slate).NullOrEmpty())
			{
				slate.Set(storeMarketValueAs.GetValue(slate), num2);
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
	}
}
