using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_BoardOrLeaveGravship : JobDriver_StandAndBeSociallyActive
{
	private bool hasChecked;

	private string FailedKey
	{
		get
		{
			if (!job.def.boardingGravship)
			{
				return "FailedToLeaveGravship";
			}
			return "FailedToBoardGravship";
		}
	}

	protected override Toil GetGotoToil()
	{
		Toil toil = Toils_Goto.GotoCell(base.TargetLocA, PathEndMode.OnCell);
		toil.AddFinishAction(delegate
		{
			if (pawn.Dead || pawn.Destroyed)
			{
				CheckFailedToBoardOrLeave(doMessage: false);
			}
			else if (pawn.Downed)
			{
				CheckFailedToBoardOrLeave(doMessage: true, "DownedLower".Translate());
			}
			else
			{
				CheckFailedToBoardOrLeave();
			}
		});
		return toil;
	}

	public override void Notify_PatherFailed()
	{
		CheckFailedToBoardOrLeave(doMessage: true, "NoPath".Translate());
		base.Notify_PatherFailed();
	}

	private void CheckFailedToBoardOrLeave(bool doMessage = true, string reason = null)
	{
		if (hasChecked)
		{
			return;
		}
		hasChecked = true;
		Building_GravEngine playerGravEngine_NewTemp = GravshipUtility.GetPlayerGravEngine_NewTemp(pawn.Map);
		if (playerGravEngine_NewTemp == null || (playerGravEngine_NewTemp.pawnsToBoard == null && playerGravEngine_NewTemp.pawnsToLeave == null) || playerGravEngine_NewTemp.ValidSubstructure.Contains(pawn.Position) == job.def.boardingGravship)
		{
			return;
		}
		if (doMessage)
		{
			TaggedString taggedString = FailedKey.Translate(pawn.Named("PAWN"), playerGravEngine_NewTemp.RenamableLabel);
			if (!reason.NullOrEmpty())
			{
				taggedString += ": " + reason.CapitalizeFirst();
			}
			if (MessagesRepeatAvoider.MessageShowAllowed(taggedString, 10f))
			{
				Messages.Message(taggedString, pawn, MessageTypeDefOf.NegativeEvent, historical: false);
			}
		}
		playerGravEngine_NewTemp.pawnsToBoard?.Remove(pawn);
		playerGravEngine_NewTemp.pawnsToLeave?.Remove(pawn);
	}
}
