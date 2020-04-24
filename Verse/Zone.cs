using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public abstract class Zone : IExposable, ISelectable, ILoadReferenceable
	{
		public ZoneManager zoneManager;

		public int ID = -1;

		public string label;

		private string baseLabel;

		public List<IntVec3> cells = new List<IntVec3>();

		private bool cellsShuffled;

		public Color color = Color.white;

		private Material materialInt;

		public bool hidden;

		private int lastStaticFireCheckTick = -9999;

		private bool lastStaticFireCheckResult;

		private const int StaticFireCheckInterval = 1000;

		private static BoolGrid extantGrid;

		private static BoolGrid foundGrid;

		public Map Map => zoneManager.map;

		public IntVec3 Position
		{
			get
			{
				if (cells.Count == 0)
				{
					return IntVec3.Invalid;
				}
				return cells[0];
			}
		}

		public Material Material
		{
			get
			{
				if (materialInt == null)
				{
					materialInt = SolidColorMaterials.SimpleSolidColorMaterial(color);
					materialInt.renderQueue = 3600;
				}
				return materialInt;
			}
		}

		public string BaseLabel => baseLabel;

		public List<IntVec3> Cells
		{
			get
			{
				if (!cellsShuffled)
				{
					cells.Shuffle();
					cellsShuffled = true;
				}
				return cells;
			}
		}

		public IEnumerable<Thing> AllContainedThings
		{
			get
			{
				ThingGrid grids = Map.thingGrid;
				for (int j = 0; j < cells.Count; j++)
				{
					List<Thing> thingList = grids.ThingsListAt(cells[j]);
					for (int i = 0; i < thingList.Count; i++)
					{
						yield return thingList[i];
					}
				}
			}
		}

		public bool ContainsStaticFire
		{
			get
			{
				if (Find.TickManager.TicksGame > lastStaticFireCheckTick + 1000)
				{
					lastStaticFireCheckResult = false;
					for (int i = 0; i < cells.Count; i++)
					{
						if (cells[i].ContainsStaticFire(Map))
						{
							lastStaticFireCheckResult = true;
							break;
						}
					}
				}
				return lastStaticFireCheckResult;
			}
		}

		public virtual bool IsMultiselectable => false;

		protected abstract Color NextZoneColor
		{
			get;
		}

		public IEnumerator<IntVec3> GetEnumerator()
		{
			for (int i = 0; i < cells.Count; i++)
			{
				yield return cells[i];
			}
		}

		public Zone()
		{
		}

		public Zone(string baseName, ZoneManager zoneManager)
		{
			baseLabel = baseName;
			label = zoneManager.NewZoneName(baseName);
			this.zoneManager = zoneManager;
			ID = Find.UniqueIDsManager.GetNextZoneID();
			color = NextZoneColor;
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref ID, "ID", -1);
			Scribe_Values.Look(ref label, "label");
			Scribe_Values.Look(ref baseLabel, "baseLabel");
			Scribe_Values.Look(ref color, "color");
			Scribe_Values.Look(ref hidden, "hidden", defaultValue: false);
			Scribe_Collections.Look(ref cells, "cells", LookMode.Undefined);
			BackCompatibility.PostExposeData(this);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				CheckAddHaulDestination();
			}
		}

		public virtual void AddCell(IntVec3 c)
		{
			if (cells.Contains(c))
			{
				Log.Error("Adding cell to zone which already has it. c=" + c + ", zone=" + this);
				return;
			}
			List<Thing> list = Map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (!thing.def.CanOverlapZones)
				{
					Log.Error("Added zone over zone-incompatible thing " + thing);
					return;
				}
			}
			cells.Add(c);
			zoneManager.AddZoneGridCell(this, c);
			Map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Zone);
			AutoHomeAreaMaker.Notify_ZoneCellAdded(c, this);
			cellsShuffled = false;
		}

		public virtual void RemoveCell(IntVec3 c)
		{
			if (!cells.Contains(c))
			{
				Log.Error("Cannot remove cell from zone which doesn't have it. c=" + c + ", zone=" + this);
				return;
			}
			cells.Remove(c);
			zoneManager.ClearZoneGridCell(c);
			Map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Zone);
			cellsShuffled = false;
			if (cells.Count == 0)
			{
				Deregister();
			}
		}

		public virtual void Delete()
		{
			SoundDefOf.Designate_ZoneDelete.PlayOneShotOnCamera(Map);
			if (cells.Count == 0)
			{
				Deregister();
			}
			else
			{
				while (cells.Count > 0)
				{
					RemoveCell(cells[cells.Count - 1]);
				}
			}
			Find.Selector.Deselect(this);
		}

		public void Deregister()
		{
			zoneManager.DeregisterZone(this);
		}

		public virtual void PostRegister()
		{
			CheckAddHaulDestination();
		}

		public virtual void PostDeregister()
		{
			IHaulDestination haulDestination = this as IHaulDestination;
			if (haulDestination != null)
			{
				Map.haulDestinationManager.RemoveHaulDestination(haulDestination);
			}
		}

		public bool ContainsCell(IntVec3 c)
		{
			for (int i = 0; i < cells.Count; i++)
			{
				if (cells[i] == c)
				{
					return true;
				}
			}
			return false;
		}

		public virtual string GetInspectString()
		{
			return "";
		}

		public virtual IEnumerable<InspectTabBase> GetInspectTabs()
		{
			return null;
		}

		public virtual IEnumerable<Gizmo> GetGizmos()
		{
			Command_Action command_Action = new Command_Action();
			command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/RenameZone");
			command_Action.defaultLabel = "CommandRenameZoneLabel".Translate();
			command_Action.defaultDesc = "CommandRenameZoneDesc".Translate();
			command_Action.action = delegate
			{
				Dialog_RenameZone dialog_RenameZone = new Dialog_RenameZone(this);
				if (KeyBindingDefOf.Misc1.IsDown)
				{
					dialog_RenameZone.WasOpenedByHotkey();
				}
				Find.WindowStack.Add(dialog_RenameZone);
			};
			command_Action.hotKey = KeyBindingDefOf.Misc1;
			yield return command_Action;
			Command_Toggle command_Toggle = new Command_Toggle();
			command_Toggle.icon = ContentFinder<Texture2D>.Get("UI/Commands/HideZone");
			command_Toggle.defaultLabel = (hidden ? "CommandUnhideZoneLabel".Translate() : "CommandHideZoneLabel".Translate());
			command_Toggle.defaultDesc = "CommandHideZoneDesc".Translate();
			command_Toggle.isActive = (() => hidden);
			command_Toggle.toggleAction = delegate
			{
				hidden = !hidden;
				foreach (IntVec3 cell in Cells)
				{
					Map.mapDrawer.MapMeshDirty(cell, MapMeshFlag.Zone);
				}
			};
			command_Toggle.hotKey = KeyBindingDefOf.Misc2;
			yield return command_Toggle;
			foreach (Gizmo zoneAddGizmo in GetZoneAddGizmos())
			{
				yield return zoneAddGizmo;
			}
			Designator designator = DesignatorUtility.FindAllowedDesignator<Designator_ZoneDelete_Shrink>();
			if (designator != null)
			{
				yield return designator;
			}
			Command_Action command_Action2 = new Command_Action();
			command_Action2.icon = TexButton.DeleteX;
			command_Action2.defaultLabel = "CommandDeleteZoneLabel".Translate();
			command_Action2.defaultDesc = "CommandDeleteZoneDesc".Translate();
			command_Action2.action = Delete;
			command_Action2.hotKey = KeyBindingDefOf.Designator_Deconstruct;
			yield return command_Action2;
		}

		public virtual IEnumerable<Gizmo> GetZoneAddGizmos()
		{
			yield break;
		}

		public void CheckContiguous()
		{
			if (cells.Count != 0)
			{
				if (extantGrid == null)
				{
					extantGrid = new BoolGrid(Map);
				}
				else
				{
					extantGrid.ClearAndResizeTo(Map);
				}
				if (foundGrid == null)
				{
					foundGrid = new BoolGrid(Map);
				}
				else
				{
					foundGrid.ClearAndResizeTo(Map);
				}
				for (int i = 0; i < cells.Count; i++)
				{
					extantGrid.Set(cells[i], value: true);
				}
				Predicate<IntVec3> passCheck = delegate(IntVec3 c)
				{
					if (!extantGrid[c])
					{
						return false;
					}
					return (!foundGrid[c]) ? true : false;
				};
				int numFound = 0;
				Action<IntVec3> processor = delegate(IntVec3 c)
				{
					foundGrid.Set(c, value: true);
					numFound++;
				};
				Map.floodFiller.FloodFill(cells[0], passCheck, processor);
				if (numFound < cells.Count)
				{
					foreach (IntVec3 allCell in Map.AllCells)
					{
						if (extantGrid[allCell] && !foundGrid[allCell])
						{
							RemoveCell(allCell);
						}
					}
				}
			}
		}

		private void CheckAddHaulDestination()
		{
			IHaulDestination haulDestination = this as IHaulDestination;
			if (haulDestination != null)
			{
				Map.haulDestinationManager.AddHaulDestination(haulDestination);
			}
		}

		public override string ToString()
		{
			return label;
		}

		public string GetUniqueLoadID()
		{
			return "Zone_" + ID;
		}
	}
}
