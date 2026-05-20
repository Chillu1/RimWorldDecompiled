using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_ThroneroomInvalidConfiguration : Alert
{
	private string validationInfo;

	public Alert_ThroneroomInvalidConfiguration()
	{
		defaultLabel = "ThroneroomInvalidConfiguration".Translate();
		defaultExplanation = "ThroneroomInvalidConfigurationDesc".Translate();
		requireRoyalty = true;
	}

	public override TaggedString GetExplanation()
	{
		return base.GetExplanation() + "\n\n" + validationInfo;
	}

	public override AlertReport GetReport()
	{
		List<Map> maps = Find.Maps;
		for (int i = 0; i < maps.Count; i++)
		{
			foreach (Thing item in maps[i].listerThings.ThingsInGroup(ThingRequestGroup.Throne))
			{
				if (!item.Fogged() && item.Faction == Faction.OfPlayer)
				{
					validationInfo = RoomRoleWorker_ThroneRoom.Validate(item.GetRoom());
					if (validationInfo != null)
					{
						return AlertReport.CulpritIs(item);
					}
				}
			}
		}
		return false;
	}
}
