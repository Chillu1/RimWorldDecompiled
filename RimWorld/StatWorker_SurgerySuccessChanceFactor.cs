using Verse;

namespace RimWorld
{
	public class StatWorker_SurgerySuccessChanceFactor : StatWorker
	{
		public override bool ShouldShowFor(StatRequest req)
		{
			if (!base.ShouldShowFor(req))
			{
				return false;
			}
			Def def = req.Def;
			if (!(def is ThingDef))
			{
				return false;
			}
			ThingDef thingDef = def as ThingDef;
			if (typeof(Building_Bed).IsAssignableFrom(thingDef.thingClass))
			{
				return true;
			}
			return false;
		}
	}
}
