using System.Collections.Generic;
using Verse.AI;

namespace RimWorld;

public class JobDriver_DevourerDigest : JobDriver
{
	private CompDevourer comp;

	private CompDevourer Comp => comp ?? (comp = pawn.GetComp<CompDevourer>());

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	public override string GetReport()
	{
		return null;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		int digestionTicks = Comp.GetDigestionTicks();
		Toil toil = Toils_General.Wait(digestionTicks).WithProgressBarToilDelay(TargetIndex.None, digestionTicks);
		toil.FailOn(() => !Comp.Digesting);
		toil.PlaySustainerOrSound(SoundDefOf.Pawn_Devourer_Digesting);
		toil.AddFinishAction(Comp.DigestJobFinished);
		yield return toil;
	}
}
