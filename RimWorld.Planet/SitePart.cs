using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class SitePart : IExposable, IThingHolder
	{
		public Site site;

		public SitePartDef def;

		public bool hidden;

		public SitePartParams parms;

		public ThingOwner things;

		public int lastRaidTick = -1;

		public Thing conditionCauser;

		public bool conditionCauserWasSpawned;

		private const float AutoFoodLevel = 0.8f;

		public IThingHolder ParentHolder => site;

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

		public void SitePartTick()
		{
			if (things == null)
			{
				return;
			}
			if (things.contentsLookMode == LookMode.Deep)
			{
				things.ThingOwnerTick();
			}
			for (int i = 0; i < things.Count; i++)
			{
				Pawn pawn = things[i] as Pawn;
				if (pawn != null && !pawn.Destroyed && pawn.needs.food != null)
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
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref lastRaidTick, "lastRaidTick", -1);
			Scribe_Values.Look(ref conditionCauserWasSpawned, "conditionCauserWasSpawned", defaultValue: false);
			Scribe_Values.Look(ref hidden, "hidden", defaultValue: false);
			if (conditionCauserWasSpawned)
			{
				Scribe_References.Look(ref conditionCauser, "conditionCauser");
			}
			else
			{
				Scribe_Deep.Look(ref conditionCauser, "conditionCauser");
			}
		}
	}
}
