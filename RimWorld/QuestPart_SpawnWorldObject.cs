using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_SpawnWorldObject : QuestPart
	{
		public string inSignal;

		public WorldObject worldObject;

		public List<ThingDef> defsToExcludeFromHyperlinks;

		private bool spawned;

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

		public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
		{
			get
			{
				foreach (Dialog_InfoCard.Hyperlink hyperlink in base.Hyperlinks)
				{
					yield return hyperlink;
				}
				if (worldObject != null)
				{
					ItemStashContentsComp component = worldObject.GetComponent<ItemStashContentsComp>();
					if (component != null)
					{
						foreach (Thing item in (IEnumerable<Thing>)component.contents)
						{
							ThingDef def = item.GetInnerIfMinified().def;
							if (defsToExcludeFromHyperlinks.NullOrEmpty() || !defsToExcludeFromHyperlinks.Contains(def))
							{
								yield return new Dialog_InfoCard.Hyperlink(def);
							}
						}
					}
				}
				Site site;
				if ((site = (worldObject as Site)) != null)
				{
					foreach (SitePart part in site.parts)
					{
						if (part.things != null)
						{
							foreach (Thing item2 in (IEnumerable<Thing>)part.things)
							{
								ThingDef def2 = item2.GetInnerIfMinified().def;
								if (defsToExcludeFromHyperlinks.NullOrEmpty() || !defsToExcludeFromHyperlinks.Contains(def2))
								{
									yield return new Dialog_InfoCard.Hyperlink(def2);
								}
							}
						}
					}
				}
			}
		}

		public override bool IncreasesPopulation
		{
			get
			{
				Site site = worldObject as Site;
				if (site != null && site.IncreasesPopulation)
				{
					return true;
				}
				return false;
			}
		}

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (!(signal.tag == inSignal) || spawned)
			{
				return;
			}
			int tile = worldObject.Tile;
			if (tile == -1)
			{
				if (!TileFinder.TryFindNewSiteTile(out tile))
				{
					tile = -1;
				}
			}
			else if (Find.WorldObjects.AnyWorldObjectAt(tile) && !TileFinder.TryFindPassableTileWithTraversalDistance(tile, 1, 50, out tile, (int x) => !Find.WorldObjects.AnyWorldObjectAt(x), ignoreFirstTilePassability: false, preferCloserTiles: true))
			{
				tile = -1;
			}
			if (tile != -1)
			{
				worldObject.Tile = tile;
				Find.WorldObjects.Add(worldObject);
				spawned = true;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref spawned, "spawned", defaultValue: false);
			Scribe_Collections.Look(ref defsToExcludeFromHyperlinks, "defsToExcludeFromHyperlinks", LookMode.Def);
			if (spawned)
			{
				Scribe_References.Look(ref worldObject, "worldObject");
			}
			else
			{
				Scribe_Deep.Look(ref worldObject, "worldObject");
			}
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
			if (TileFinder.TryFindNewSiteTile(out int tile))
			{
				worldObject = SiteMaker.MakeSite((SitePartDef)null, tile, (Faction)null, ifHostileThenMustRemainHostile: true, (float?)null);
			}
		}
	}
}
