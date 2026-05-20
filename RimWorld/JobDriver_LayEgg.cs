using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_LayEgg : JobDriver
	{
		private const int LayEgg = 500;

		private const TargetIndex LaySpotOrEggBoxInd = TargetIndex.A;

		public CompEggContainer EggBoxComp => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompEggContainer>();

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return Toils_General.Wait(500);
			yield return Toils_General.Do(delegate
			{
				Thing thing = pawn.GetComp<CompEggLayer>().ProduceEgg();
				if (job.GetTarget(TargetIndex.A).HasThing && EggBoxComp.Accepts(thing.def))
				{
					EggBoxComp.innerContainer.TryAdd(thing);
				}
				else
				{
					GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near, delegate(Thing t, int i)
					{
						if (pawn.Faction != Faction.OfPlayer)
						{
							t.SetForbidden(value: true);
						}
					});
				}
			});
		}
	}
}
