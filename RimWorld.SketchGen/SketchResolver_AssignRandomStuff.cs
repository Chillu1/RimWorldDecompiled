using System.Collections.Generic;
using Verse;

namespace RimWorld.SketchGen
{
	public class SketchResolver_AssignRandomStuff : SketchResolver
	{
		private Dictionary<IntVec3, List<SketchThing>> thingsAt = new Dictionary<IntVec3, List<SketchThing>>();

		private HashSet<SketchThing> visited = new HashSet<SketchThing>();

		private Stack<SketchThing> stack = new Stack<SketchThing>();

		protected override void ResolveInt(ResolveParams parms)
		{
			ThingDef assignRandomStuffTo = parms.assignRandomStuffTo;
			bool flag = parms.connectedGroupsSameStuff ?? false;
			bool allowWood = parms.allowWood ?? true;
			bool allowFlammableWalls = parms.allowFlammableWalls ?? true;
			thingsAt.Clear();
			foreach (SketchThing thing2 in parms.sketch.Things)
			{
				if (assignRandomStuffTo == null || thing2.def == assignRandomStuffTo)
				{
					foreach (IntVec3 item in thing2.OccupiedRect)
					{
						if (!thingsAt.TryGetValue(item, out List<SketchThing> value))
						{
							value = new List<SketchThing>();
							thingsAt.Add(item, value);
						}
						value.Add(thing2);
					}
				}
			}
			visited.Clear();
			foreach (SketchThing thing in parms.sketch.Things)
			{
				if ((assignRandomStuffTo == null || thing.def == assignRandomStuffTo) && !visited.Contains(thing))
				{
					ThingDef stuff = GenStuff.RandomStuffInexpensiveFor(thing.def, null, (ThingDef x) => SketchGenUtility.IsStuffAllowed(x, allowWood, parms.useOnlyStonesAvailableOnMap, allowFlammableWalls, thing.def));
					thing.stuff = stuff;
					visited.Add(thing);
					if (flag)
					{
						stack.Clear();
						stack.Push(thing);
						while (stack.Count != 0)
						{
							SketchThing sketchThing = stack.Pop();
							sketchThing.stuff = stuff;
							foreach (IntVec3 item2 in sketchThing.OccupiedRect.ExpandedBy(1))
							{
								if (thingsAt.TryGetValue(item2, out List<SketchThing> value2))
								{
									for (int i = 0; i < value2.Count; i++)
									{
										if (value2[i].def == thing.def && !visited.Contains(value2[i]))
										{
											visited.Add(value2[i]);
											stack.Push(value2[i]);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		protected override bool CanResolveInt(ResolveParams parms)
		{
			return true;
		}
	}
}
