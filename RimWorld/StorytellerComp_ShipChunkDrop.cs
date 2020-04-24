using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_ShipChunkDrop : StorytellerComp
	{
		private static readonly SimpleCurve ShipChunkDropMTBDaysCurve = new SimpleCurve
		{
			new CurvePoint(0f, 20f),
			new CurvePoint(1f, 40f),
			new CurvePoint(2f, 80f),
			new CurvePoint(2.75f, 135f)
		};

		private float ShipChunkDropMTBDays
		{
			get
			{
				float x = (float)Find.TickManager.TicksGame / 3600000f;
				return ShipChunkDropMTBDaysCurve.Evaluate(x);
			}
		}

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (Rand.MTBEventOccurs(ShipChunkDropMTBDays, 60000f, 1000f))
			{
				IncidentDef shipChunkDrop = IncidentDefOf.ShipChunkDrop;
				IncidentParms parms = GenerateParms(shipChunkDrop.category, target);
				if (shipChunkDrop.Worker.CanFireNow(parms))
				{
					yield return new FiringIncident(shipChunkDrop, this, parms);
				}
			}
		}
	}
}
