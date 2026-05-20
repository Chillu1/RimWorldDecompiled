using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class TaleData_Thing : TaleData
{
	public int thingID;

	public ThingDef thingDef;

	public ThingDef stuff;

	public string title;

	public QualityCategory quality;

	public override void ExposeData()
	{
		Scribe_Values.Look(ref thingID, "thingID", 0);
		Scribe_Defs.Look(ref thingDef, "thingDef");
		Scribe_Defs.Look(ref stuff, "stuff");
		Scribe_Values.Look(ref title, "title");
		Scribe_Values.Look(ref quality, "quality", QualityCategory.Awful);
	}

	public override IEnumerable<Rule> GetRules(string prefix, Dictionary<string, string> constants = null)
	{
		yield return new Rule_String(prefix + "_label", thingDef.label);
		yield return new Rule_String(prefix + "_definite", Find.ActiveLanguageWorker.WithDefiniteArticle(thingDef.label));
		yield return new Rule_String(prefix + "_indefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(thingDef.label));
		if (stuff != null)
		{
			yield return new Rule_String(prefix + "_stuffLabel", stuff.label);
		}
		if (title != null)
		{
			yield return new Rule_String(prefix + "_title", title);
		}
		yield return new Rule_String(prefix + "_quality", quality.GetLabel());
	}

	public static TaleData_Thing GenerateFrom(Thing t)
	{
		TaleData_Thing taleData_Thing = new TaleData_Thing();
		taleData_Thing.thingID = t.thingIDNumber;
		taleData_Thing.thingDef = t.def;
		taleData_Thing.stuff = t.Stuff;
		t.TryGetQuality(out taleData_Thing.quality);
		CompArt compArt = t.TryGetComp<CompArt>();
		if (compArt != null && compArt.Active)
		{
			taleData_Thing.title = compArt.Title;
		}
		if (t is Book book)
		{
			taleData_Thing.title = book.Title;
		}
		return taleData_Thing;
	}

	public static TaleData_Thing GenerateRandom()
	{
		ThingDef obj = DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.comps != null && d.comps.Any((CompProperties cp) => cp.compClass == typeof(CompArt))).RandomElement();
		ThingDef thingDef = GenStuff.RandomStuffFor(obj);
		Thing thing = ThingMaker.MakeThing(obj, thingDef);
		ArtGenerationContext artGenerationContext = ((Rand.Value < 0.5f) ? ArtGenerationContext.Colony : ArtGenerationContext.Outsider);
		CompQuality compQuality = thing.TryGetComp<CompQuality>();
		if (compQuality != null && (int)compQuality.Quality < (int)thing.TryGetComp<CompArt>().Props.minQualityForArtistic)
		{
			compQuality.SetQuality(thing.TryGetComp<CompArt>().Props.minQualityForArtistic, artGenerationContext);
		}
		thing.TryGetComp<CompArt>().InitializeArt(artGenerationContext);
		return GenerateFrom(thing);
	}
}
