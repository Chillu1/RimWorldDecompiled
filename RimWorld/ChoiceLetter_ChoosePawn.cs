using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ChoiceLetter_ChoosePawn : ChoiceLetter
{
	public List<Pawn> pawns = new List<Pawn>();

	public string chosenPawnSignal;

	public override bool CanDismissWithRightClick => false;

	public override bool CanShowInLetterStack
	{
		get
		{
			if (!base.CanShowInLetterStack)
			{
				return false;
			}
			if (chosenPawnSignal.NullOrEmpty())
			{
				return false;
			}
			bool result = false;
			for (int i = 0; i < pawns.Count; i++)
			{
				if (!pawns[i].DestroyedOrNull())
				{
					result = true;
					break;
				}
			}
			return result;
		}
	}

	public override IEnumerable<DiaOption> Choices
	{
		get
		{
			if (!base.ArchivedOnly)
			{
				for (int i = 0; i < pawns.Count; i++)
				{
					if (!pawns[i].DestroyedOrNull())
					{
						yield return Option_ChoosePawn(pawns[i]);
					}
				}
				yield return base.Option_Postpone;
			}
			else
			{
				yield return base.Option_Close;
			}
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

	private DiaOption Option_ChoosePawn(Pawn p)
	{
		return new DiaOption(p.LabelCap)
		{
			action = delegate
			{
				if (!chosenPawnSignal.NullOrEmpty())
				{
					Find.SignalManager.SendSignal(new Signal(chosenPawnSignal, p.Named("CHOSEN")));
				}
				Find.LetterStack.RemoveLetter(this);
			},
			resolveTree = true
		};
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		Scribe_Values.Look(ref chosenPawnSignal, "chosenPawnSignal");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}
}
