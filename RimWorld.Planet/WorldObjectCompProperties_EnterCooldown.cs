namespace RimWorld.Planet;

public class WorldObjectCompProperties_EnterCooldown : WorldObjectCompProperties
{
	public bool autoStartOnMapRemoved = true;

	public float durationDays = 1f;

	public WorldObjectCompProperties_EnterCooldown()
	{
		compClass = typeof(EnterCooldownComp);
	}
}
