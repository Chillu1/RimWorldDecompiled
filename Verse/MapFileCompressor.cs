using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse
{
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
			DataExposeUtility.ByteArray(ref compressedData, "compressedThingMap");
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
						Log.Error(string.Concat("Found two compressible things in ", curSq, ". The last was ", item));
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
				if (thingDefsByShortHash.ContainsKey(allDef.shortHash))
				{
					Log.Error(string.Concat("Hash collision between ", allDef, " and  ", thingDefsByShortHash[allDef.shortHash], ": both have short hash ", allDef.shortHash));
				}
				else
				{
					thingDefsByShortHash.Add(allDef.shortHash, allDef);
				}
			}
			int major = VersionControl.MajorFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion);
			int minor = VersionControl.MinorFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion);
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
							Thing thing = ThingMaker.MakeThing(thingDef);
							thing.SetPositionDirect(c);
							loadables.Add(thing);
						}
						catch (Exception arg)
						{
							Log.Error("Could not instantiate compressed thing: " + arg);
						}
					}
				}
			});
			return loadables;
		}
	}
}
