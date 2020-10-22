using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ChoiceLetter_AcceptVisitors : ChoiceLetter
	{
		public List<Pawn> pawns = new List<Pawn>();

		public string acceptedSignal;

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
				bool result = false;
				for (int i = 0; i < pawns.Count; i++)
				{
					if (CanStillAccept(pawns[i]))
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		private DiaOption Option_Accept
		{
			get
			{
				DiaOption diaOption = new DiaOption("AcceptButton".Translate());
				diaOption.action = delegate
				{
					pawns.RemoveAll((Pawn x) => !CanStillAccept(x));
					if (!acceptedSignal.NullOrEmpty())
					{
						object arg = ((pawns.Count == 1) ? ((object)pawns[0]) : ((object)pawns));
						Find.SignalManager.SendSignal(new Signal(acceptedSignal, arg.Named("SUBJECT")));
					}
					Find.LetterStack.RemoveLetter(this);
				};
				diaOption.resolveTree = true;
				bool flag = false;
				for (int i = 0; i < pawns.Count; i++)
				{
					if (CanStillAccept(pawns[i]))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					diaOption.Disable(null);
				}
				return diaOption;
			}
		}

		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				if (!base.ArchivedOnly)
				{
					yield return Option_Accept;
					yield return base.Option_Reject;
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

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
			Scribe_Values.Look(ref acceptedSignal, "acceptedSignal");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				pawns.RemoveAll((Pawn x) => x == null);
			}
		}

		private bool CanStillAccept(Pawn p)
		{
			if (p.DestroyedOrNull() || !p.SpawnedOrAnyParentSpawned)
			{
				return false;
			}
			if (p.CurJob != null && p.CurJob.exitMapOnArrival)
			{
				return false;
			}
			return true;
		}
	}
}
