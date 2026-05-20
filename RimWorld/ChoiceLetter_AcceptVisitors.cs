using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class ChoiceLetter_AcceptVisitors : ChoiceLetter
{
	public List<Pawn> pawns = new List<Pawn>();

	public string acceptedSignal;

	public string rejectedSignal;

	public bool charity;

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
					foreach (Pawn pawn in pawns)
					{
						QuestUtility.SendQuestTargetSignals(pawn.questTags, "Recruited", pawn.Named("SUBJECT"));
					}
					object arg = ((pawns.Count == 1) ? ((object)pawns[0]) : ((object)pawns));
					Find.SignalManager.SendSignal(new Signal(acceptedSignal, arg.Named("SUBJECT")));
				}
				Find.LetterStack.RemoveLetter(this);
			};
			diaOption.resolveTree = true;
			bool flag = false;
			for (int num = 0; num < pawns.Count; num++)
			{
				if (CanStillAccept(pawns[num]))
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

	private DiaOption Option_RejectWithCharityConfirmation => new DiaOption("RejectLetter".Translate())
	{
		action = delegate
		{
			Action action = delegate
			{
				if (!rejectedSignal.NullOrEmpty())
				{
					object arg = ((pawns.Count == 1) ? ((object)pawns[0]) : ((object)pawns));
					Find.SignalManager.SendSignal(new Signal(rejectedSignal, arg.Named("SUBJECT")));
				}
				Find.LetterStack.RemoveLetter(this);
			};
			if (!ModsConfig.IdeologyActive || !charity)
			{
				action();
			}
			else
			{
				IEnumerable<Pawn> source = IdeoUtility.AllColonistsWithCharityPrecept();
				if (source.Any())
				{
					string text = "";
					foreach (IGrouping<Ideo, Pawn> item in from c in source
						group c by c.Ideo)
					{
						text = text + "\n- " + "BelieversIn".Translate(item.Key.name.Colorize(item.Key.TextColor), item.Select((Pawn f) => f.NameShortColored.Resolve()).ToCommaList()).Resolve();
					}
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmationCharityJoiner".Translate(text), action));
				}
				else
				{
					action();
				}
			}
		},
		resolveTree = true
	};

	public override IEnumerable<DiaOption> Choices
	{
		get
		{
			if (!base.ArchivedOnly)
			{
				yield return Option_Accept;
				yield return Option_RejectWithCharityConfirmation;
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
		Scribe_Values.Look(ref rejectedSignal, "rejectedSignal");
		Scribe_Values.Look(ref charity, "charity", defaultValue: false);
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
		if (p.Faction != null && p.Faction.HostileTo(Faction.OfPlayer))
		{
			return false;
		}
		Lord lord = p.GetLord();
		if (lord != null)
		{
			if (lord.CurLordToil is LordToil_ExitMap || lord.CurLordToil is LordToil_ExitMapRandom)
			{
				return false;
			}
			if (lord.LordJob is LordJob_VisitColony lordJob_VisitColony && lordJob_VisitColony.exitSubgraph.lordToils.Contains(lord.CurLordToil))
			{
				return false;
			}
		}
		return true;
	}
}
