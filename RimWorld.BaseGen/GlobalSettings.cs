using Verse;

namespace RimWorld.BaseGen
{
	public class GlobalSettings
	{
		public Map map;

		public int minBuildings;

		public int minEmptyNodes;

		public int minBarracks;

		public CellRect mainRect;

		public int basePart_buildingsResolved;

		public int basePart_emptyNodesResolved;

		public int basePart_barracksResolved;

		public float basePart_batteriesCoverage;

		public float basePart_farmsCoverage;

		public float basePart_powerPlantsCoverage;

		public float basePart_breweriesCoverage;

		public void Clear()
		{
			map = null;
			minBuildings = 0;
			minBarracks = 0;
			minEmptyNodes = 0;
			mainRect = CellRect.Empty;
			basePart_buildingsResolved = 0;
			basePart_emptyNodesResolved = 0;
			basePart_barracksResolved = 0;
			basePart_batteriesCoverage = 0f;
			basePart_farmsCoverage = 0f;
			basePart_powerPlantsCoverage = 0f;
			basePart_breweriesCoverage = 0f;
		}
	}
}
