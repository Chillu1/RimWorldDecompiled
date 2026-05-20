using Verse;

namespace RimWorld
{
	public class JobGiver_CreateAndEnterCocoon : JobGiver_CreateAndEnterDryadHolder
	{
		public override JobDef JobDef => JobDefOf.CreateAndEnterCocoon;

		public override bool ExtraValidator(Pawn pawn, CompTreeConnection connectionComp)
		{
			if (connectionComp.DryadKind != pawn.kindDef)
			{
				return true;
			}
			return base.ExtraValidator(pawn, connectionComp);
		}
	}
}
