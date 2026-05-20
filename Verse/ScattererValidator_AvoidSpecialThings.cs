using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class ScattererValidator_AvoidSpecialThings : ScattererValidator
{
	private static Dictionary<ThingDef, float> thingsToAvoid;

	public override bool Allows(IntVec3 c, Map map)
	{
		return IsValid(c, map);
	}

	public static bool IsValid(IntVec3 c, Map map)
	{
		if (thingsToAvoid == null)
		{
			thingsToAvoid = new Dictionary<ThingDef, float>
			{
				{
					ThingDefOf.SteamGeyser,
					3f
				},
				{
					ThingDefOf.AncientCryptosleepCasket,
					30f
				}
			};
			if (ModsConfig.RoyaltyActive)
			{
				thingsToAvoid.Add(ThingDefOf.Plant_TreeAnima, 5f);
			}
			if (ModsConfig.IdeologyActive)
			{
				thingsToAvoid.Add(ThingDefOf.ArchonexusCore, 20f);
				thingsToAvoid.Add(ThingDefOf.GrandArchotechStructure, 20f);
				thingsToAvoid.Add(ThingDefOf.MajorArchotechStructure, 20f);
			}
			if (ModsConfig.BiotechActive)
			{
				thingsToAvoid.Add(ThingDefOf.AncientExostriderRemains, 6f);
			}
			if (ModsConfig.AnomalyActive)
			{
				thingsToAvoid.Add(ThingDefOf.VoidMonolith, 10f);
				thingsToAvoid.Add(ThingDefOf.VoidStructure, 5f);
			}
			if (ModsConfig.OdysseyActive)
			{
				thingsToAvoid.Add(ThingDefOf.GeothermalVent, 10f);
				thingsToAvoid.Add(ThingDefOf.AncientSmokeVent, 19f);
				thingsToAvoid.Add(ThingDefOf.AncientToxVent, 19f);
				thingsToAvoid.Add(ThingDefOf.AncientHeatVent, 19f);
			}
		}
		foreach (KeyValuePair<ThingDef, float> item in thingsToAvoid)
		{
			if (item.Key == null)
			{
				continue;
			}
			foreach (Thing item2 in map.listerThings.ThingsOfDef(item.Key))
			{
				if (c.InHorDistOf(item2.Position, item.Value + item2.def.size.Magnitude / 2f + item.Key.size.Magnitude / 2f))
				{
					return false;
				}
			}
		}
		return true;
	}
}
