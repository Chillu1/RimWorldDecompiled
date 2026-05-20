using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThoughtWorker_BloodTorch : ThoughtWorker
	{
		private const float MaxDist = 8f;

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			if (p.Suspended)
			{
				return false;
			}
			Room pawnRoom = p.GetRoom();
			if (GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(ThingDefOf.SanguphageMeetingTorch), PathEndMode.ClosestTouch, TraverseParms.For(p), 8f, delegate(Thing b)
			{
				Room room = b.GetRoom();
				return (room == null || room == pawnRoom) && (b.TryGetComp<CompGlower>()?.Glows ?? false);
			}) == null)
			{
				return false;
			}
			return ThoughtState.ActiveAtStage(0);
		}
	}
}
