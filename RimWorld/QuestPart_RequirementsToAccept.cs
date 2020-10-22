using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public abstract class QuestPart_RequirementsToAccept : QuestPart
	{
		public virtual IEnumerable<GlobalTargetInfo> Culprits
		{
			get
			{
				yield break;
			}
		}

		public abstract AcceptanceReport CanAccept();

		public virtual bool CanPawnAccept(Pawn p)
		{
			return true;
		}
	}
}
