using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_InspectString : QuestPartActivable
{
	public List<ISelectable> targets = new List<ISelectable>();

	public string inspectString;

	private string resolvedInspectString;

	private ILoadReferenceable targetRef;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			for (int i = 0; i < targets.Count; i++)
			{
				ISelectable selectable = targets[i];
				if (selectable is Thing)
				{
					yield return (Thing)selectable;
				}
				else if (selectable is WorldObject)
				{
					yield return (WorldObject)selectable;
				}
			}
		}
	}

	protected override void Enable(SignalArgs receivedArgs)
	{
		base.Enable(receivedArgs);
		resolvedInspectString = receivedArgs.GetFormattedText(inspectString);
	}

	public override string ExtraInspectString(ISelectable target)
	{
		if (targets.Contains(target))
		{
			return resolvedInspectString;
		}
		return null;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref targets, "targets", LookMode.Reference);
		Scribe_Values.Look(ref inspectString, "inspectString");
		Scribe_Values.Look(ref resolvedInspectString, "resolvedInspectString");
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		if (Find.AnyPlayerHomeMap != null)
		{
			targets.Add(Find.RandomPlayerHomeMap.mapPawns.FreeColonists.FirstOrDefault());
			inspectString = "Debug inspect string.";
		}
	}
}
