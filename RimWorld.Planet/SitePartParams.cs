using Verse;

namespace RimWorld.Planet;

public class SitePartParams : IExposable
{
	public int randomValue;

	public float threatPoints;

	public float lootMarketValue;

	public float points;

	public Precept_Relic relic;

	public ThingDef preciousLumpResources;

	public PawnKindDef animalKind;

	public int turretsCount;

	public int mortarsCount;

	public LayoutStructureSketch ancientLayoutStructureSketch;

	public ThingSetMakerDef ancientComplexRewardMaker;

	public string triggerSecuritySignal;

	public string relicLostSignal;

	public Thing relicThing;

	public float interiorThreatPoints;

	public float exteriorThreatPoints;

	public int stabilizerCount;

	public void ExposeData()
	{
		Scribe_Values.Look(ref randomValue, "randomValue", 0);
		Scribe_Values.Look(ref threatPoints, "threatPoints", 0f);
		Scribe_Values.Look(ref lootMarketValue, "lootMarketValue", 0f);
		Scribe_Values.Look(ref points, "points", 0f);
		Scribe_References.Look(ref relic, "relic");
		Scribe_Defs.Look(ref preciousLumpResources, "preciousLumpResources");
		Scribe_Defs.Look(ref animalKind, "animalKind");
		Scribe_Values.Look(ref turretsCount, "turretsCount", 0);
		Scribe_Values.Look(ref mortarsCount, "mortarsCount", 0);
		Scribe_Deep.Look(ref ancientLayoutStructureSketch, "ancientComplexSketch");
		Scribe_Defs.Look(ref ancientComplexRewardMaker, "ancientComplexRewardMaker");
		Scribe_Values.Look(ref triggerSecuritySignal, "triggerSecuritySignal");
		Scribe_References.Look(ref relicThing, "relicThing");
		Scribe_Values.Look(ref relicLostSignal, "relicLostSignal");
		Scribe_Values.Look(ref interiorThreatPoints, "interiorThreatPoints", 0f);
		Scribe_Values.Look(ref exteriorThreatPoints, "exteriorThreatPoints", 0f);
		Scribe_Values.Look(ref stabilizerCount, "stabilizerCount", 0);
	}
}
