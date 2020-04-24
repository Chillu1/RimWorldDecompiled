using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NeedAuthority : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.needs.authority == null || !p.needs.authority.IsActive)
			{
				return ThoughtState.Inactive;
			}
			AuthorityCategory curCategory = p.needs.authority.CurCategory;
			if (curCategory == AuthorityCategory.Normal)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveAtStage((int)curCategory);
		}
	}
}
