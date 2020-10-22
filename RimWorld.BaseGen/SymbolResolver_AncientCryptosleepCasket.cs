using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_AncientCryptosleepCasket : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			int groupID = rp.ancientCryptosleepCasketGroupID ?? Find.UniqueIDsManager.GetNextAncientCryptosleepCasketGroupID();
			PodContentsType value = rp.podContentsType ?? Gen.RandomEnumValue<PodContentsType>(disallowFirstValue: true);
			Rot4 rot = rp.thingRot ?? Rot4.North;
			Building_AncientCryptosleepCasket building_AncientCryptosleepCasket = (Building_AncientCryptosleepCasket)ThingMaker.MakeThing(ThingDefOf.AncientCryptosleepCasket);
			building_AncientCryptosleepCasket.groupID = groupID;
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.podContentsType = value;
			List<Thing> list = ThingSetMakerDefOf.MapGen_AncientPodContents.root.Generate(parms);
			for (int i = 0; i < list.Count; i++)
			{
				if (!building_AncientCryptosleepCasket.TryAcceptThing(list[i], allowSpecialEffects: false))
				{
					Pawn pawn = list[i] as Pawn;
					if (pawn != null)
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
					}
					else
					{
						list[i].Destroy();
					}
				}
			}
			GenSpawn.Spawn(building_AncientCryptosleepCasket, rp.rect.RandomCell, BaseGen.globalSettings.map, rot);
		}
	}
}
