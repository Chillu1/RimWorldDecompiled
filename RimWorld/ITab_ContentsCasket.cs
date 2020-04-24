using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ITab_ContentsCasket : ITab_ContentsBase
	{
		private List<Thing> listInt = new List<Thing>();

		public override IList<Thing> container
		{
			get
			{
				Building_Casket building_Casket = base.SelThing as Building_Casket;
				listInt.Clear();
				if (building_Casket != null && building_Casket.ContainedThing != null)
				{
					listInt.Add(building_Casket.ContainedThing);
				}
				return listInt;
			}
		}

		public ITab_ContentsCasket()
		{
			labelKey = "TabCasketContents";
			containedItemsKey = "ContainedItems";
			canRemoveThings = false;
		}
	}
}
