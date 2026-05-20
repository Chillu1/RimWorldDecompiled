using System.Collections.Generic;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AssignRandomStuff : SketchResolver
{
	private Dictionary<IntVec3, List<SketchThing>> thingsAt = new Dictionary<IntVec3, List<SketchThing>>();

	private HashSet<SketchThing> visited = new HashSet<SketchThing>();

	private Stack<SketchThing> stack = new Stack<SketchThing>();

	protected override void ResolveInt(SketchResolveParams parms)
	{
		ThingDef assignRandomStuffTo = parms.assignRandomStuffTo;
		bool valueOrDefault = parms.connectedGroupsSameStuff == true;
		bool allowWood = parms.allowWood ?? true;
		bool allowFlammableWalls = parms.allowFlammableWalls ?? true;
		thingsAt.Clear();
		foreach (SketchThing thing2 in parms.sketch.Things)
		{
			if (assignRandomStuffTo != null && thing2.def != assignRandomStuffTo)
			{
				continue;
			}
			foreach (IntVec3 item in thing2.OccupiedRect)
			{
				if (!thingsAt.TryGetValue(item, out var value))
				{
					value = new List<SketchThing>();
					thingsAt.Add(item, value);
				}
				value.Add(thing2);
			}
		}
		visited.Clear();
		foreach (SketchThing thing in parms.sketch.Things)
		{
			if ((assignRandomStuffTo != null && thing.def != assignRandomStuffTo) || visited.Contains(thing))
			{
				continue;
			}
			ThingDef stuff = GenStuff.RandomStuffInexpensiveFor(thing.def, null, (ThingDef x) => SketchGenUtility.IsStuffAllowed(x, allowWood, parms.useOnlyStonesAvailableOnMap, allowFlammableWalls, thing.def));
			thing.stuff = stuff;
			visited.Add(thing);
			if (!valueOrDefault)
			{
				continue;
			}
			stack.Clear();
			stack.Push(thing);
			while (stack.Count != 0)
			{
				SketchThing sketchThing = stack.Pop();
				sketchThing.stuff = stuff;
				foreach (IntVec3 item2 in sketchThing.OccupiedRect.ExpandedBy(1))
				{
					if (!thingsAt.TryGetValue(item2, out var value2))
					{
						continue;
					}
					for (int num = 0; num < value2.Count; num++)
					{
						if (value2[num].def == thing.def && !visited.Contains(value2[num]))
						{
							visited.Add(value2[num]);
							stack.Push(value2[num]);
						}
					}
				}
			}
		}
	}

	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		return true;
	}
}
