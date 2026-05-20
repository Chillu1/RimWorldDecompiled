using Verse;

namespace RimWorld.BaseGen;

public class GlobalSettings
{
	public Map map;

	public int minBuildings;

	public int minEmptyNodes;

	public int minLandingPads;

	public int minBarracks;

	public int requiredWorshippedTerminalRooms;

	public int requiredGravcoreRooms;

	public int minThroneRooms;

	public int maxFarms = -1;

	public CellRect mainRect;

	public int basePart_buildingsResolved;

	public int basePart_emptyNodesResolved;

	public int basePart_landingPadsResolved;

	public int basePart_barracksResolved;

	public int basePart_throneRoomsResolved;

	public int basePart_worshippedTerminalsResolved;

	public int basePart_gravcoresResolved;

	public float basePart_batteriesCoverage;

	public float basePart_farmsCoverage;

	public int basePart_farmsCount;

	public float basePart_powerPlantsCoverage;

	public float basePart_breweriesCoverage;

	public int landingPadsGenerated;

	public void Clear()
	{
		map = null;
		minBuildings = 0;
		minBarracks = 0;
		requiredWorshippedTerminalRooms = 0;
		requiredGravcoreRooms = 0;
		minEmptyNodes = 0;
		minLandingPads = 0;
		minThroneRooms = 0;
		maxFarms = -1;
		mainRect = CellRect.Empty;
		basePart_buildingsResolved = 0;
		basePart_emptyNodesResolved = 0;
		basePart_landingPadsResolved = 0;
		basePart_barracksResolved = 0;
		basePart_throneRoomsResolved = 0;
		basePart_batteriesCoverage = 0f;
		basePart_farmsCoverage = 0f;
		basePart_farmsCount = 0;
		basePart_powerPlantsCoverage = 0f;
		basePart_breweriesCoverage = 0f;
		basePart_worshippedTerminalsResolved = 0;
		basePart_gravcoresResolved = 0;
	}

	public void ClearResult()
	{
		landingPadsGenerated = 0;
	}
}
