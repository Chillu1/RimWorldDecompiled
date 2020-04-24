using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Zone_Stockpile : Zone, ISlotGroupParent, IStoreSettingsParent, IHaulDestination
	{
		public StorageSettings settings;

		public SlotGroup slotGroup;

		private static readonly ITab[] ITabs = new ITab[1]
		{
			new ITab_Storage()
		};

		public bool StorageTabVisible => true;

		public bool IgnoreStoredThingsBeauty => false;

		protected override Color NextZoneColor => ZoneColorUtility.NextStorageZoneColor();

		public Zone_Stockpile()
		{
			slotGroup = new SlotGroup(this);
		}

		public Zone_Stockpile(StorageSettingsPreset preset, ZoneManager zoneManager)
			: base(preset.PresetName(), zoneManager)
		{
			settings = new StorageSettings(this);
			settings.SetFromPreset(preset);
			slotGroup = new SlotGroup(this);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref settings, "settings", this);
		}

		public override void AddCell(IntVec3 sq)
		{
			base.AddCell(sq);
			if (slotGroup != null)
			{
				slotGroup.Notify_AddedCell(sq);
			}
		}

		public override void RemoveCell(IntVec3 sq)
		{
			base.RemoveCell(sq);
			slotGroup.Notify_LostCell(sq);
		}

		public override void PostDeregister()
		{
			base.PostDeregister();
			BillUtility.Notify_ZoneStockpileRemoved(this);
		}

		public override IEnumerable<InspectTabBase> GetInspectTabs()
		{
			return ITabs;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(settings))
			{
				yield return item;
			}
		}

		public override IEnumerable<Gizmo> GetZoneAddGizmos()
		{
			yield return DesignatorUtility.FindAllowedDesignator<Designator_ZoneAddStockpile_Expand>();
		}

		public SlotGroup GetSlotGroup()
		{
			return slotGroup;
		}

		public IEnumerable<IntVec3> AllSlotCells()
		{
			for (int i = 0; i < cells.Count; i++)
			{
				yield return cells[i];
			}
		}

		public List<IntVec3> AllSlotCellsList()
		{
			return cells;
		}

		public StorageSettings GetParentStoreSettings()
		{
			return null;
		}

		public StorageSettings GetStoreSettings()
		{
			return settings;
		}

		public bool Accepts(Thing t)
		{
			return settings.AllowedToAccept(t);
		}

		public string SlotYielderLabel()
		{
			return label;
		}

		public void Notify_ReceivedThing(Thing newItem)
		{
			if (newItem.def.storedConceptLearnOpportunity != null)
			{
				LessonAutoActivator.TeachOpportunity(newItem.def.storedConceptLearnOpportunity, OpportunityType.GoodToKnow);
			}
		}

		public void Notify_LostThing(Thing newItem)
		{
		}
	}
}
