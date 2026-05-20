using System.Collections.Generic;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class RitualCancellationTrigger
	{
		public abstract IEnumerable<Trigger> CancellationTriggers(RitualRoleAssignments assignments);
	}
}
