using System.Collections.Generic;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AncientEquipmentRoom : SketchResolver
{
	private static IEnumerable<ThingDef> CentralThings
	{
		get
		{
			yield return ThingDefOf.AncientMachine;
			yield return ThingDefOf.AncientStorageCylinder;
		}
	}

	private static IEnumerable<ThingDef> EdgeThings
	{
		get
		{
			yield return ThingDefOf.AncientSystemRack;
			yield return ThingDefOf.AncientEquipmentBlocks;
		}
	}

	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		if (parms.rect.HasValue)
		{
			return parms.sketch != null;
		}
		return false;
	}

	protected override void ResolveInt(SketchResolveParams parms)
	{
		if (!ModLister.CheckIdeology("Ancient equipment room"))
		{
			return;
		}
		SketchResolveParams parms2 = parms;
		parms2.cornerThing = ThingDefOf.AncientLamp;
		parms2.requireFloor = true;
		SketchResolverDefOf.AddCornerThings.Resolve(parms2);
		foreach (ThingDef centralThing in CentralThings)
		{
			SketchResolveParams parms3 = parms;
			parms3.thingCentral = centralThing;
			parms3.requireFloor = true;
			SketchResolverDefOf.AddThingsCentral.Resolve(parms3);
		}
		foreach (ThingDef edgeThing in EdgeThings)
		{
			SketchResolveParams parms4 = parms;
			parms4.wallEdgeThing = edgeThing;
			parms4.requireFloor = true;
			SketchResolverDefOf.AddWallEdgeThings.Resolve(parms4);
		}
	}
}
