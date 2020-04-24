using Verse;

namespace RimWorld
{
	public class ThoughtWorker_IsIndoorsForUndergrounder : ThoughtWorker
	{
		public static bool IsAwakeAndIndoors(Pawn p, out bool isNaturalRoof)
		{
			isNaturalRoof = false;
			if (!p.Awake())
			{
				return false;
			}
			if (p.Position.UsesOutdoorTemperature(p.Map))
			{
				return false;
			}
			RoofDef roofDef = p.Map.roofGrid.RoofAt(p.Position);
			if (roofDef == null)
			{
				return false;
			}
			isNaturalRoof = roofDef.isNatural;
			return true;
		}

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			bool isNaturalRoof;
			return IsAwakeAndIndoors(p, out isNaturalRoof) && !isNaturalRoof;
		}
	}
}
