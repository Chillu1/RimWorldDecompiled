using Verse;

namespace RimWorld.Planet
{
	public class DownedRefugeeComp : ImportantPawnComp, IThingHolder
	{
		protected override string PawnSaveKey => "refugee";

		protected override void RemovePawnOnWorldObjectRemoved()
		{
			if (!pawn.Any)
			{
				return;
			}
			if (!pawn[0].Dead)
			{
				if (pawn[0].relations != null)
				{
					pawn[0].relations.Notify_FailedRescueQuest();
				}
				HealthUtility.HealNonPermanentInjuriesAndRestoreLegs(pawn[0]);
			}
			pawn.ClearAndDestroyContentsOrPassToWorld();
		}

		public override string CompInspectStringExtra()
		{
			if (pawn.Any)
			{
				return "Refugee".Translate() + ": " + pawn[0].LabelCap;
			}
			return null;
		}
	}
}
