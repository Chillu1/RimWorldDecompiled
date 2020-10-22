using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ChoiceLetter_BetrayVisitors : ChoiceLetter
	{
		public List<Pawn> pawns = new List<Pawn>();

		public override bool CanDismissWithRightClick => false;

		public override bool CanShowInLetterStack
		{
			get
			{
				if (!base.CanShowInLetterStack)
				{
					return false;
				}
				if (quest == null || quest.State != QuestState.Ongoing)
				{
					return false;
				}
				for (int i = 0; i < pawns.Count; i++)
				{
					if (!pawns[i].Spawned && !pawns[i].Destroyed)
					{
						return false;
					}
				}
				return true;
			}
		}

		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				yield return base.Option_Close;
				if (lookTargets.IsValid())
				{
					yield return base.Option_JumpToLocationAndPostpone;
				}
				if (quest != null && !quest.hidden)
				{
					yield return Option_ViewInQuestsTab("ViewRelatedQuest", postpone: true);
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}
	}
}
