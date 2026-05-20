using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class Instruction_BuildSandbags : Lesson_Instruction
{
	private List<IntVec3> sandbagCells;

	protected override float ProgressPercent
	{
		get
		{
			int num = 0;
			int num2 = 0;
			foreach (IntVec3 sandbagCell in sandbagCells)
			{
				if (TutorUtility.BuildingOrBlueprintOrFrameCenterExists(sandbagCell, base.Map, ThingDefOf.Sandbags))
				{
					num2++;
				}
				num++;
			}
			return (float)num2 / (float)num;
		}
	}

	public override void OnActivated()
	{
		base.OnActivated();
		Find.TutorialState.sandbagsRect = TutorUtility.FindUsableRect(7, 7, base.Map);
		sandbagCells = new List<IntVec3>();
		foreach (IntVec3 edgeCell in Find.TutorialState.sandbagsRect.EdgeCells)
		{
			if (edgeCell.x != Find.TutorialState.sandbagsRect.CenterCell.x && edgeCell.z != Find.TutorialState.sandbagsRect.CenterCell.z)
			{
				sandbagCells.Add(edgeCell);
			}
		}
		foreach (IntVec3 item in Find.TutorialState.sandbagsRect.ContractedBy(1))
		{
			if (Find.TutorialState.sandbagsRect.ContractedBy(2).Contains(item))
			{
				continue;
			}
			List<Thing> thingList = item.GetThingList(base.Map);
			for (int num = thingList.Count - 1; num >= 0; num--)
			{
				Thing thing = thingList[num];
				if (thing.def.passability != Traversability.Standable && (thing.def.category == ThingCategory.Plant || thing.def.category == ThingCategory.Item))
				{
					thing.Destroy();
				}
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref sandbagCells, "sandbagCells", LookMode.Undefined);
	}

	public override void LessonOnGUI()
	{
		TutorUtility.DrawLabelOnGUI(Gen.AveragePosition(sandbagCells), def.onMapInstruction);
		base.LessonOnGUI();
	}

	public override void LessonUpdate()
	{
		GenDraw.DrawFieldEdges(sandbagCells.Where((IntVec3 c) => !TutorUtility.BuildingOrBlueprintOrFrameCenterExists(c, base.Map, ThingDefOf.Sandbags)).ToList());
		GenDraw.DrawArrowPointingAt(Gen.AveragePosition(sandbagCells));
		if (ProgressPercent > 0.9999f)
		{
			Find.ActiveLesson.Deactivate();
		}
	}

	public override AcceptanceReport AllowAction(EventPack ep)
	{
		if (ep.Tag == "Designate-Sandbags")
		{
			return TutorUtility.EventCellsAreWithin(ep, sandbagCells);
		}
		return base.AllowAction(ep);
	}
}
