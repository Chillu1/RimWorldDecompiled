using Verse;

namespace RimWorld
{
	public abstract class QuestPart_RequirementsToAccept : QuestPart
	{
		public abstract AcceptanceReport CanAccept();

		public virtual bool CanPawnAccept(Pawn p)
		{
			return true;
		}
	}
}
