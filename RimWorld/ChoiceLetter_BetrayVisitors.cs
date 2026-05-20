using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

[Obsolete("Betray visitors is now a subquest of refugee quest.")]
public class ChoiceLetter_BetrayVisitors : ChoiceLetter
{
	public List<Pawn> pawns = new List<Pawn>();

	public Pawn asker;

	public bool requiresAliveAsker;

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
			if (requiresAliveAsker && (asker == null || asker.Dead))
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
			if (lookTargets.IsValid())
			{
				yield return base.Option_JumpToLocationAndPostpone;
			}
			if (quest != null && !quest.hidden)
			{
				yield return Option_ViewInQuestsTab("ViewRelatedQuest", postpone: true);
			}
			yield return base.Option_Close;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_References.Look(ref asker, "asker");
		Scribe_Values.Look(ref requiresAliveAsker, "requiresAliveAsker", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
