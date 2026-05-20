using Verse;

namespace RimWorld;

public class CompRelicContainer : CompThingContainer
{
	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!ModLister.CheckIdeology("Reliqary"))
		{
			parent.Destroy();
		}
		else
		{
			base.PostSpawnSetup(respawningAfterLoad);
		}
	}

	public static bool IsRelic(Thing thing)
	{
		return thing.IsRelic();
	}

	public override bool Accepts(Thing thing)
	{
		return IsRelic(thing);
	}

	public override bool Accepts(ThingDef thingDef)
	{
		return false;
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (!base.Empty)
		{
			text = string.Concat(text, "\n" + "StatsReport_RelicAtRitualMoodBonus".Translate() + ": ", ThoughtDefOf.RelicAtRitual.stages[0].baseMoodEffect.ToString());
		}
		return text;
	}
}
