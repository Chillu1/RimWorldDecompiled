using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ChoiceLetter_AcceptCreepJoiner : ChoiceLetter
{
	public string signalAccept;

	public string signalCapture;

	public string signalReject;

	public Pawn pawn;

	public Pawn speaker;

	public override bool CanDismissWithRightClick => false;

	public override bool CanShowInLetterStack
	{
		get
		{
			if (base.CanShowInLetterStack && quest != null)
			{
				if (quest.State != QuestState.Ongoing)
				{
					return quest.State == QuestState.NotYetAccepted;
				}
				return true;
			}
			return false;
		}
	}

	public override IEnumerable<DiaOption> Choices
	{
		get
		{
			if (base.ArchivedOnly || !pawn.Spawned)
			{
				yield return base.Option_Close;
				yield break;
			}
			DiaOption diaOption = new DiaOption("AcceptCreeper".Translate(pawn.Named("PAWN"), speaker.Named("SPEAKER")));
			DiaOption optionCapture = new DiaOption("CaptureCreeper".Translate(pawn.Named("PAWN"), speaker.Named("SPEAKER")));
			DiaOption optionReject = new DiaOption("RejectCreeper".Translate(pawn.Named("PAWN"), speaker.Named("SPEAKER")));
			diaOption.action = delegate
			{
				if (pawn.Spawned)
				{
					Find.SignalManager.SendSignal(new Signal(signalAccept));
					Find.LetterStack.RemoveLetter(this);
				}
			};
			diaOption.resolveTree = true;
			optionCapture.action = delegate
			{
				if (pawn.Spawned)
				{
					Find.SignalManager.SendSignal(new Signal(signalCapture));
				}
			};
			optionCapture.resolveTree = true;
			if (pawn.Spawned && !CaptureUtility.CanArrest(speaker, pawn, out var reason))
			{
				optionCapture.Disable(reason);
			}
			optionReject.action = delegate
			{
				if (pawn.Spawned)
				{
					Find.SignalManager.SendSignal(new Signal(signalReject));
					Find.LetterStack.RemoveLetter(this);
				}
			};
			optionReject.resolveTree = true;
			yield return diaOption;
			yield return optionCapture;
			yield return optionReject;
			if (lookTargets.IsValid())
			{
				yield return base.Option_JumpToLocationAndPostpone;
			}
			yield return base.Option_Postpone;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref signalAccept, "signalAccept");
		Scribe_Values.Look(ref signalCapture, "signalCapture");
		Scribe_Values.Look(ref signalReject, "signalReject");
		Scribe_References.Look(ref pawn, "pawn");
		Scribe_References.Look(ref speaker, "speaker");
	}
}
