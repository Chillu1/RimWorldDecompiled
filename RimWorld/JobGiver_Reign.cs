using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Reign : ThinkNode_JobGiver
	{
		public const float NeedLevelThreshold = 0.9f;

		public override float GetPriority(Pawn pawn)
		{
			if (pawn.needs.authority != null && pawn.needs.authority.IsActive && pawn.needs.authority.CurLevel < 0.9f)
			{
				return 6f;
			}
			return 0f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Building_Throne building_Throne = RoyalTitleUtility.FindBestUsableThrone(pawn);
			if (building_Throne == null || !building_Throne.Spawned)
			{
				return null;
			}
			if (!pawn.CanReach(building_Throne, PathEndMode.InteractionCell, pawn.NormalMaxDanger()))
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.Reign, building_Throne);
		}
	}
}
