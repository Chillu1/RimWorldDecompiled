using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class PawnsArrivalModeWorker
	{
		public PawnsArrivalModeDef def;

		public virtual bool CanUseWith(IncidentParms parms)
		{
			if (parms.faction != null && def.minTechLevel != 0 && (int)parms.faction.def.techLevel < (int)def.minTechLevel)
			{
				return false;
			}
			if (parms.raidArrivalModeForQuickMilitaryAid && !def.forQuickMilitaryAid)
			{
				return false;
			}
			if (parms.raidStrategy != null && !parms.raidStrategy.arriveModes.Contains(def))
			{
				return false;
			}
			return true;
		}

		public virtual float GetSelectionWeight(IncidentParms parms)
		{
			if (def.selectionWeightCurve != null)
			{
				return def.selectionWeightCurve.Evaluate(parms.points);
			}
			return 0f;
		}

		public abstract void Arrive(List<Pawn> pawns, IncidentParms parms);

		public virtual void TravelingTransportPodsArrived(List<ActiveDropPodInfo> dropPods, Map map)
		{
			throw new NotSupportedException("Traveling transport pods arrived with mode " + def.defName);
		}

		public abstract bool TryResolveRaidSpawnCenter(IncidentParms parms);
	}
}
