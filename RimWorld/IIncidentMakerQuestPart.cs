using System.Collections.Generic;

namespace RimWorld
{
	public interface IIncidentMakerQuestPart
	{
		IEnumerable<FiringIncident> MakeIntervalIncidents();
	}
}
