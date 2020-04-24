using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class Building_Bed : Building
	{
		private bool forPrisonersInt;

		private bool medicalInt;

		private bool alreadySetDefaultMed;

		private static int lastPrisonerSetChangeFrame = -1;

		private static readonly Color SheetColorNormal = new Color(161f / 255f, 71f / 85f, 0.7058824f);

		private static readonly Color SheetColorRoyal = new Color(57f / 85f, 233f / 255f, 38f / 51f);

		public static readonly Color SheetColorForPrisoner = new Color(1f, 61f / 85f, 11f / 85f);

		private static readonly Color SheetColorMedical = new Color(33f / 85f, 53f / 85f, 226f / 255f);

		private static readonly Color SheetColorMedicalForPrisoner = new Color(167f / 255f, 32f / 85f, 13f / 85f);

		public List<Pawn> OwnersForReading => CompAssignableToPawn.AssignedPawnsForReading;

		public CompAssignableToPawn CompAssignableToPawn => GetComp<CompAssignableToPawn>();

		public bool ForPrisoners
		{
			get
			{
				return forPrisonersInt;
			}
			set
			{
				if (value != forPrisonersInt && def.building.bed_humanlike)
				{
					if (Current.ProgramState != ProgramState.Playing && Scribe.mode != 0)
					{
						Log.Error("Tried to set ForPrisoners while game mode was " + Current.ProgramState);
						return;
					}
					RemoveAllOwners();
					forPrisonersInt = value;
					Notify_ColorChanged();
					NotifyRoomBedTypeChanged();
				}
			}
		}

		public bool Medical
		{
			get
			{
				return medicalInt;
			}
			set
			{
				if (value != medicalInt && def.building.bed_humanlike)
				{
					RemoveAllOwners();
					medicalInt = value;
					Notify_ColorChanged();
					if (base.Spawned)
					{
						base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
						NotifyRoomBedTypeChanged();
					}
					FacilityChanged();
				}
			}
		}

		public bool AnyUnownedSleepingSlot
		{
			get
			{
				if (Medical)
				{
					Log.Warning("Tried to check for unowned sleeping slot on medical bed " + this);
					return false;
				}
				return CompAssignableToPawn.HasFreeSlot;
			}
		}

		public bool AnyUnoccupiedSleepingSlot
		{
			get
			{
				for (int i = 0; i < SleepingSlotsCount; i++)
				{
					if (GetCurOccupant(i) == null)
					{
						return true;
					}
				}
				return false;
			}
		}

		public IEnumerable<Pawn> CurOccupants
		{
			get
			{
				for (int i = 0; i < SleepingSlotsCount; i++)
				{
					Pawn curOccupant = GetCurOccupant(i);
					if (curOccupant != null)
					{
						yield return curOccupant;
					}
				}
			}
		}

		public override Color DrawColor
		{
			get
			{
				if (def.MadeFromStuff)
				{
					return base.DrawColor;
				}
				return DrawColorTwo;
			}
		}

		public override Color DrawColorTwo
		{
			get
			{
				if (!def.building.bed_humanlike)
				{
					return base.DrawColorTwo;
				}
				bool forPrisoners = ForPrisoners;
				bool medical = Medical;
				if (forPrisoners && medical)
				{
					return SheetColorMedicalForPrisoner;
				}
				if (forPrisoners)
				{
					return SheetColorForPrisoner;
				}
				if (medical)
				{
					return SheetColorMedical;
				}
				if (def == ThingDefOf.RoyalBed)
				{
					return SheetColorRoyal;
				}
				return SheetColorNormal;
			}
		}

		public int SleepingSlotsCount => BedUtility.GetSleepingSlotsCount(def.size);

		private bool PlayerCanSeeOwners => CompAssignableToPawn.PlayerCanSeeAssignments;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			Region validRegionAt_NoRebuild = map.regionGrid.GetValidRegionAt_NoRebuild(base.Position);
			if (validRegionAt_NoRebuild != null && validRegionAt_NoRebuild.Room.isPrisonCell)
			{
				ForPrisoners = true;
			}
			if (!alreadySetDefaultMed)
			{
				alreadySetDefaultMed = true;
				if (def.building.bed_defaultMedical)
				{
					Medical = true;
				}
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			RemoveAllOwners();
			ForPrisoners = false;
			Medical = false;
			alreadySetDefaultMed = false;
			Room room = this.GetRoom();
			base.DeSpawn(mode);
			room?.Notify_RoomShapeOrContainedBedsChanged();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref forPrisonersInt, "forPrisoners", defaultValue: false);
			Scribe_Values.Look(ref medicalInt, "medical", defaultValue: false);
			Scribe_Values.Look(ref alreadySetDefaultMed, "alreadySetDefaultMed", defaultValue: false);
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			Room room = this.GetRoom();
			if (room != null && RoomCanBePrisonCell(room))
			{
				room.DrawFieldEdges();
			}
		}

		public static bool RoomCanBePrisonCell(Room r)
		{
			if (!r.TouchesMapEdge && !r.IsHuge)
			{
				return r.RegionType == RegionType.Normal;
			}
			return false;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (def.building.bed_humanlike && base.Faction == Faction.OfPlayer)
			{
				Command_Toggle command_Toggle = new Command_Toggle();
				command_Toggle.defaultLabel = "CommandBedSetForPrisonersLabel".Translate();
				command_Toggle.defaultDesc = "CommandBedSetForPrisonersDesc".Translate();
				command_Toggle.icon = ContentFinder<Texture2D>.Get("UI/Commands/ForPrisoners");
				command_Toggle.isActive = (() => ForPrisoners);
				command_Toggle.toggleAction = delegate
				{
					ToggleForPrisonersByInterface();
				};
				if (!RoomCanBePrisonCell(this.GetRoom()) && !ForPrisoners)
				{
					command_Toggle.Disable("CommandBedSetForPrisonersFailOutdoors".Translate());
				}
				command_Toggle.hotKey = KeyBindingDefOf.Misc3;
				command_Toggle.turnOffSound = null;
				command_Toggle.turnOnSound = null;
				yield return command_Toggle;
				Command_Toggle command_Toggle2 = new Command_Toggle();
				command_Toggle2.defaultLabel = "CommandBedSetAsMedicalLabel".Translate();
				command_Toggle2.defaultDesc = "CommandBedSetAsMedicalDesc".Translate();
				command_Toggle2.icon = ContentFinder<Texture2D>.Get("UI/Commands/AsMedical");
				command_Toggle2.isActive = (() => Medical);
				command_Toggle2.toggleAction = delegate
				{
					Medical = !Medical;
				};
				command_Toggle2.hotKey = KeyBindingDefOf.Misc2;
				yield return command_Toggle2;
				if (!ForPrisoners && !Medical)
				{
					Command_Action command_Action = new Command_Action();
					command_Action.defaultLabel = "CommandThingSetOwnerLabel".Translate();
					command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner");
					command_Action.defaultDesc = "CommandBedSetOwnerDesc".Translate();
					command_Action.action = delegate
					{
						Find.WindowStack.Add(new Dialog_AssignBuildingOwner(CompAssignableToPawn));
					};
					command_Action.hotKey = KeyBindingDefOf.Misc3;
					yield return command_Action;
				}
			}
		}

		private void ToggleForPrisonersByInterface()
		{
			if (lastPrisonerSetChangeFrame == Time.frameCount)
			{
				return;
			}
			lastPrisonerSetChangeFrame = Time.frameCount;
			bool newForPrisoners = !ForPrisoners;
			(newForPrisoners ? SoundDefOf.Checkbox_TurnedOn : SoundDefOf.Checkbox_TurnedOff).PlayOneShotOnCamera();
			List<Building_Bed> bedsToAffect = new List<Building_Bed>();
			foreach (Building_Bed item in Find.Selector.SelectedObjects.OfType<Building_Bed>())
			{
				if (item.ForPrisoners != newForPrisoners)
				{
					Room room = item.GetRoom();
					if (room == null || !RoomCanBePrisonCell(room))
					{
						if (!bedsToAffect.Contains(item))
						{
							bedsToAffect.Add(item);
						}
					}
					else
					{
						foreach (Building_Bed containedBed in room.ContainedBeds)
						{
							if (!bedsToAffect.Contains(containedBed))
							{
								bedsToAffect.Add(containedBed);
							}
						}
					}
				}
			}
			Action action = delegate
			{
				List<Room> list = new List<Room>();
				foreach (Building_Bed item2 in bedsToAffect)
				{
					Room room2 = item2.GetRoom();
					item2.ForPrisoners = (newForPrisoners && !room2.TouchesMapEdge);
					for (int j = 0; j < SleepingSlotsCount; j++)
					{
						GetCurOccupant(j)?.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
					if (!list.Contains(room2) && !room2.TouchesMapEdge)
					{
						list.Add(room2);
					}
				}
				foreach (Room item3 in list)
				{
					item3.Notify_RoomShapeOrContainedBedsChanged();
				}
			};
			if (bedsToAffect.Where((Building_Bed b) => b.OwnersForReading.Any() && b != this).Count() == 0)
			{
				action();
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (newForPrisoners)
			{
				stringBuilder.Append("TurningOnPrisonerBedWarning".Translate());
			}
			else
			{
				stringBuilder.Append("TurningOffPrisonerBedWarning".Translate());
			}
			stringBuilder.AppendLine();
			foreach (Building_Bed item4 in bedsToAffect)
			{
				if ((newForPrisoners && !item4.ForPrisoners) || (!newForPrisoners && item4.ForPrisoners))
				{
					for (int i = 0; i < item4.OwnersForReading.Count; i++)
					{
						stringBuilder.AppendLine();
						stringBuilder.Append(item4.OwnersForReading[i].LabelShort);
					}
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append("AreYouSure".Translate());
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(stringBuilder.ToString(), action));
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (def.building.bed_humanlike)
			{
				if (ForPrisoners)
				{
					stringBuilder.AppendInNewLine("ForPrisonerUse".Translate());
				}
				else if (PlayerCanSeeOwners)
				{
					stringBuilder.AppendInNewLine("ForColonistUse".Translate());
				}
				if (Medical)
				{
					stringBuilder.AppendInNewLine("MedicalBed".Translate());
					if (base.Spawned)
					{
						stringBuilder.AppendInNewLine("RoomInfectionChanceFactor".Translate() + ": " + this.GetRoom().GetStat(RoomStatDefOf.InfectionChanceFactor).ToStringPercent());
					}
				}
				else if (PlayerCanSeeOwners)
				{
					if (OwnersForReading.Count == 0)
					{
						stringBuilder.AppendInNewLine("Owner".Translate() + ": " + "Nobody".Translate());
					}
					else if (OwnersForReading.Count == 1)
					{
						stringBuilder.AppendInNewLine("Owner".Translate() + ": " + OwnersForReading[0].Label);
					}
					else
					{
						stringBuilder.AppendInNewLine("Owners".Translate() + ": ");
						bool flag = false;
						for (int i = 0; i < OwnersForReading.Count; i++)
						{
							if (flag)
							{
								stringBuilder.Append(", ");
							}
							flag = true;
							stringBuilder.Append(OwnersForReading[i].LabelShort);
						}
					}
				}
			}
			return stringBuilder.ToString();
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			if (myPawn.RaceProps.Humanlike && !ForPrisoners && Medical && !myPawn.Drafted && base.Faction == Faction.OfPlayer && RestUtility.CanUseBedEver(myPawn, def))
			{
				if (!HealthAIUtility.ShouldSeekMedicalRest(myPawn) && !HealthAIUtility.ShouldSeekMedicalRestUrgent(myPawn))
				{
					yield return new FloatMenuOption("UseMedicalBed".Translate() + " (" + "NotInjured".Translate() + ")", null);
					yield break;
				}
				Action action = delegate
				{
					if (!ForPrisoners && Medical && myPawn.CanReserveAndReach(this, PathEndMode.ClosestTouch, Danger.Deadly, SleepingSlotsCount, -1, null, ignoreOtherReservations: true))
					{
						if (myPawn.CurJobDef == JobDefOf.LayDown && myPawn.CurJob.GetTarget(TargetIndex.A).Thing == this)
						{
							myPawn.CurJob.restUntilHealed = true;
						}
						else
						{
							Job job = JobMaker.MakeJob(JobDefOf.LayDown, this);
							job.restUntilHealed = true;
							myPawn.jobs.TryTakeOrderedJob(job);
						}
						myPawn.mindState.ResetLastDisturbanceTick();
					}
				};
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("UseMedicalBed".Translate(), action), myPawn, this, (AnyUnoccupiedSleepingSlot ? "ReservedBy" : "SomeoneElseSleeping").CapitalizeFirst());
			}
		}

		public override void DrawGUIOverlay()
		{
			if (Medical || Find.CameraDriver.CurrentZoom != 0 || !PlayerCanSeeOwners)
			{
				return;
			}
			Color defaultThingLabelColor = GenMapUI.DefaultThingLabelColor;
			if (!OwnersForReading.Any())
			{
				GenMapUI.DrawThingLabel(this, "Unowned".Translate(), defaultThingLabelColor);
				return;
			}
			if (OwnersForReading.Count == 1)
			{
				if (!OwnersForReading[0].InBed() || OwnersForReading[0].CurrentBed() != this)
				{
					GenMapUI.DrawThingLabel(this, OwnersForReading[0].LabelShort, defaultThingLabelColor);
				}
				return;
			}
			for (int i = 0; i < OwnersForReading.Count; i++)
			{
				if (!OwnersForReading[i].InBed() || OwnersForReading[i].CurrentBed() != this || !(OwnersForReading[i].Position == GetSleepingSlotPos(i)))
				{
					GenMapUI.DrawThingLabel(GetMultiOwnersLabelScreenPosFor(i), OwnersForReading[i].LabelShort, defaultThingLabelColor);
				}
			}
		}

		public Pawn GetCurOccupant(int slotIndex)
		{
			if (!base.Spawned)
			{
				return null;
			}
			IntVec3 sleepingSlotPos = GetSleepingSlotPos(slotIndex);
			List<Thing> list = base.Map.thingGrid.ThingsListAt(sleepingSlotPos);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i] as Pawn;
				if (pawn != null && pawn.CurJob != null && pawn.GetPosture() == PawnPosture.LayingInBed)
				{
					return pawn;
				}
			}
			return null;
		}

		public int GetCurOccupantSlotIndex(Pawn curOccupant)
		{
			for (int i = 0; i < SleepingSlotsCount; i++)
			{
				if (GetCurOccupant(i) == curOccupant)
				{
					return i;
				}
			}
			Log.Error("Could not find pawn " + curOccupant + " on any of sleeping slots.");
			return 0;
		}

		public Pawn GetCurOccupantAt(IntVec3 pos)
		{
			for (int i = 0; i < SleepingSlotsCount; i++)
			{
				if (GetSleepingSlotPos(i) == pos)
				{
					return GetCurOccupant(i);
				}
			}
			return null;
		}

		public IntVec3 GetSleepingSlotPos(int index)
		{
			return BedUtility.GetSleepingSlotPos(index, base.Position, base.Rotation, def.size);
		}

		private void RemoveAllOwners()
		{
			for (int num = OwnersForReading.Count - 1; num >= 0; num--)
			{
				OwnersForReading[num].ownership.UnclaimBed();
			}
		}

		private void NotifyRoomBedTypeChanged()
		{
			this.GetRoom()?.Notify_BedTypeChanged();
		}

		private void FacilityChanged()
		{
			CompFacility compFacility = this.TryGetComp<CompFacility>();
			CompAffectedByFacilities compAffectedByFacilities = this.TryGetComp<CompAffectedByFacilities>();
			compFacility?.Notify_ThingChanged();
			compAffectedByFacilities?.Notify_ThingChanged();
		}

		private Vector3 GetMultiOwnersLabelScreenPosFor(int slotIndex)
		{
			IntVec3 sleepingSlotPos = GetSleepingSlotPos(slotIndex);
			Vector3 drawPos = DrawPos;
			if (base.Rotation.IsHorizontal)
			{
				drawPos.z = (float)sleepingSlotPos.z + 0.6f;
			}
			else
			{
				drawPos.x = (float)sleepingSlotPos.x + 0.5f;
				drawPos.z += -0.4f;
			}
			Vector2 v = drawPos.MapToUIPosition();
			if (!base.Rotation.IsHorizontal && SleepingSlotsCount == 2)
			{
				v = AdjustOwnerLabelPosToAvoidOverlapping(v, slotIndex);
			}
			return v;
		}

		private Vector3 AdjustOwnerLabelPosToAvoidOverlapping(Vector3 screenPos, int slotIndex)
		{
			Text.Font = GameFont.Tiny;
			float num = Text.CalcSize(OwnersForReading[slotIndex].LabelShort).x + 1f;
			Vector2 vector = DrawPos.MapToUIPosition();
			float num2 = Mathf.Abs(screenPos.x - vector.x);
			IntVec3 sleepingSlotPos = GetSleepingSlotPos(slotIndex);
			if (num > num2 * 2f)
			{
				float num3 = 0f;
				num3 = ((slotIndex != 0) ? ((float)GetSleepingSlotPos(0).x) : ((float)GetSleepingSlotPos(1).x));
				if ((float)sleepingSlotPos.x < num3)
				{
					screenPos.x -= (num - num2 * 2f) / 2f;
				}
				else
				{
					screenPos.x += (num - num2 * 2f) / 2f;
				}
			}
			return screenPos;
		}
	}
}
