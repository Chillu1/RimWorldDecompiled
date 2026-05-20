using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public abstract class QuestPart_RequirementsToAccept : QuestPart
{
	public virtual IEnumerable<GlobalTargetInfo> Culprits => Enumerable.Empty<GlobalTargetInfo>();

	public virtual bool ShowInRequirementBox => true;

	public abstract AcceptanceReport CanAccept();

	public virtual bool CanPawnAccept(Pawn p)
	{
		return true;
	}
}
