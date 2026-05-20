using Verse;

namespace RimWorld;

public class PawnRelationWorker_Overseer : PawnRelationWorker
{
	public override void OnRelationCreated(Pawn firstPawn, Pawn secondPawn)
	{
		base.OnRelationCreated(firstPawn, secondPawn);
		if (MechanitorUtility.IsMechanitor(firstPawn))
		{
			firstPawn.mechanitor.AssignPawnControlGroup(secondPawn);
		}
		else if (MechanitorUtility.IsMechanitor(secondPawn))
		{
			secondPawn.mechanitor.AssignPawnControlGroup(firstPawn);
		}
	}

	public override void OnRelationRemoved(Pawn firstPawn, Pawn secondPawn)
	{
		base.OnRelationRemoved(firstPawn, secondPawn);
		if (MechanitorUtility.IsMechanitor(firstPawn))
		{
			firstPawn.mechanitor.UnassignPawnFromAnyControlGroup(secondPawn);
		}
		else if (MechanitorUtility.IsMechanitor(secondPawn))
		{
			secondPawn.mechanitor.UnassignPawnFromAnyControlGroup(firstPawn);
		}
	}

	public override void Notify_PostRemovedByDeath(Pawn firstPawn, Pawn secondPawn)
	{
		Pawn pawn = (MechanitorUtility.IsMechanitor(firstPawn) ? firstPawn : secondPawn);
		Pawn pawn2 = ((firstPawn == pawn) ? secondPawn : firstPawn);
		if (!pawn2.Dead)
		{
			pawn2.OverseerSubject?.Notify_DisconnectedFromOverseer();
		}
		if (pawn != null && !pawn.Dead)
		{
			Messages.Message(Find.ActiveLanguageWorker.PostProcessed("MessageMechanitorLostControlOfMech".Translate(pawn, pawn2) + ": " + pawn2.LabelShortCap), new LookTargets(new Thing[2] { pawn, pawn2 }), MessageTypeDefOf.NeutralEvent);
		}
	}

	public override void Notify_PostRemovedLeftBehind(Pawn firstPawn, Pawn secondPawn)
	{
		Pawn pawn = (MechanitorUtility.IsMechanitor(firstPawn) ? firstPawn : secondPawn);
		Pawn pawn2 = ((firstPawn == pawn) ? secondPawn : firstPawn);
		if (!pawn2.Dead)
		{
			pawn2.OverseerSubject?.Notify_DisconnectedFromOverseer();
		}
		if (pawn != null && !pawn.Dead)
		{
			Messages.Message(Find.ActiveLanguageWorker.PostProcessed("MessageMechanitorLostControlOfMech".Translate(pawn, pawn2) + ": " + pawn2.LabelShortCap), new LookTargets(new Thing[2] { pawn, pawn2 }), MessageTypeDefOf.NeutralEvent);
		}
	}
}
