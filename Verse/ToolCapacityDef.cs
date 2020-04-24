using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class ToolCapacityDef : Def
	{
		public IEnumerable<ManeuverDef> Maneuvers => DefDatabase<ManeuverDef>.AllDefsListForReading.Where((ManeuverDef x) => x.requiredCapacity == this);

		public IEnumerable<VerbProperties> VerbsProperties => Maneuvers.Select((ManeuverDef x) => x.verb);
	}
}
