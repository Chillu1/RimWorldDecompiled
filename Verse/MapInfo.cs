using System.Runtime.CompilerServices;
using RimWorld.Planet;

namespace Verse;

public sealed class MapInfo : IExposable
{
	private IntVec3 sizeInt;

	public MapParent parent;

	public bool isPocketMap;

	public bool disableSunShadows;

	public PlanetTile Tile => parent?.Tile ?? PlanetTile.Invalid;

	public int NumCells => Size.x * Size.y * Size.z;

	public IntVec3 Size
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return sizeInt;
		}
		set
		{
			sizeInt = value;
		}
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref sizeInt, "size");
		Scribe_Values.Look(ref isPocketMap, "isPocketMap", defaultValue: false);
		Scribe_Values.Look(ref disableSunShadows, "disableSunShadows", defaultValue: false);
		Scribe_References.Look(ref parent, "parent");
	}
}
