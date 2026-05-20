using Verse;

namespace RimWorld;

public static class ThingStyleHelper
{
	public static bool CanBeStyled(this ThingDef thingDef)
	{
		return thingDef.CompDefFor<CompStyleable>() != null;
	}

	public static bool GetEverSeenByPlayer(this Thing thing)
	{
		return (thing as ThingWithComps)?.compStyleable?.everSeenByPlayer == true;
	}

	public static ThingStyleDef GetStyleDef(this Thing thing)
	{
		return (thing as ThingWithComps)?.compStyleable?.styleDef;
	}

	public static Precept_ThingStyle GetStyleSourcePrecept(this Thing thing)
	{
		return (thing as ThingWithComps)?.compStyleable?.SourcePrecept;
	}

	public static void SetEverSeenByPlayer(this Thing thing, bool everSeenByPlayer)
	{
		CompStyleable compStyleable = (thing as ThingWithComps)?.compStyleable;
		if (compStyleable != null)
		{
			compStyleable.everSeenByPlayer = everSeenByPlayer;
		}
		if (everSeenByPlayer && thing.StyleSourcePrecept is Precept_Relic)
		{
			thing.StyleSourcePrecept.ideo.Notify_RelicSeenByPlayer(thing);
		}
	}

	public static void SetStyleDef(this Thing thing, ThingStyleDef styleDef)
	{
		CompStyleable compStyleable = (thing as ThingWithComps)?.compStyleable;
		if (compStyleable == null)
		{
			if (styleDef != null)
			{
				Log.Warning("Tried setting ThingStyleDef to a thing without CompStyleable (" + thing.def.defName + ")!");
			}
		}
		else
		{
			compStyleable.styleDef = styleDef;
			compStyleable.cachedStyleCategoryDef = null;
		}
	}

	public static void SetStyleSourcePrecept(this Thing thing, Precept_ThingStyle precept)
	{
		CompStyleable compStyleable = (thing as ThingWithComps)?.compStyleable;
		if (compStyleable == null)
		{
			if (precept != null)
			{
				Log.Warning("Tried setting StyleSourcePrecept to a thing without CompStyleable (" + thing.def.defName + ")!");
			}
		}
		else
		{
			compStyleable.SourcePrecept = precept;
		}
	}
}
