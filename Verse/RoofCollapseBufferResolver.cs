using System.Collections.Generic;
using System.Text;
using RimWorld;

namespace Verse;

public class RoofCollapseBufferResolver
{
	private Map map;

	private List<Thing> tmpCrushedThings = new List<Thing>();

	private HashSet<string> tmpCrushedNames = new HashSet<string>();

	public RoofCollapseBufferResolver(Map map)
	{
		this.map = map;
	}

	public void CollapseRoofsMarkedToCollapse()
	{
		RoofCollapseBuffer roofCollapseBuffer = map.roofCollapseBuffer;
		if (!roofCollapseBuffer.CellsMarkedToCollapse.Any())
		{
			return;
		}
		tmpCrushedThings.Clear();
		RoofCollapserImmediate.DropRoofInCells(roofCollapseBuffer.CellsMarkedToCollapse, map, tmpCrushedThings);
		if (tmpCrushedThings.Any())
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("RoofCollapsed".Translate());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("TheseThingsCrushed".Translate());
			tmpCrushedNames.Clear();
			for (int i = 0; i < tmpCrushedThings.Count; i++)
			{
				Thing thing = tmpCrushedThings[i];
				if (!(thing is Corpse { Bugged: not false }))
				{
					string item = thing.LabelShortCap;
					if (thing.def.category == ThingCategory.Pawn)
					{
						item = thing.LabelCap;
					}
					if (!tmpCrushedNames.Contains(item))
					{
						tmpCrushedNames.Add(item);
					}
				}
			}
			foreach (string tmpCrushedName in tmpCrushedNames)
			{
				stringBuilder.AppendLine("    -" + tmpCrushedName);
			}
			Find.LetterStack.ReceiveLetter("LetterLabelRoofCollapsed".Translate(), stringBuilder.ToString().TrimEndNewlines(), LetterDefOf.NegativeEvent, new TargetInfo(roofCollapseBuffer.CellsMarkedToCollapse[0], map));
		}
		else
		{
			Messages.Message("RoofCollapsed".Translate(), new TargetInfo(roofCollapseBuffer.CellsMarkedToCollapse[0], map), MessageTypeDefOf.SilentInput);
		}
		tmpCrushedThings.Clear();
		roofCollapseBuffer.Clear();
	}
}
