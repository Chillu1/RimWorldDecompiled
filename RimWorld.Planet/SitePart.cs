using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class SitePart : IExposable, IThingHolderTickable, IThingHolder
{
	public Site site;

	public SitePartDef def;

	public bool hidden;

	public SitePartParams parms;

	public ThingOwner things;

	public int lastRaidTick = -1;

	public Thing conditionCauser;

	public bool conditionCauserWasSpawned;

	public List<ThingDefCount> lootThings;

	public int expectedEnemyCount = -1;

	public Thing relicThing;

	public bool relicWasSpawned;

	private const float AutoFoodLevel = 0.8f;

	public IThingHolder ParentHolder => site;

	public bool ShouldTickContents
	{
		get
		{
			if (things != null && things.contentsLookMode == LookMode.Deep)
			{
				return !things.dontTickContents;
			}
			return false;
		}
	}

	public SitePart()
	{
	}

	public SitePart(Site site, SitePartDef def, SitePartParams parms)
	{
		this.site = site;
		this.def = def;
		this.parms = parms;
		hidden = def.defaultHidden;
	}

	public void SitePartTickInterval(int delta)
	{
		if (things == null)
		{
			return;
		}
		for (int i = 0; i < things.Count; i++)
		{
			if (things[i] is Pawn { Destroyed: false } pawn && pawn.needs.food != null)
			{
				pawn.needs.food.CurLevelPercentage = 0.8f;
			}
		}
	}

	public void PostDestroy()
	{
		if (things != null)
		{
			things.ClearAndDestroyContentsOrPassToWorld();
		}
		if (!relicWasSpawned)
		{
			relicThing?.Destroy();
			relicThing = null;
		}
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return things;
	}

	public void ExposeData()
	{
		Scribe_Deep.Look(ref parms, "parms");
		Scribe_Deep.Look(ref things, "things", this);
		Scribe_Collections.Look(ref lootThings, "lootThings", LookMode.Deep);
		Scribe_Defs.Look(ref def, "def");
		Scribe_Values.Look(ref lastRaidTick, "lastRaidTick", -1);
		Scribe_Values.Look(ref conditionCauserWasSpawned, "conditionCauserWasSpawned", defaultValue: false);
		Scribe_Values.Look(ref hidden, "hidden", defaultValue: false);
		Scribe_Values.Look(ref expectedEnemyCount, "expectedEnemyCount", -1);
		if (conditionCauserWasSpawned)
		{
			Scribe_References.Look(ref conditionCauser, "conditionCauser");
		}
		else
		{
			Scribe_Deep.Look(ref conditionCauser, "conditionCauser");
		}
		Scribe_Values.Look(ref relicWasSpawned, "relicWasSpawned", defaultValue: false);
		if (relicWasSpawned)
		{
			Scribe_References.Look(ref relicThing, "relicThing");
		}
		else
		{
			Scribe_Deep.Look(ref relicThing, "relicThing");
		}
	}
}
