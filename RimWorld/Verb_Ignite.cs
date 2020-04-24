using Verse;

namespace RimWorld
{
	public class Verb_Ignite : Verb
	{
		public Verb_Ignite()
		{
			verbProps = NativeVerbPropertiesDatabase.VerbWithCategory(VerbCategory.Ignite);
		}

		protected override bool TryCastShot()
		{
			Thing thing = currentTarget.Thing;
			Pawn casterPawn = CasterPawn;
			FireUtility.TryStartFireIn(thing.OccupiedRect().ClosestCellTo(casterPawn.Position), casterPawn.Map, 0.3f);
			if (casterPawn.Spawned)
			{
				casterPawn.Drawer.Notify_MeleeAttackOn(thing);
			}
			return true;
		}
	}
}
