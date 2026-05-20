using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_SetItemStashContents : QuestNode
{
	public class ThingCategoryCount
	{
		public ThingCategoryDef category;

		public IntRange amount;

		public bool allowDuplicates = true;
	}

	public SlateRef<IEnumerable<ThingDef>> items;

	public SlateRef<List<ThingCategoryCount>> categories;

	private static List<ThingDef> tmpItems = new List<ThingDef>();

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		slate.Set("itemStashThings", GetContents(slate));
	}

	private IEnumerable<ThingDef> GetContents(Slate slate)
	{
		IEnumerable<ThingDef> value = items.GetValue(slate);
		if (value != null)
		{
			foreach (ThingDef item in value)
			{
				yield return item;
			}
		}
		List<ThingCategoryCount> value2 = categories.GetValue(slate);
		if (value2 == null)
		{
			yield break;
		}
		foreach (ThingCategoryCount c in value2)
		{
			try
			{
				int amt = Mathf.Max(c.amount.RandomInRange, 1);
				for (int i = 0; i < amt; i++)
				{
					if (DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.thingCategories != null && x.thingCategories.Contains(c.category) && (c.allowDuplicates || !tmpItems.Contains(x))).TryRandomElement(out var result))
					{
						tmpItems.Add(result);
						yield return result;
					}
				}
			}
			finally
			{
				tmpItems.Clear();
			}
		}
	}
}
