using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class DrugPolicy : IExposable, ILoadReferenceable
	{
		public int uniqueId;

		public string label;

		private List<DrugPolicyEntry> entriesInt;

		public int Count => entriesInt.Count;

		public DrugPolicyEntry this[int index]
		{
			get
			{
				return entriesInt[index];
			}
			set
			{
				entriesInt[index] = value;
			}
		}

		public DrugPolicyEntry this[ThingDef drugDef]
		{
			get
			{
				for (int i = 0; i < entriesInt.Count; i++)
				{
					if (entriesInt[i].drug == drugDef)
					{
						return entriesInt[i];
					}
				}
				throw new ArgumentException();
			}
		}

		public DrugPolicy()
		{
		}

		public DrugPolicy(int uniqueId, string label)
		{
			this.uniqueId = uniqueId;
			this.label = label;
			InitializeIfNeeded();
		}

		public void InitializeIfNeeded()
		{
			if (entriesInt != null)
			{
				return;
			}
			entriesInt = new List<DrugPolicyEntry>();
			List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].category == ThingCategory.Item && allDefsListForReading[i].IsDrug)
				{
					DrugPolicyEntry drugPolicyEntry = new DrugPolicyEntry();
					drugPolicyEntry.drug = allDefsListForReading[i];
					drugPolicyEntry.allowedForAddiction = true;
					entriesInt.Add(drugPolicyEntry);
				}
			}
			entriesInt.SortBy((DrugPolicyEntry e) => e.drug.GetCompProperties<CompProperties_Drug>().listOrder);
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref uniqueId, "uniqueId", 0);
			Scribe_Values.Look(ref label, "label");
			Scribe_Collections.Look(ref entriesInt, "drugs", LookMode.Deep);
		}

		public string GetUniqueLoadID()
		{
			return "DrugPolicy_" + label + uniqueId.ToString();
		}
	}
}
