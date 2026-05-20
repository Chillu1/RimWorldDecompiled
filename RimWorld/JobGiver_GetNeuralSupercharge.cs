using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GetNeuralSupercharge : ThinkNode_JobGiver
	{
		public override float GetPriority(Pawn pawn)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return 0f;
			}
			int lastReceivedNeuralSuperchargeTick = pawn.health.lastReceivedNeuralSuperchargeTick;
			if (lastReceivedNeuralSuperchargeTick != -1 && Find.TickManager.TicksGame - lastReceivedNeuralSuperchargeTick < 30000)
			{
				return 0f;
			}
			if (ClosestSupercharger(pawn) == null)
			{
				return 0f;
			}
			return 9.25f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Thing thing = ClosestSupercharger(pawn);
			if (thing == null || !pawn.CanReserve(thing))
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.GetNeuralSupercharge, thing);
		}

		private Thing ClosestSupercharger(Pawn pawn)
		{
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.NeuralSupercharger), PathEndMode.InteractionCell, TraverseParms.For(pawn), 9999f, Validator);
			bool Validator(Thing x)
			{
				CompNeuralSupercharger compNeuralSupercharger = x.TryGetComp<CompNeuralSupercharger>();
				if (compNeuralSupercharger.Charged && !x.IsForbidden(pawn))
				{
					return compNeuralSupercharger.CanAutoUse(pawn);
				}
				return false;
			}
		}
	}
}
