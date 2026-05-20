using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class LandingOutcomeWorker_ThrusterBreakdown : LandingOutcomeWorker
{
	public LandingOutcomeWorker_ThrusterBreakdown(LandingOutcomeDef def)
		: base(def)
	{
	}

	public override void ApplyOutcome(Gravship gravship)
	{
		List<ThingWithComps> list = (from compGravshipFacility in gravship.Engine.GravshipComponents
			where compGravshipFacility.parent.HasComp<CompGravshipThruster>()
			select compGravshipFacility.parent).ToList();
		list = list.InRandomOrder().Take(Rand.Range(Mathf.Min(2, list.Count), Mathf.Min(4, list.Count))).ToList();
		foreach (ThingWithComps item in list)
		{
			if (item.TryGetComp<CompBreakdownable>(out var comp))
			{
				comp.DoBreakdown();
			}
		}
		TaggedString taggedString = "ThrustersBrokeDown".Translate() + ":\n" + list.Select((ThingWithComps thingWithComps) => thingWithComps.LabelCap).ToLineList(" - ");
		SendStandardLetter(gravship.Engine, taggedString, new LookTargets(list));
	}
}
