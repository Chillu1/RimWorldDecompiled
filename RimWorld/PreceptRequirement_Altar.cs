using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class PreceptRequirement_Altar : PreceptRequirement
{
	public override bool Met(List<Precept> precepts)
	{
		for (int i = 0; i < precepts.Count; i++)
		{
			if (precepts[i] is Precept_Building precept_Building && precept_Building.ThingDef.isAltar)
			{
				return true;
			}
		}
		return false;
	}

	public override Precept MakePrecept(Ideo ideo)
	{
		PreceptDef ideoBuilding = PreceptDefOf.IdeoBuilding;
		Precept_Building obj = (Precept_Building)PreceptMaker.MakePrecept(ideoBuilding);
		obj.ideo = ideo;
		obj.ThingDef = (from b in ideoBuilding.Worker.ThingDefsForIdeo(ideo, null)
			where b.def.isAltar
			select b).RandomElement().def;
		return obj;
	}
}
