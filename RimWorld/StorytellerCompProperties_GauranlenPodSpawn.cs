namespace RimWorld;

public class StorytellerCompProperties_GauranlenPodSpawn : StorytellerCompProperties
{
	public float daysBetweenPodSpawns;

	public int countdownFactorAnyConnectors = 1;

	public IncidentDef incident;

	public StorytellerCompProperties_GauranlenPodSpawn()
	{
		compClass = typeof(StorytellerComp_GauranlenPodSpawn);
	}
}
