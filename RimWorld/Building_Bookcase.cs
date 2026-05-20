using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Building_Bookcase : Building, IThingHolderEvents<Book>, IHaulEnroute, ILoadReferenceable, IStorageGroupMember, IHaulDestination, IStoreSettingsParent, IHaulSource, IThingHolder, ISearchableContents, IBeautyContainer
{
	private ThingOwner<Book> innerContainer;

	private StorageSettings settings;

	private StorageGroup storageGroup;

	private Graphic bookendGraphicEastInt;

	private Graphic bookendGraphicNorthInt;

	private static readonly Vector3 DrawOffset = new Vector3(0f, 0.018292684f, 0f);

	private static readonly Vector3 DrawOffsetBookcaseEnd = new Vector3(0f, 0.07317074f, 0f);

	private const float BookWidthEastWest = 0.16f;

	private const float BookWidthNorthSouth = 0.155f;

	private static readonly Vector3[] StandardBookshelfRotOffsets = new Vector3[4]
	{
		new Vector3(-0.081f, 0f, 0f),
		new Vector3(-0.082f, 0f, 0.02f),
		new Vector3(0.08f, 0f, 0.08f),
		new Vector3(0.082f, 0f, -0.15f)
	};

	private float cachedBeauty = -1f;

	private Graphic BookendGraphicEast => bookendGraphicEastInt ?? (bookendGraphicEastInt = def.building.bookendGraphicEast.GraphicColoredFor(this));

	private Graphic BookendGraphicNorth => bookendGraphicNorthInt ?? (bookendGraphicNorthInt = def.building.bookendGraphicNorth.GraphicColoredFor(this));

	public int MaximumBooks => def.building.maxItemsInCell * def.size.Area;

	public IReadOnlyList<Book> HeldBooks => innerContainer.InnerListForReading;

	public float ReadingBonus => this.GetRoom()?.GetStat(RoomStatDefOf.ReadingBonus) ?? 0f;

	protected virtual Vector3[] RotOffsets => StandardBookshelfRotOffsets;

	public IEnumerable<float> CellsFilledPercentage
	{
		get
		{
			int books = HeldBooks.Count;
			for (int i = 0; i < def.size.Area; i++)
			{
				int num = Mathf.Min(books, def.building.maxItemsInCell);
				books -= num;
				yield return Mathf.Clamp01((float)num / (float)def.building.maxItemsInCell);
			}
		}
	}

	public ThingOwner SearchableContents => innerContainer;

	public bool StorageTabVisible => true;

	public bool HaulSourceEnabled => true;

	public bool HaulDestinationEnabled => true;

	StorageGroup IStorageGroupMember.Group
	{
		get
		{
			return storageGroup;
		}
		set
		{
			storageGroup = value;
		}
	}

	bool IStorageGroupMember.DrawConnectionOverlay => base.Spawned;

	Map IStorageGroupMember.Map => base.MapHeld;

	string IStorageGroupMember.StorageGroupTag => def.building.storageGroupTag;

	StorageSettings IStorageGroupMember.StoreSettings => GetStoreSettings();

	StorageSettings IStorageGroupMember.ParentStoreSettings => GetParentStoreSettings();

	StorageSettings IStorageGroupMember.ThingStoreSettings => settings;

	bool IStorageGroupMember.DrawStorageTab => true;

	bool IStorageGroupMember.ShowRenameButton => base.Faction == Faction.OfPlayer;

	public float BeautyOffset
	{
		get
		{
			if (cachedBeauty < 0f)
			{
				cachedBeauty = HeldBooks.Aggregate(0f, (float beauty, Book book) => beauty + book.GetBeauty(this.GetRoom().PsychologicallyOutdoors));
			}
			return cachedBeauty - 0.001f;
		}
	}

	string IBeautyContainer.BeautyOffsetExplanation => string.Format("{0}: {1}", "ContainedBookBeauty".Translate(), BeautyOffset.ToStringWithSign());

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public StorageSettings GetStoreSettings()
	{
		if (storageGroup != null)
		{
			return storageGroup.GetStoreSettings();
		}
		return settings;
	}

	public StorageSettings GetParentStoreSettings()
	{
		return def.building.fixedStorageSettings;
	}

	public bool Accepts(Thing t)
	{
		if (HeldBooks.Count >= MaximumBooks)
		{
			if (!(t is Book item))
			{
				return false;
			}
			if (!innerContainer.InnerListForReading.Contains(item))
			{
				return false;
			}
		}
		if (GetStoreSettings().AllowedToAccept(t))
		{
			return innerContainer.CanAcceptAnyOf(t);
		}
		return false;
	}

	public int SpaceRemainingFor(ThingDef _)
	{
		return MaximumBooks - HeldBooks.Count;
	}

	public void Notify_SettingsChanged()
	{
		if (base.Spawned)
		{
			base.MapHeld.listerHaulables.Notify_HaulSourceChanged(this);
		}
	}

	public void Notify_ItemAdded(Book item)
	{
		DirtyRoomStats();
		cachedBeauty = -1f;
		base.MapHeld.listerHaulables.Notify_AddedThing(item);
	}

	public void Notify_ItemRemoved(Book item)
	{
		DirtyRoomStats();
		cachedBeauty = -1f;
	}

	public Building_Bookcase()
	{
		innerContainer = new ThingOwner<Book>(this, oneStackOnly: false);
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (storageGroup != null && map != storageGroup.Map)
		{
			StorageSettings storeSettings = storageGroup.GetStoreSettings();
			storageGroup.RemoveMember(this);
			storageGroup = null;
			settings.CopyFrom(storeSettings);
		}
	}

	public override void PostMake()
	{
		base.PostMake();
		settings = new StorageSettings(this);
		if (def.building.defaultStorageSettings != null)
		{
			settings.CopyFrom(def.building.defaultStorageSettings);
		}
		DirtyRoomStats();
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		drawLoc -= Altitudes.AltIncVect * 2f;
		base.DrawAt(drawLoc, flip);
		Rot4 rot = base.Rotation.Rotated(RotationDirection.Counterclockwise);
		float num = ((base.Rotation == Rot4.North || base.Rotation == Rot4.South) ? 0.155f : 0.16f);
		Vector3 vector = rot.FacingCell.ToVector3() * num;
		Vector3 vector2 = rot.FacingCell.ToVector3() * ((float)(-MaximumBooks) * num * 0.5f);
		Vector3 vector3 = RotOffsets[base.Rotation.AsInt];
		for (int i = 0; i < HeldBooks.Count; i++)
		{
			Book book = HeldBooks[i];
			Rot4 opposite = base.Rotation.Opposite;
			if (opposite == Rot4.East || opposite == Rot4.West)
			{
				opposite = opposite.Opposite;
			}
			Vector3 loc = drawLoc + vector2 + vector3 + DrawOffset + vector * i;
			book.VerticalGraphic.Draw(loc, opposite, this);
		}
		if (base.Rotation != Rot4.South)
		{
			if (base.Rotation != Rot4.North && def.building.bookendGraphicEast != null)
			{
				BookendGraphicEast.Draw(drawLoc + DrawOffsetBookcaseEnd, Rot4.North, this);
			}
			else if (base.Rotation == Rot4.North && def.building.bookendGraphicNorth != null)
			{
				BookendGraphicNorth.Draw(drawLoc + DrawOffsetBookcaseEnd, Rot4.North, this);
			}
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		if (storageGroup != null)
		{
			storageGroup?.RemoveMember(this);
			storageGroup = null;
		}
		if (mode != DestroyMode.WillReplace)
		{
			innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
		}
		base.DeSpawn(mode);
	}

	public override void DrawExtraSelectionOverlays()
	{
		base.DrawExtraSelectionOverlays();
		if (Find.Selector.SingleSelectedThing == this)
		{
			Room room = this.GetRoom();
			if (room != null && room.ProperRoom)
			{
				room.DrawFieldEdges();
			}
		}
		StorageGroupUtility.DrawSelectionOverlaysFor(this);
	}

	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (base.Spawned)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			if (storageGroup != null)
			{
				text += string.Format("{0}: {1} ", "StorageGroupLabel".Translate(), storageGroup.RenamableLabel.CapitalizeFirst());
				text = ((storageGroup.MemberCount <= 1) ? ((string)(text + ("(" + "OneBuilding".Translate() + ")\n"))) : ((string)(text + ("(" + "NumBuildings".Translate(storageGroup.MemberCount).CapitalizeFirst() + ")\n"))));
			}
			text += string.Format("{0}: {1} / {2}", "BookshelfStoredInspect".Translate(), HeldBooks.Count, MaximumBooks);
			text += string.Format("\n{0}: {1}", "BookshelfReadingBonusInspect".Translate(), Mathf.Max(ReadingBonus - 1f, 0f).ToStringPercent());
			if (this.IsOutside())
			{
				text += string.Format(" ({0})", "Outdoors".Translate()).Colorize(Color.gray).ResolveTags();
			}
		}
		return text;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		foreach (Gizmo item2 in StorageSettingsClipboard.CopyPasteGizmosFor(GetStoreSettings()))
		{
			yield return item2;
		}
		if (StorageTabVisible && base.MapHeld != null)
		{
			foreach (Gizmo item3 in StorageGroupUtility.StorageGroupMemberGizmos(this))
			{
				yield return item3;
			}
		}
		if (!DebugSettings.godMode)
		{
			yield break;
		}
		yield return new Command_Action
		{
			defaultLabel = "DEV: Fill",
			action = delegate
			{
				for (int i = HeldBooks.Count; i < MaximumBooks; i++)
				{
					Book item = BookUtility.MakeBook(ThingDefOf.TextBook, ArtGenerationContext.Outsider);
					innerContainer.TryAdd(item);
				}
			}
		};
	}

	public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
	{
		foreach (FloatMenuOption floatMenuOption in HaulSourceUtility.GetFloatMenuOptions(this, selPawn))
		{
			yield return floatMenuOption;
		}
		foreach (Book heldBook in HeldBooks)
		{
			foreach (FloatMenuOption floatMenuOption2 in heldBook.GetFloatMenuOptions(selPawn))
			{
				yield return floatMenuOption2;
			}
		}
		foreach (FloatMenuOption floatMenuOption3 in base.GetFloatMenuOptions(selPawn))
		{
			yield return floatMenuOption3;
		}
	}

	private void DirtyRoomStats()
	{
		this.GetRoom()?.Notify_ContainedThingSpawnedOrDespawned(this);
	}

	public override void Notify_ColorChanged()
	{
		bookendGraphicEastInt = null;
		bookendGraphicNorthInt = null;
		base.Notify_ColorChanged();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		Scribe_Deep.Look(ref settings, "settings", this);
		Scribe_References.Look(ref storageGroup, "storageGroup");
	}
}
