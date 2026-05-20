using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_AncientCryptosleepCasket : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		int groupID = rp.ancientCryptosleepCasketGroupID ?? Find.UniqueIDsManager.GetNextAncientCryptosleepCasketGroupID();
		PodContentsType type = rp.podContentsType ?? Gen.RandomEnumValue<PodContentsType>(disallowFirstValue: true);
		Rot4 rot = rp.thingRot ?? Rot4.North;
		Building_AncientCryptosleepCasket pod = RoomGenUtility.SpawnCryptoCasket(rp.rect.RandomCell, BaseGen.globalSettings.map, rot, groupID, type, ThingSetMakerDefOf.MapGen_AncientPodContents);
		if (rp.ancientCryptosleepCasketOpenSignalTag != null)
		{
			RoomGenUtility.SpawnOpenCryptoCasketSignal(pod, BaseGen.globalSettings.map, rp.ancientCryptosleepCasketOpenSignalTag);
		}
	}
}
