using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Building_ShipReactor : Building
{
	public bool charlonsReactor;

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		if (charlonsReactor)
		{
			QuestUtility.SendQuestTargetSignals(base.Map.Parent.questTags, "ReactorDestroyed");
		}
		base.Destroy(mode);
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		foreach (Gizmo item in ShipUtility.ShipStartupGizmos(this))
		{
			yield return item;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref charlonsReactor, "charlonsReactor", defaultValue: false);
	}
}
