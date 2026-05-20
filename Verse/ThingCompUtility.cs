using System.Runtime.CompilerServices;

namespace Verse;

public static class ThingCompUtility
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T TryGetComp<T>(this Thing thing) where T : ThingComp
	{
		if (!(thing is ThingWithComps thingWithComps))
		{
			return null;
		}
		return thingWithComps.GetComp<T>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetComp<T>(this Thing thing, out T comp) where T : ThingComp
	{
		comp = null;
		if (!(thing is ThingWithComps thing2))
		{
			return false;
		}
		return thing2.TryGetComp<T>(out comp);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetComp<T>(this ThingWithComps thing, out T comp) where T : ThingComp
	{
		comp = thing.GetComp<T>();
		return comp != null;
	}

	public static ThingComp TryGetComp(this Thing thing, CompProperties properties)
	{
		if (!(thing is ThingWithComps thingWithComps))
		{
			return null;
		}
		return thingWithComps.GetCompByDefType(properties);
	}

	public static bool HasComp<T>(this Thing thing) where T : ThingComp
	{
		ThingWithComps obj = thing as ThingWithComps;
		return ((obj != null) ? obj.GetComp<T>() : null) != null;
	}
}
