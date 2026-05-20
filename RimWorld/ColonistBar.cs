using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public class ColonistBar
{
	public struct Entry
	{
		public Pawn pawn;

		public Map map;

		public int group;

		public Action<int, int> reorderAction;

		public Action<int, Vector2> extraDraggedItemOnGUI;

		public Entry(Pawn pawn, Map map, int group)
		{
			this.pawn = pawn;
			this.map = map;
			this.group = group;
			reorderAction = delegate(int from, int to)
			{
				Find.ColonistBar.Reorder(from, to, group);
			};
			extraDraggedItemOnGUI = delegate(int index, Vector2 dragStartPos)
			{
				Find.ColonistBar.DrawColonistMouseAttachment(index, dragStartPos, group);
			};
		}
	}

	public ColonistBarColonistDrawer drawer = new ColonistBarColonistDrawer();

	private ColonistBarDrawLocsFinder drawLocsFinder = new ColonistBarDrawLocsFinder();

	private List<Entry> cachedEntries = new List<Entry>();

	private List<Vector2> cachedDrawLocs = new List<Vector2>();

	private List<int> cachedReorderableGroups = new List<int>();

	private float cachedScale = 1f;

	private bool entriesDirty = true;

	private List<Pawn> colonistsToHighlight = new List<Pawn>();

	public static readonly Texture2D BGTex = Command.BGTex;

	public static readonly Vector2 BaseSize = new Vector2(48f, 48f);

	public const float BaseSelectedTexJump = 20f;

	public const float BaseSelectedTexScale = 0.4f;

	public const float EntryInAnotherMapAlpha = 0.4f;

	public const float BaseSpaceBetweenGroups = 25f;

	public const float BaseSpaceBetweenColonistsHorizontal = 24f;

	public const float BaseSpaceBetweenColonistsVertical = 32f;

	private const float WeaponIconOffsetScaleFactor = 1.05f;

	private const float WeaponIconScaleFactor = 0.75f;

	private static List<Pawn> tmpPawns = new List<Pawn>();

	private static List<Map> tmpMaps = new List<Map>();

	private static List<Caravan> tmpCaravans = new List<Caravan>();

	private static List<Pawn> tmpColonistsInOrder = new List<Pawn>();

	private static List<Pair<Thing, Map>> tmpColonistsWithMap = new List<Pair<Thing, Map>>();

	private static List<Thing> tmpColonists = new List<Thing>();

	private static List<Thing> tmpMapColonistsOrCorpsesInScreenRect = new List<Thing>();

	private static List<Pawn> tmpCaravanPawns = new List<Pawn>();

	public List<Entry> Entries
	{
		get
		{
			CheckRecacheEntries();
			return cachedEntries;
		}
	}

	private bool ShowGroupFrames
	{
		get
		{
			List<Entry> entries = Entries;
			int num = -1;
			for (int i = 0; i < entries.Count; i++)
			{
				num = Mathf.Max(num, entries[i].group);
			}
			return num >= 1;
		}
	}

	public float Scale => cachedScale;

	public List<Vector2> DrawLocs => cachedDrawLocs;

	public Vector2 Size => BaseSize * Scale;

	public float SpaceBetweenColonistsHorizontal => 24f * Scale;

	private bool Visible
	{
		get
		{
			if (UI.screenWidth < 800 || UI.screenHeight < 500)
			{
				return false;
			}
			if (Find.TilePicker.Active)
			{
				return false;
			}
			return true;
		}
	}

	public void MarkColonistsDirty()
	{
		entriesDirty = true;
	}

	public void ColonistBarOnGUI()
	{
		if (!Visible)
		{
			return;
		}
		if (Event.current.type != EventType.Layout)
		{
			List<Entry> entries = Entries;
			int num = -1;
			bool showGroupFrames = ShowGroupFrames;
			int value = -1;
			for (int i = 0; i < cachedDrawLocs.Count; i++)
			{
				Rect rect = new Rect(cachedDrawLocs[i].x, cachedDrawLocs[i].y, Size.x, Size.y);
				Entry entry = entries[i];
				bool flag = num != entry.group;
				num = entry.group;
				if (Event.current.type == EventType.Repaint)
				{
					if (flag)
					{
						value = ReorderableWidget.NewGroup(entry.reorderAction, ReorderableDirection.Horizontal, new Rect(0f, 0f, UI.screenWidth, UI.screenHeight), SpaceBetweenColonistsHorizontal, entry.extraDraggedItemOnGUI);
					}
					cachedReorderableGroups[i] = value;
				}
				bool reordering;
				if (entry.pawn != null)
				{
					drawer.HandleClicks(rect, entry.pawn, cachedReorderableGroups[i], out reordering);
				}
				else
				{
					reordering = false;
				}
				if (Event.current.type != EventType.Repaint)
				{
					continue;
				}
				if (flag && showGroupFrames)
				{
					drawer.DrawGroupFrame(entry.group);
				}
				if (entry.pawn != null)
				{
					drawer.DrawColonist(rect, entry.pawn, entry.map, colonistsToHighlight.Contains(entry.pawn), reordering);
					ThingWithComps thingWithComps = entry.pawn.equipment?.Primary;
					if ((Prefs.ShowWeaponsUnderPortraitMode == ShowWeaponsUnderPortraitMode.Always || (Prefs.ShowWeaponsUnderPortraitMode == ShowWeaponsUnderPortraitMode.WhileDrafted && entry.pawn.Drafted)) && thingWithComps != null && thingWithComps.def.IsWeapon)
					{
						Widgets.ThingIcon(new Rect(rect.x, rect.y + rect.height * 1.05f, rect.width, rect.height).ScaledBy(0.75f), thingWithComps, 1f, null, stackOfOne: true);
					}
				}
			}
			num = -1;
			if (showGroupFrames)
			{
				for (int j = 0; j < cachedDrawLocs.Count; j++)
				{
					Entry entry2 = entries[j];
					bool num2 = num != entry2.group;
					num = entry2.group;
					if (num2)
					{
						drawer.HandleGroupFrameClicks(entry2.group);
					}
				}
			}
		}
		if (Event.current.type == EventType.Repaint)
		{
			colonistsToHighlight.Clear();
		}
	}

	private void CheckRecacheEntries()
	{
		if (!entriesDirty)
		{
			return;
		}
		entriesDirty = false;
		cachedEntries.Clear();
		int num = 0;
		if (Find.PlaySettings.showColonistBar)
		{
			tmpMaps.Clear();
			tmpMaps.AddRange(Find.Maps);
			tmpMaps.SortBy((Map x) => !x.IsPlayerHome, (Map x) => x.uniqueID);
			for (int num2 = 0; num2 < tmpMaps.Count; num2++)
			{
				tmpPawns.Clear();
				tmpPawns.AddRange(tmpMaps[num2].mapPawns.FreeColonists);
				tmpPawns.AddRange(tmpMaps[num2].mapPawns.ColonySubhumansControllable);
				List<Thing> list = tmpMaps[num2].listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
				for (int num3 = 0; num3 < list.Count; num3++)
				{
					if (!list[num3].IsDessicated())
					{
						Pawn innerPawn = ((Corpse)list[num3]).InnerPawn;
						if (innerPawn != null && innerPawn.IsColonist)
						{
							tmpPawns.Add(innerPawn);
						}
					}
				}
				IReadOnlyList<Pawn> allPawnsSpawned = tmpMaps[num2].mapPawns.AllPawnsSpawned;
				for (int num4 = 0; num4 < allPawnsSpawned.Count; num4++)
				{
					if (allPawnsSpawned[num4].carryTracker.CarriedThing is Corpse corpse && !corpse.IsDessicated() && corpse.InnerPawn.IsColonist)
					{
						tmpPawns.Add(corpse.InnerPawn);
					}
				}
				foreach (Pawn tmpPawn in tmpPawns)
				{
					if (tmpPawn.playerSettings.displayOrder == -9999999)
					{
						tmpPawn.playerSettings.displayOrder = Mathf.Max(tmpPawns.MaxBy((Pawn p) => p.playerSettings.displayOrder).playerSettings.displayOrder, 0) + 1;
					}
				}
				PlayerPawnsDisplayOrderUtility.Sort(tmpPawns);
				foreach (Pawn tmpPawn2 in tmpPawns)
				{
					cachedEntries.Add(new Entry(tmpPawn2, tmpMaps[num2], num));
				}
				if (!tmpPawns.Any())
				{
					cachedEntries.Add(new Entry(null, tmpMaps[num2], num));
				}
				num++;
			}
			tmpCaravans.Clear();
			tmpCaravans.AddRange(Find.WorldObjects.Caravans);
			tmpCaravans.SortBy((Caravan x) => x.ID);
			for (int num5 = 0; num5 < tmpCaravans.Count; num5++)
			{
				if (!tmpCaravans[num5].IsPlayerControlled)
				{
					continue;
				}
				tmpPawns.Clear();
				tmpPawns.AddRange(tmpCaravans[num5].PawnsListForReading);
				PlayerPawnsDisplayOrderUtility.Sort(tmpPawns);
				for (int num6 = 0; num6 < tmpPawns.Count; num6++)
				{
					if (tmpPawns[num6].IsColonist || tmpPawns[num6].IsColonySubhumanPlayerControlled)
					{
						cachedEntries.Add(new Entry(tmpPawns[num6], null, num));
					}
				}
				num++;
			}
		}
		cachedReorderableGroups.Clear();
		foreach (Entry cachedEntry in cachedEntries)
		{
			_ = cachedEntry;
			cachedReorderableGroups.Add(-1);
		}
		drawer.Notify_RecachedEntries();
		tmpPawns.Clear();
		tmpMaps.Clear();
		tmpCaravans.Clear();
		drawLocsFinder.CalculateDrawLocs(cachedDrawLocs, out cachedScale, num);
	}

	public float GetEntryRectAlpha(Rect rect)
	{
		if (Messages.CollidesWithAnyMessage(rect, out var messageAlpha))
		{
			return Mathf.Lerp(1f, 0.2f, messageAlpha);
		}
		return 1f;
	}

	public void Highlight(Pawn pawn)
	{
		if (Visible && !colonistsToHighlight.Contains(pawn))
		{
			colonistsToHighlight.Add(pawn);
		}
	}

	public void Reorder(int from, int to, int entryGroup)
	{
		int num = 0;
		Pawn pawn = null;
		Pawn pawn2 = null;
		Pawn pawn3 = null;
		for (int i = 0; i < cachedEntries.Count; i++)
		{
			if (cachedEntries[i].group == entryGroup && cachedEntries[i].pawn != null)
			{
				if (num == from)
				{
					pawn = cachedEntries[i].pawn;
				}
				if (num == to)
				{
					pawn2 = cachedEntries[i].pawn;
				}
				pawn3 = cachedEntries[i].pawn;
				num++;
			}
		}
		if (pawn == null)
		{
			return;
		}
		int num2 = pawn2?.playerSettings.displayOrder ?? (pawn3.playerSettings.displayOrder + 1);
		for (int j = 0; j < cachedEntries.Count; j++)
		{
			Pawn pawn4 = cachedEntries[j].pawn;
			if (pawn4 == null)
			{
				continue;
			}
			if (pawn4.playerSettings.displayOrder == num2)
			{
				if (pawn2 != null && cachedEntries[j].group == entryGroup)
				{
					pawn4.playerSettings.displayOrder++;
				}
			}
			else if (pawn4.playerSettings.displayOrder > num2)
			{
				pawn4.playerSettings.displayOrder++;
			}
			else
			{
				pawn4.playerSettings.displayOrder--;
			}
		}
		pawn.playerSettings.displayOrder = num2;
		MarkColonistsDirty();
		MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
	}

	public void DrawColonistMouseAttachment(int index, Vector2 dragStartPos, int entryGroup)
	{
		Pawn pawn = null;
		Vector2 vector = default(Vector2);
		int num = 0;
		for (int i = 0; i < cachedEntries.Count; i++)
		{
			if (cachedEntries[i].group == entryGroup && cachedEntries[i].pawn != null)
			{
				if (num == index)
				{
					pawn = cachedEntries[i].pawn;
					vector = cachedDrawLocs[i];
					break;
				}
				num++;
			}
		}
		if (pawn != null)
		{
			RenderTexture iconTex = PortraitsCache.Get(pawn, ColonistBarColonistDrawer.PawnTextureSize, Rot4.South, ColonistBarColonistDrawer.PawnTextureCameraOffset, 1.28205f);
			Rect rect = new Rect(vector.x, vector.y, Size.x, Size.y);
			Rect pawnTextureRect = drawer.GetPawnTextureRect(rect.position);
			pawnTextureRect.position += Event.current.mousePosition - dragStartPos;
			Rect? customRect = pawnTextureRect;
			GenUI.DrawMouseAttachment(iconTex, "", 0f, default(Vector2), customRect);
		}
	}

	public bool AnyColonistOrCorpseAt(Vector2 pos)
	{
		if (!TryGetEntryAt(pos, out var entry))
		{
			return false;
		}
		return entry.pawn != null;
	}

	public bool TryGetEntryAt(Vector2 pos, out Entry entry)
	{
		List<Vector2> drawLocs = DrawLocs;
		List<Entry> entries = Entries;
		Vector2 size = Size;
		for (int i = 0; i < drawLocs.Count; i++)
		{
			if (new Rect(drawLocs[i].x, drawLocs[i].y, size.x, size.y).Contains(pos))
			{
				entry = entries[i];
				return true;
			}
		}
		entry = default(Entry);
		return false;
	}

	public List<Pawn> GetColonistsInOrder()
	{
		List<Entry> entries = Entries;
		tmpColonistsInOrder.Clear();
		for (int i = 0; i < entries.Count; i++)
		{
			if (entries[i].pawn != null)
			{
				tmpColonistsInOrder.Add(entries[i].pawn);
			}
		}
		return tmpColonistsInOrder;
	}

	public List<Thing> ColonistsOrCorpsesInScreenRect(Rect rect)
	{
		List<Vector2> drawLocs = DrawLocs;
		List<Entry> entries = Entries;
		Vector2 size = Size;
		tmpColonistsWithMap.Clear();
		for (int i = 0; i < drawLocs.Count; i++)
		{
			if (rect.Overlaps(new Rect(drawLocs[i].x, drawLocs[i].y, size.x, size.y)))
			{
				Pawn pawn = entries[i].pawn;
				if (pawn != null)
				{
					Thing first = ((!pawn.Dead || pawn.Corpse == null || !pawn.Corpse.SpawnedOrAnyParentSpawned) ? ((Thing)pawn) : ((Thing)pawn.Corpse));
					tmpColonistsWithMap.Add(new Pair<Thing, Map>(first, entries[i].map));
				}
			}
		}
		if (WorldRendererUtility.WorldSelected && tmpColonistsWithMap.Any((Pair<Thing, Map> x) => x.Second == null))
		{
			tmpColonistsWithMap.RemoveAll((Pair<Thing, Map> x) => x.Second != null);
		}
		else if (tmpColonistsWithMap.Any((Pair<Thing, Map> x) => x.Second == Find.CurrentMap))
		{
			tmpColonistsWithMap.RemoveAll((Pair<Thing, Map> x) => x.Second != Find.CurrentMap);
		}
		tmpColonists.Clear();
		for (int num = 0; num < tmpColonistsWithMap.Count; num++)
		{
			tmpColonists.Add(tmpColonistsWithMap[num].First);
		}
		tmpColonistsWithMap.Clear();
		return tmpColonists;
	}

	public List<Thing> MapColonistsOrCorpsesInScreenRect(Rect rect)
	{
		tmpMapColonistsOrCorpsesInScreenRect.Clear();
		if (!Visible)
		{
			return tmpMapColonistsOrCorpsesInScreenRect;
		}
		List<Thing> list = ColonistsOrCorpsesInScreenRect(rect);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Spawned)
			{
				tmpMapColonistsOrCorpsesInScreenRect.Add(list[i]);
			}
		}
		return tmpMapColonistsOrCorpsesInScreenRect;
	}

	public List<Pawn> CaravanMembersInScreenRect(Rect rect)
	{
		tmpCaravanPawns.Clear();
		if (!Visible)
		{
			return tmpCaravanPawns;
		}
		List<Thing> list = ColonistsOrCorpsesInScreenRect(rect);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is Pawn pawn && pawn.IsCaravanMember())
			{
				tmpCaravanPawns.Add(pawn);
			}
		}
		return tmpCaravanPawns;
	}

	public List<Caravan> CaravanMembersCaravansInScreenRect(Rect rect)
	{
		tmpCaravans.Clear();
		if (!Visible)
		{
			return tmpCaravans;
		}
		List<Pawn> list = CaravanMembersInScreenRect(rect);
		for (int i = 0; i < list.Count; i++)
		{
			tmpCaravans.Add(list[i].GetCaravan());
		}
		return tmpCaravans;
	}

	public Caravan CaravanMemberCaravanAt(Vector2 at)
	{
		if (!Visible)
		{
			return null;
		}
		if (ColonistOrCorpseAt(at) is Pawn pawn && pawn.IsCaravanMember())
		{
			return pawn.GetCaravan();
		}
		return null;
	}

	public Thing ColonistOrCorpseAt(Vector2 pos)
	{
		if (!Visible)
		{
			return null;
		}
		if (!TryGetEntryAt(pos, out var entry))
		{
			return null;
		}
		Pawn pawn = entry.pawn;
		if (pawn != null && pawn.Dead && pawn.Corpse != null && pawn.Corpse.SpawnedOrAnyParentSpawned)
		{
			return pawn.Corpse;
		}
		return pawn;
	}
}
