using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SlotGroup
	{
		public ISlotGroupParent parent;

		private Map Map => parent.Map;

		public StorageSettings Settings => parent.GetStoreSettings();

		public IEnumerable<Thing> HeldThings
		{
			get
			{
				List<IntVec3> cellsList = CellsList;
				Map map = Map;
				for (int i = 0; i < cellsList.Count; i++)
				{
					List<Thing> thingList = map.thingGrid.ThingsListAt(cellsList[i]);
					for (int j = 0; j < thingList.Count; j++)
					{
						if (thingList[j].def.EverStorable(willMinifyIfPossible: false))
						{
							yield return thingList[j];
						}
					}
				}
			}
		}

		public List<IntVec3> CellsList => parent.AllSlotCellsList();

		public IEnumerator<IntVec3> GetEnumerator()
		{
			List<IntVec3> cellsList = CellsList;
			for (int i = 0; i < cellsList.Count; i++)
			{
				yield return cellsList[i];
			}
		}

		public SlotGroup(ISlotGroupParent parent)
		{
			this.parent = parent;
		}

		public void Notify_AddedCell(IntVec3 c)
		{
			Map.haulDestinationManager.SetCellFor(c, this);
			Map.listerHaulables.RecalcAllInCell(c);
			Map.listerMergeables.RecalcAllInCell(c);
		}

		public void Notify_LostCell(IntVec3 c)
		{
			Map.haulDestinationManager.ClearCellFor(c, this);
			Map.listerHaulables.RecalcAllInCell(c);
			Map.listerMergeables.RecalcAllInCell(c);
		}

		public override string ToString()
		{
			if (parent != null)
			{
				return parent.ToString();
			}
			return "NullParent";
		}
	}
}
