using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class AnimalPenConnectedDistrictsCalculator : AnimalPenEnclosureCalculator
	{
		private readonly List<District> districtsTmp = new List<District>();

		private static readonly Dictionary<District, List<District>> connectedDistrictsForRootCached = new Dictionary<District, List<District>>();

		private static int connectedDistrictsForRootCachedTick = -1;

		protected override void VisitDirectlyConnectedRegion(Region r)
		{
			AddDistrict(r);
		}

		protected override void VisitIndirectlyDirectlyConnectedRegion(Region r)
		{
			AddDistrict(r);
		}

		protected override void VisitPassableDoorway(Region r)
		{
			AddDistrict(r);
		}

		private void AddDistrict(Region r)
		{
			District district = r.District;
			if (!districtsTmp.Contains(district))
			{
				districtsTmp.Add(district);
			}
		}

		public static void InvalidateDistrictCache(District district)
		{
			connectedDistrictsForRootCached.Remove(district);
		}

		public List<District> CalculateConnectedDistricts(IntVec3 position, Map map)
		{
			District district = position.GetDistrict(map);
			if (Find.TickManager.TicksGame == connectedDistrictsForRootCachedTick)
			{
				if (connectedDistrictsForRootCached.ContainsKey(district))
				{
					return connectedDistrictsForRootCached[district];
				}
			}
			else
			{
				connectedDistrictsForRootCached.Clear();
				connectedDistrictsForRootCachedTick = Find.TickManager.TicksGame;
			}
			districtsTmp.Clear();
			if (!VisitPen(position, map))
			{
				districtsTmp.Clear();
			}
			connectedDistrictsForRootCached[district] = districtsTmp.ToList();
			return districtsTmp;
		}

		public void Reset()
		{
			districtsTmp.Clear();
		}
	}
}
