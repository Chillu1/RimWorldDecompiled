using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Building_Grave : Building_Casket, IStoreSettingsParent, IHaulDestination
	{
		private StorageSettings storageSettings;

		private Graphic cachedGraphicFull;

		public Pawn AssignedPawn
		{
			get
			{
				if (CompAssignableToPawn == null || !CompAssignableToPawn.AssignedPawnsForReading.Any())
				{
					return null;
				}
				return CompAssignableToPawn.AssignedPawnsForReading[0];
			}
		}

		public CompAssignableToPawn_Grave CompAssignableToPawn => GetComp<CompAssignableToPawn_Grave>();

		public override Graphic Graphic
		{
			get
			{
				if (HasCorpse)
				{
					if (def.building.fullGraveGraphicData == null)
					{
						return base.Graphic;
					}
					if (cachedGraphicFull == null)
					{
						cachedGraphicFull = def.building.fullGraveGraphicData.GraphicColoredFor(this);
					}
					return cachedGraphicFull;
				}
				return base.Graphic;
			}
		}

		public bool HasCorpse => Corpse != null;

		public Corpse Corpse
		{
			get
			{
				for (int i = 0; i < innerContainer.Count; i++)
				{
					Corpse corpse = innerContainer[i] as Corpse;
					if (corpse != null)
					{
						return corpse;
					}
				}
				return null;
			}
		}

		public bool StorageTabVisible
		{
			get
			{
				if (AssignedPawn == null)
				{
					return !HasCorpse;
				}
				return false;
			}
		}

		public StorageSettings GetStoreSettings()
		{
			return storageSettings;
		}

		public StorageSettings GetParentStoreSettings()
		{
			return def.building.fixedStorageSettings;
		}

		public override void PostMake()
		{
			base.PostMake();
			storageSettings = new StorageSettings(this);
			if (def.building.defaultStorageSettings != null)
			{
				storageSettings.CopyFrom(def.building.defaultStorageSettings);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref storageSettings, "storageSettings", this);
		}

		public override void EjectContents()
		{
			base.EjectContents();
			if (base.Spawned)
			{
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
			}
		}

		public virtual void Notify_CorpseBuried(Pawn worker)
		{
			CompArt comp = GetComp<CompArt>();
			if (comp != null && !comp.Active)
			{
				comp.JustCreatedBy(worker);
				comp.InitializeArt(Corpse.InnerPawn);
			}
			base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
			worker.records.Increment(RecordDefOf.CorpsesBuried);
			TaleRecorder.RecordTale(TaleDefOf.BuriedCorpse, worker, (Corpse != null) ? Corpse.InnerPawn : null);
		}

		public override bool Accepts(Thing thing)
		{
			if (!base.Accepts(thing))
			{
				return false;
			}
			if (HasCorpse)
			{
				return false;
			}
			if (AssignedPawn != null)
			{
				Corpse corpse = thing as Corpse;
				if (corpse == null)
				{
					return false;
				}
				if (corpse.InnerPawn != AssignedPawn)
				{
					return false;
				}
			}
			else if (!storageSettings.AllowedToAccept(thing))
			{
				return false;
			}
			return true;
		}

		public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (base.TryAcceptThing(thing, allowSpecialEffects))
			{
				Corpse corpse = thing as Corpse;
				if (corpse != null && corpse.InnerPawn.ownership != null && corpse.InnerPawn.ownership.AssignedGrave != this)
				{
					corpse.InnerPawn.ownership.UnclaimGrave();
				}
				if (base.Spawned)
				{
					base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
				}
				return true;
			}
			return false;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (StorageTabVisible)
			{
				foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(storageSettings))
				{
					yield return item;
				}
			}
			if (!HasCorpse)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "CommandGraveAssignColonistLabel".Translate();
				command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner");
				command_Action.defaultDesc = "CommandGraveAssignColonistDesc".Translate();
				command_Action.action = delegate
				{
					Find.WindowStack.Add(new Dialog_AssignBuildingOwner(CompAssignableToPawn));
				};
				command_Action.hotKey = KeyBindingDefOf.Misc3;
				yield return command_Action;
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (HasCorpse)
			{
				if (base.Tile != -1)
				{
					string value = GenDate.DateFullStringAt(GenDate.TickGameToAbs(Corpse.timeOfDeath), Find.WorldGrid.LongLatOf(base.Tile));
					stringBuilder.AppendLine();
					stringBuilder.Append("DiedOn".Translate(value));
				}
			}
			else if (AssignedPawn != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("AssignedColonist".Translate());
				stringBuilder.Append(": ");
				stringBuilder.Append(AssignedPawn.LabelCap);
			}
			return stringBuilder.ToString();
		}
	}
}
