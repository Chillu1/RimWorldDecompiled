namespace RimWorld.Planet
{
	public class WorldObjectCompProperties_EnterCooldown : WorldObjectCompProperties
	{
		public bool autoStartOnMapRemoved = true;

		public float durationDays = 4f;

		public WorldObjectCompProperties_EnterCooldown()
		{
			compClass = typeof(EnterCooldownComp);
		}
	}
}
