using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobDriver_ChimeraSwitchToAttackMode : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.initAction = delegate
		{
			Lord lord = pawn.GetLord();
			if (lord?.LordJob is LordJob_ChimeraAssault)
			{
				lord.ReceiveMemo("StalkToAttack");
			}
		};
		yield return toil;
	}
}
