using Verse;

namespace RimWorld.Planet
{
	public class PrisonerWillingToJoinComp : ImportantPawnComp, IThingHolder
	{
		protected override string PawnSaveKey => "prisoner";

		protected override void RemovePawnOnWorldObjectRemoved()
		{
			pawn.ClearAndDestroyContentsOrPassToWorld();
		}

		public override string CompInspectStringExtra()
		{
			if (pawn.Any)
			{
				return "Prisoner".Translate() + ": " + pawn[0].LabelCap;
			}
			return null;
		}
	}
}
