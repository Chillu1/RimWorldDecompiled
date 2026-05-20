using System;
using System.Collections.Generic;

namespace Verse;

public class MapFileCompressor : IExposable
{
	private Map map;

	private byte[] compressedData;

	public CompressibilityDecider compressibilityDecider;

	public MapFileCompressor(Map map)
	{
		this.map = map;
	}

	public void ExposeData()
	{
		DataExposeUtility.LookByteArray(ref compressedData, "compressedThingMap");
	}

	public void BuildCompressedString()
	{
		compressibilityDecider = new CompressibilityDecider(map);
		compressibilityDecider.DetermineReferences();
		compressedData = MapSerializeUtility.SerializeUshort(map, HashValueForSquare);
	}

	private ushort HashValueForSquare(IntVec3 curSq)
	{
		ushort num = 0;
		foreach (Thing item in map.thingGrid.ThingsAt(curSq))
		{
			if (item.IsSaveCompressible())
			{
				if (num != 0)
				{
					IntVec3 intVec = curSq;
					Log.Error("Found two compressible things in " + intVec.ToString() + ". The last was " + item);
				}
				num = item.def.shortHash;
			}
		}
		return num;
	}

	public IEnumerable<Thing> ThingsToSpawnAfterLoad()
	{
		Dictionary<ushort, ThingDef> thingDefsByShortHash = new Dictionary<ushort, ThingDef>();
		foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
		{
			if (thingDefsByShortHash.TryGetValue(allDef.shortHash, out var value))
			{
				Log.Error($"Hash collision between {allDef} and  {value}: both have short hash {allDef.shortHash}");
			}
			else
			{
				thingDefsByShortHash.Add(allDef.shortHash, allDef);
			}
		}
		int major = ScribeMetaHeaderUtility.loadedGameVersionMajor;
		int minor = ScribeMetaHeaderUtility.loadedGameVersionMinor;
		List<Thing> loadables = new List<Thing>();
		MapSerializeUtility.LoadUshort(compressedData, map, delegate(IntVec3 c, ushort val)
		{
			if (val != 0)
			{
				ThingDef thingDef = BackCompatibility.BackCompatibleThingDefWithShortHash_Force(val, major, minor);
				if (thingDef == null)
				{
					try
					{
						thingDef = thingDefsByShortHash[val];
					}
					catch (KeyNotFoundException)
					{
						ThingDef thingDef2 = BackCompatibility.BackCompatibleThingDefWithShortHash(val);
						if (thingDef2 != null)
						{
							thingDef = thingDef2;
							thingDefsByShortHash.Add(val, thingDef2);
						}
						else
						{
							Log.Error("Map compressor decompression error: No thingDef with short hash " + val + ". Adding as null to dictionary.");
							thingDefsByShortHash.Add(val, null);
						}
					}
				}
				if (thingDef != null)
				{
					try
					{
						if (!thingDef.saveCompressible)
						{
							Log.Warning("Tried loading non-compressible thing as compressed thing: " + thingDef.defName);
						}
						else
						{
							Thing thing = ThingMaker.MakeThing(thingDef);
							thing.SetPositionDirect(c);
							loadables.Add(thing);
						}
					}
					catch (Exception ex2)
					{
						Log.Error("Could not instantiate compressed thing: " + ex2);
					}
				}
			}
		});
		return loadables;
	}
}
