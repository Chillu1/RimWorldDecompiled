using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_SelfShutdown : JobDriver
{
	public const TargetIndex RestSpotIndex = TargetIndex.A;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!ModLister.CheckBiotech("Self-shutdown"))
		{
			return false;
		}
		return pawn.Reserve(base.TargetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		Toil toil = Toils_LayDown.SelfShutdown();
		toil.PlaySoundAtStart(SoundDefOf.MechSelfShutdown);
		yield return toil;
	}
}
