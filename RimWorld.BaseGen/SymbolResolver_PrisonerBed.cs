using System;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_PrisonerBed : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		ResolveParams resolveParams = rp;
		Action<Thing> prevPostThingSpawn = resolveParams.postThingSpawn;
		resolveParams.postThingSpawn = delegate(Thing x)
		{
			if (prevPostThingSpawn != null)
			{
				prevPostThingSpawn(x);
			}
			if (x is Building_Bed building_Bed)
			{
				building_Bed.ForPrisoners = true;
			}
		};
		BaseGen.symbolStack.Push("bed", resolveParams);
	}
}
