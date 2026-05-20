using System.Collections.Generic;

namespace RimWorld
{
	public class RitualObligationTargetWorker_ConsumableBuilding : RitualObligationTargetWorker_ThingDef
	{
		public RitualObligationTargetWorker_ConsumableBuilding()
		{
		}

		public RitualObligationTargetWorker_ConsumableBuilding(RitualObligationTargetFilterDef def)
			: base(def)
		{
		}

		public override List<string> MissingTargetBuilding(Ideo ideo)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < def.thingDefs.Count; i++)
			{
				if (ideo.HasPreceptForBuilding(def.thingDefs[i]))
				{
					return base.MissingTargetBuilding(ideo);
				}
				list.Add(def.thingDefs[i].label);
			}
			return list;
		}
	}
}
