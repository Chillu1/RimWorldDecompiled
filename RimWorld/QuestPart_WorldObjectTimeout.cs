using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_WorldObjectTimeout : QuestPart_Delay
{
	public WorldObject worldObject;

	public bool destroyOnCleanup;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			if (worldObject != null)
			{
				yield return worldObject;
			}
		}
	}

	public override string ExtraInspectString(ISelectable target)
	{
		if (target == worldObject)
		{
			if (target is Site site)
			{
				for (int i = 0; i < site.parts.Count; i++)
				{
					if (site.parts[i].def.handlesWorldObjectTimeoutInspectString)
					{
						return null;
					}
				}
			}
			return "WorldObjectTimeout".Translate(base.TicksLeft.ToStringTicksToPeriod());
		}
		return null;
	}

	protected override void DelayFinished()
	{
		QuestPart_DestroyWorldObject.TryRemove(worldObject);
		if (worldObject != null)
		{
			Complete(worldObject.Named("SUBJECT"));
		}
		else
		{
			Complete();
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (destroyOnCleanup)
		{
			QuestPart_DestroyWorldObject.TryRemove(worldObject);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref worldObject, "worldObject");
		Scribe_Values.Look(ref destroyOnCleanup, "destroyOnCleanup", defaultValue: false);
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		if (TileFinder.TryFindNewSiteTile(out var tile))
		{
			worldObject = SiteMaker.MakeSite((SitePartDef)null, tile, (Faction)null, ifHostileThenMustRemainHostile: true, (float?)null, (WorldObjectDef)null);
		}
	}
}
