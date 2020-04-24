using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_GotoMindControlled : JobDriver_Goto
	{
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			if (job.def.waitAfterArriving > 0)
			{
				yield return Toils_General.Wait(job.def.waitAfterArriving);
			}
		}
	}
}
