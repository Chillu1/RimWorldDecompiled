using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_EntityNeedsTend : Alert
{
	private List<Pawn> needingEntitiesResult = new List<Pawn>();

	private StringBuilder sb = new StringBuilder();

	private List<Pawn> NeedingEntities
	{
		get
		{
			needingEntitiesResult.Clear();
			foreach (Map map in Find.Maps)
			{
				foreach (Thing item in map.listerThings.ThingsInGroup(ThingRequestGroup.EntityHolder))
				{
					Pawn tendableEnityFromPotentialPlatform = WorkGiver_TendEntity.GetTendableEnityFromPotentialPlatform(item);
					if (tendableEnityFromPotentialPlatform != null && HealthAIUtility.ShouldBeTendedNowByPlayerUrgent(tendableEnityFromPotentialPlatform))
					{
						needingEntitiesResult.Add(tendableEnityFromPotentialPlatform);
					}
				}
			}
			return needingEntitiesResult;
		}
	}

	public Alert_EntityNeedsTend()
	{
		defaultLabel = "EntityNeedsTreatment".Translate();
		defaultPriority = AlertPriority.High;
	}

	public override TaggedString GetExplanation()
	{
		sb.Length = 0;
		foreach (Pawn item in needingEntitiesResult)
		{
			sb.AppendLine("  - " + item.NameShortColored.Resolve());
		}
		return "EntityNeedsTreatmentDesc".Translate(sb.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(NeedingEntities);
	}
}
