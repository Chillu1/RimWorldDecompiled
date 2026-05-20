using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class LandingOutcomeWorker_MinorGravshipCrash : LandingOutcomeWorker
{
	private static readonly FloatRange DamageRadiusRange = new FloatRange(5.9f, 11.9f);

	private static readonly IntRange DamageAmountRange = new IntRange(25, 30);

	public LandingOutcomeWorker_MinorGravshipCrash(LandingOutcomeDef def)
		: base(def)
	{
	}

	public override void ApplyOutcome(Gravship gravship)
	{
		ThingWithComps parent = gravship.Engine.GravshipComponents.Where((CompGravshipFacility comp) => comp.parent.HasComp<CompGravshipThruster>()).RandomElement().parent;
		float randomInRange = DamageRadiusRange.RandomInRange;
		HashSet<Thing> hashSet = new HashSet<Thing>();
		foreach (IntVec3 item in GenRadial.RadialCellsAround(parent.Position, randomInRange, useCenter: true))
		{
			if (!item.InBounds(parent.Map))
			{
				continue;
			}
			int randomInRange2 = DamageAmountRange.RandomInRange;
			DamageInfo dinfo = new DamageInfo(DamageDefOf.Crush, randomInRange2);
			List<Thing> thingList = item.GetThingList(parent.Map);
			for (int num = thingList.Count - 1; num >= 0; num--)
			{
				Thing thing = thingList[num];
				if (!hashSet.Contains(thing) && thing is Building)
				{
					thing.TakeDamage(dinfo);
					hashSet.Add(thing);
				}
			}
		}
		SendStandardLetter(gravship.Engine, null, parent);
	}
}
