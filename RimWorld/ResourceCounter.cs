using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public sealed class ResourceCounter
	{
		private Map map;

		private Dictionary<ThingDef, int> countedAmounts = new Dictionary<ThingDef, int>();

		private static List<ThingDef> resources = new List<ThingDef>();

		public int Silver => GetCount(ThingDefOf.Silver);

		public float TotalHumanEdibleNutrition
		{
			get
			{
				float num = 0f;
				foreach (KeyValuePair<ThingDef, int> countedAmount in countedAmounts)
				{
					if (countedAmount.Key.IsNutritionGivingIngestible && countedAmount.Key.ingestible.HumanEdible)
					{
						num += countedAmount.Key.GetStatValueAbstract(StatDefOf.Nutrition) * (float)countedAmount.Value;
					}
				}
				return num;
			}
		}

		public Dictionary<ThingDef, int> AllCountedAmounts => countedAmounts;

		public static void ResetDefs()
		{
			resources.Clear();
			resources.AddRange(from def in DefDatabase<ThingDef>.AllDefs
				where def.CountAsResource
				orderby def.resourceReadoutPriority descending
				select def);
		}

		public ResourceCounter(Map map)
		{
			this.map = map;
			ResetResourceCounts();
		}

		public void ResetResourceCounts()
		{
			countedAmounts.Clear();
			for (int i = 0; i < resources.Count; i++)
			{
				countedAmounts.Add(resources[i], 0);
			}
		}

		public int GetCount(ThingDef rDef)
		{
			if (rDef.resourceReadoutPriority == ResourceCountPriority.Uncounted)
			{
				return 0;
			}
			if (countedAmounts.TryGetValue(rDef, out int value))
			{
				return value;
			}
			Log.Error("Looked for nonexistent key " + rDef + " in counted resources.");
			countedAmounts.Add(rDef, 0);
			return 0;
		}

		public int GetCountIn(ThingRequestGroup group)
		{
			int num = 0;
			foreach (KeyValuePair<ThingDef, int> countedAmount in countedAmounts)
			{
				if (group.Includes(countedAmount.Key))
				{
					num += countedAmount.Value;
				}
			}
			return num;
		}

		public int GetCountIn(ThingCategoryDef cat)
		{
			int num = 0;
			for (int i = 0; i < cat.childThingDefs.Count; i++)
			{
				num += GetCount(cat.childThingDefs[i]);
			}
			for (int j = 0; j < cat.childCategories.Count; j++)
			{
				if (!cat.childCategories[j].resourceReadoutRoot)
				{
					num += GetCountIn(cat.childCategories[j]);
				}
			}
			return num;
		}

		public void ResourceCounterTick()
		{
			if (Find.TickManager.TicksGame % 204 == 0)
			{
				UpdateResourceCounts();
			}
		}

		public void UpdateResourceCounts()
		{
			ResetResourceCounts();
			List<SlotGroup> allGroupsListForReading = map.haulDestinationManager.AllGroupsListForReading;
			for (int i = 0; i < allGroupsListForReading.Count; i++)
			{
				foreach (Thing heldThing in allGroupsListForReading[i].HeldThings)
				{
					Thing innerIfMinified = heldThing.GetInnerIfMinified();
					if (innerIfMinified.def.CountAsResource && ShouldCount(innerIfMinified))
					{
						countedAmounts[innerIfMinified.def] += innerIfMinified.stackCount;
					}
				}
			}
		}

		private bool ShouldCount(Thing t)
		{
			if (t.IsNotFresh())
			{
				return false;
			}
			return true;
		}
	}
}
