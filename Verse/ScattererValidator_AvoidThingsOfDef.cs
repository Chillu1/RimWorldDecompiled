using System.Collections.Generic;

namespace Verse;

public class ScattererValidator_AvoidThingsOfDef : ScattererValidator
{
	public int radius = 1;

	public List<ThingDef> thingsToAvoid = new List<ThingDef>();

	public override bool Allows(IntVec3 c, Map map)
	{
		foreach (ThingDef item in thingsToAvoid)
		{
			foreach (Thing item2 in map.listerThings.ThingsOfDef(item))
			{
				if (c.InHorDistOf(item2.Position, radius))
				{
					return false;
				}
			}
		}
		return true;
	}
}
