using Verse;

namespace RimWorld.Planet
{
	public class SitePartParams : IExposable
	{
		public int randomValue;

		public float threatPoints;

		public ThingDef preciousLumpResources;

		public PawnKindDef animalKind;

		public int turretsCount;

		public int mortarsCount;

		public void ExposeData()
		{
			Scribe_Values.Look(ref randomValue, "randomValue", 0);
			Scribe_Values.Look(ref threatPoints, "threatPoints", 0f);
			Scribe_Defs.Look(ref preciousLumpResources, "preciousLumpResources");
			Scribe_Defs.Look(ref animalKind, "animalKind");
			Scribe_Values.Look(ref turretsCount, "turretsCount", 0);
			Scribe_Values.Look(ref mortarsCount, "mortarsCount", 0);
		}
	}
}
