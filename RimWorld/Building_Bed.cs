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
		private BedOwnerType forOwnerType;

		private bool medicalInt;

		private bool alreadySetDefaultMed;

		private static int lastBedOwnerSetChangeFrame = -1;

		private static List<IntVec3> tmpOrderedInteractionCells = new List<IntVec3>(8);

		private static readonly Color SheetColorNormal = new Color(0.6313726f, 71f / 85f, 0.7058824f);

		private static readonly Color SheetColorRoyal = new Color(57f / 85f, 0.9137255f, 38f / 51f);

		public static readonly Color SheetColorForPrisoner = new Color(1f, 61f / 85f, 11f / 85f);

		private static readonly Color SheetColorMedical = new Color(33f / 85f, 53f / 85f, 0.8862745f);

		private static readonly Color SheetColorMedicalForPrisoner = new Color(0.654902f, 32f / 85f, 13f / 85f);

		private static readonly Color SheetColorForSlave = new Color32(252, 244, 3, byte.MaxValue);

		private static readonly Color SheetColorMedicalForSlave = new Color32(153, 148, 0, byte.MaxValue);

		private static readonly BedInteractionCellSearchPattern defaultBedInteractionCellsOrder = new BedInteractionCellSearchPattern();

		public List<Pawn> OwnersForReading => CompAssignableToPawn.AssignedPawnsForReading;

		public CompAssignableToPawn CompAssignableToPawn => GetComp<CompAssignableToPawn>();

		public bool ForPrisoners
		{
			get
			{
				return forOwnerType == BedOwnerType.Prisoner;
			}
			set
			{
				if (value == ForPrisoners || !def.building.bed_humanlike || ForHumanBabies)
				{
					return;
				}
				if (Current.ProgramState != ProgramState.Playing && Scribe.mode != LoadSaveMode.Inactive)
				{
					Log.Error("Tried to set ForPrisoners while game mode was " + Current.ProgramState);
					return;
				}
				RemoveAllOwners();
				if (value)
				{
					forOwnerType = BedOwnerType.Prisoner;
				}
				else
				{
					forOwnerType = BedOwnerType.Colonist;
					Log.Error("Bed ForPrisoners=false, but should it be for for colonists or slaves?  Set ForOwnerType instead.");
				}
				Notify_ColorChanged();
				NotifyRoomBedTypeChanged();
			}
		}

		public bool ForSlaves => ForOwnerType == BedOwnerType.Slave;

		public bool ForColonists => ForOwnerType == BedOwnerType.Colonist;

		public bool ForHumanBabies
		{
			get
			{
				if (def.building.bed_humanlike)
				{
					return def.building.bed_maxBodySize < LifeStageDefOf.HumanlikeChild.bodySizeFactor;
				}
				return false;
			}
		}

		public BedOwnerType ForOwnerType
		{
			get
			{
				return forOwnerType;
			}
			set
			{
				if (value != forOwnerType && def.building.bed_humanlike && !ForHumanBabies && (value != BedOwnerType.Slave || ModLister.CheckIdeology("Slavery")))
				{
					RemoveAllOwners();
					forOwnerType = value;
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
				if (value != medicalInt && (!value || def.building.bed_canBeMedical))
				{
					RemoveAllOwners();
					medicalInt = value;
					Notify_ColorChanged();
					if (base.Spawned)
					{
						base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things);
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

		public int TotalSleepingSlots
		{
			get
			{
				if (Medical)
				{
					Log.Warning("Tried to check for total sleeping slots on medical bed " + this);
					return 0;
				}
				return CompAssignableToPawn.TotalSlots;
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

		public bool AnyOccupants
		{
			get
			{
				for (int i = 0; i < SleepingSlotsCount; i++)
				{
					if (GetCurOccupant(i) != null)
					{
						return true;
					}
				}
				return false;
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
				if (def.building != null && !def.building.bed_UseSheetColor)
				{
					return base.DrawColorTwo;
				}
				bool medical = Medical;
				switch (forOwnerType)
				{
				case BedOwnerType.Prisoner:
					if (!medical)
					{
						return SheetColorForPrisoner;
					}
					return SheetColorMedicalForPrisoner;
				case BedOwnerType.Slave:
					if (!medical)
					{
						return SheetColorForSlave;
					}
					return SheetColorMedicalForSlave;
				default:
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
		}

		public int SleepingSlotsCount => BedUtility.GetSleepingSlotsCount(def.size);

		private bool PlayerCanSeeOwners => CompAssignableToPawn.PlayerCanSeeAssignments;

		public override IntVec3 InteractionCell => FindPreferredInteractionCell(base.Position) ?? base.InteractionCell;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!alreadySetDefaultMed)
			{
				alreadySetDefaultMed = true;
				if (def.building.bed_defaultMedical)
				{
					Medical = true;
				}
			}
			if (!respawningAfterLoad)
			{
				Region validRegionAt_NoRebuild = map.regionGrid.GetValidRegionAt_NoRebuild(base.Position);
				if (validRegionAt_NoRebuild != null && validRegionAt_NoRebuild.Room.IsPrisonCell)
				{
					ForPrisoners = true;
				}
				District district = this.GetDistrict();
				if (district != null)
				{
					district.Notify_RoomShapeOrContainedBedsChanged();
					district.Room.Notify_RoomShapeChanged();
				}
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			if (mode != DestroyMode.WillReplace)
			{
				if (mode != DestroyMode.Vanish)
				{
					RemoveAllOwners(mode == DestroyMode.KillFinalize);
				}
				else if (InstallBlueprintUtility.ExistingBlueprintFor(this) == null)
				{
					foreach (Pawn item in OwnersForReading)
					{
						Messages.Message("MessageBedLostAssignment".Translate(def, item), new LookTargets(this, item), MessageTypeDefOf.CautionInput, historical: false);
					}
				}
				ForOwnerType = BedOwnerType.Colonist;
				Medical = false;
				alreadySetDefaultMed = false;
			}
			District district = this.GetDistrict();
			base.DeSpawn(mode);
			if (district != null)
			{
				district.Notify_RoomShapeOrContainedBedsChanged();
				district.Room.Notify_RoomShapeChanged();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref medicalInt, "medical", defaultValue: false);
			Scribe_Values.Look(ref alreadySetDefaultMed, "alreadySetDefaultMed", defaultValue: false);
			Scribe_Values.Look(ref forOwnerType, "forOwnerType", BedOwnerType.Colonist);
			BackCompatibility.PostExposeData(this);
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
			if (r.ProperRoom)
			{
				return !r.IsHuge;
			}
			return false;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (base.Faction != Faction.OfPlayer)
			{
				yield break;
			}
			if (def.building.bed_humanlike && !ForHumanBabies)
			{
				if (ModsConfig.IdeologyActive)
				{
					yield return new Command_SetBedOwnerType(this);
				}
				else
				{
					Command_Toggle command_Toggle = new Command_Toggle();
					command_Toggle.defaultLabel = "CommandBedSetForPrisonersLabel".Translate();
					command_Toggle.defaultDesc = "CommandBedSetForPrisonersDesc".Translate();
					command_Toggle.icon = ContentFinder<Texture2D>.Get("UI/Commands/ForPrisoners");
					command_Toggle.isActive = () => ForPrisoners;
					command_Toggle.toggleAction = delegate
					{
						SetBedOwnerTypeByInterface((!ForPrisoners) ? BedOwnerType.Prisoner : BedOwnerType.Colonist);
					};
					if (!RoomCanBePrisonCell(this.GetRoom()) && !ForPrisoners)
					{
						command_Toggle.Disable("CommandBedSetForPrisonersFailOutdoors".Translate());
					}
					command_Toggle.hotKey = KeyBindingDefOf.Misc3;
					command_Toggle.turnOffSound = null;
					command_Toggle.turnOnSound = null;
					yield return command_Toggle;
				}
			}
			if (def.building.bed_canBeMedical)
			{
				Command_Toggle command_Toggle2 = new Command_Toggle();
				command_Toggle2.defaultLabel = "CommandBedSetAsMedicalLabel".Translate();
				command_Toggle2.defaultDesc = "CommandBedSetAsMedicalDesc".Translate();
				command_Toggle2.icon = ContentFinder<Texture2D>.Get("UI/Commands/AsMedical");
				command_Toggle2.isActive = () => Medical;
				command_Toggle2.toggleAction = delegate
				{
					Medical = !Medical;
				};
				command_Toggle2.hotKey = KeyBindingDefOf.Misc2;
				yield return command_Toggle2;
			}
		}

		public void SetBedOwnerTypeByInterface(BedOwnerType ownerType)
		{
			if (lastBedOwnerSetChangeFrame == Time.frameCount)
			{
				return;
			}
			lastBedOwnerSetChangeFrame = Time.frameCount;
			((ForOwnerType != ownerType) ? SoundDefOf.Checkbox_TurnedOn : SoundDefOf.Checkbox_TurnedOff).PlayOneShotOnCamera();
			List<Building_Bed> bedsToAffect = new List<Building_Bed>();
			foreach (Building_Bed item in Find.Selector.SelectedObjects.OfType<Building_Bed>())
			{
				if (item.ForOwnerType == ownerType)
				{
					continue;
				}
				Room room = item.GetRoom();
				if (room == null && ownerType != BedOwnerType.Prisoner)
				{
					if (!bedsToAffect.Contains(item))
					{
						bedsToAffect.Add(item);
					}
					continue;
				}
				foreach (Building_Bed containedBed in room.ContainedBeds)
				{
					if (containedBed.ForOwnerType != ownerType)
					{
						if (containedBed.ForOwnerType == BedOwnerType.Prisoner && !bedsToAffect.Contains(containedBed))
						{
							bedsToAffect.Add(containedBed);
						}
						else if (ownerType == BedOwnerType.Prisoner && RoomCanBePrisonCell(room) && !bedsToAffect.Contains(containedBed))
						{
							bedsToAffect.Add(containedBed);
						}
						else if (containedBed == item && !bedsToAffect.Contains(containedBed))
						{
							bedsToAffect.Add(containedBed);
						}
					}
				}
			}
			Action action = delegate
			{
				List<District> list = new List<District>();
				List<Room> list2 = new List<Room>();
				foreach (Building_Bed item2 in bedsToAffect)
				{
					District district = item2.GetDistrict();
					Room room2 = district.Room;
					if (ownerType == BedOwnerType.Prisoner && room2.TouchesMapEdge)
					{
						item2.ForOwnerType = BedOwnerType.Colonist;
					}
					else
					{
						item2.ForOwnerType = ownerType;
					}
					if (!room2.TouchesMapEdge)
					{
						if (!list2.Contains(room2))
						{
							list2.Add(room2);
						}
						if (!list.Contains(district))
						{
							list.Add(district);
						}
					}
				}
				foreach (District item3 in list)
				{
					item3.Notify_RoomShapeOrContainedBedsChanged();
				}
				foreach (Room item4 in list2)
				{
					item4.Notify_RoomShapeChanged();
				}
			};
			if (bedsToAffect.Where((Building_Bed b) => b.OwnersForReading.Any((Pawn owner) => owner.RaceProps.Humanlike) && b != this).Count() == 0)
			{
				action();
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (!ModsConfig.IdeologyActive)
			{
				if (ownerType == BedOwnerType.Prisoner)
				{
					stringBuilder.Append("TurningOnPrisonerBedWarning".Translate());
				}
				else
				{
					stringBuilder.Append("TurningOffPrisonerBedWarning".Translate());
				}
			}
			else
			{
				stringBuilder.Append("ChangingOwnerTypeBedWarning".Translate());
			}
			stringBuilder.AppendLine();
			foreach (Building_Bed item5 in bedsToAffect)
			{
				if (ownerType != item5.ForOwnerType)
				{
					for (int num = 0; num < item5.OwnersForReading.Count; num++)
					{
						stringBuilder.AppendLine();
						stringBuilder.Append(item5.OwnersForReading[num].LabelShort);
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
			if (def.building.bed_humanlike && def.building.bed_DisplayOwnerType && base.Faction == Faction.OfPlayer)
			{
				switch (ForOwnerType)
				{
				case BedOwnerType.Prisoner:
					stringBuilder.AppendInNewLine("ForPrisonerUse".Translate());
					break;
				case BedOwnerType.Slave:
					stringBuilder.AppendInNewLine("ForSlaveUse".Translate());
					break;
				case BedOwnerType.Colonist:
					stringBuilder.AppendInNewLine("ForColonistUse".Translate());
					break;
				default:
					Log.Error($"Unknown bed owner type: {ForOwnerType}");
					break;
				}
			}
			if (Medical)
			{
				stringBuilder.AppendInNewLine("MedicalBed".Translate());
				if (base.Spawned)
				{
					stringBuilder.AppendInNewLine("RoomInfectionChanceFactor".Translate() + ": " + this.GetRoom().GetStat(RoomStatDefOf.InfectionChanceFactor).ToStringPercent());
				}
			}
			else if (PlayerCanSeeOwners && def.building.bed_DisplayOwnersInInspectString)
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
				if (OwnersForReading.Count == 1 && ChildcareUtility.CanSuckle(OwnersForReading[0], out var _))
				{
					Pawn p = OwnersForReading[0];
					float ambientTemperature = base.AmbientTemperature;
					if (!p.SafeTemperatureRange().IncludesEpsilon(ambientTemperature))
					{
						stringBuilder.AppendInNewLine("BedUnsafeTemperature".Translate().Colorize(ColoredText.WarningColor));
					}
					else if (!p.ComfortableTemperatureRange().IncludesEpsilon(ambientTemperature))
					{
						stringBuilder.AppendInNewLine("BedUncomfortableTemperature".Translate());
					}
				}
			}
			return stringBuilder.ToString();
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			FloatMenuOption bedRestFloatMenuOption = GetBedRestFloatMenuOption(myPawn);
			if (bedRestFloatMenuOption != null)
			{
				yield return bedRestFloatMenuOption;
			}
		}

		public FloatMenuOption GetBedRestFloatMenuOption(Pawn myPawn)
		{
			if (myPawn.RaceProps.Humanlike && !ForPrisoners && Medical && !myPawn.Drafted && base.Faction == Faction.OfPlayer && RestUtility.CanUseBedEver(myPawn, def))
			{
				if (!HealthAIUtility.ShouldSeekMedicalRest(myPawn))
				{
					if (myPawn.health.surgeryBills.AnyShouldDoNow && !WorkGiver_PatientGoToBedTreatment.AnyAvailableDoctorFor(myPawn))
					{
						return new FloatMenuOption("UseMedicalBed".Translate() + " (" + "NoDoctor".Translate() + ")", null);
					}
					return new FloatMenuOption("UseMedicalBed".Translate() + " (" + "NotInjured".Translate() + ")", null);
				}
				if (myPawn.IsSlaveOfColony && !ForSlaves)
				{
					return new FloatMenuOption("UseMedicalBed".Translate() + " (" + "NotForSlaves".Translate() + ")", null);
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
							myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
						}
						myPawn.mindState.ResetLastDisturbanceTick();
					}
				};
				return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("UseMedicalBed".Translate(), action), myPawn, this, (AnyUnoccupiedSleepingSlot ? "ReservedBy" : "SomeoneElseSleeping").CapitalizeFirst());
			}
			return null;
		}

		public override void DrawGUIOverlay()
		{
			if (Medical || Find.CameraDriver.CurrentZoom != CameraZoomRange.Closest || !PlayerCanSeeOwners)
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
				Pawn pawn = OwnersForReading[0];
				if ((!pawn.InBed() || pawn.CurrentBed() != this) && (!pawn.RaceProps.Animal || Prefs.AnimalNameMode.ShouldDisplayAnimalName(pawn)))
				{
					GenMapUI.DrawThingLabel(this, pawn.LabelShort, defaultThingLabelColor);
				}
				return;
			}
			for (int i = 0; i < OwnersForReading.Count; i++)
			{
				Pawn pawn2 = OwnersForReading[i];
				if (!pawn2.InBed() || OwnersForReading[i].CurrentBed() != this || !(pawn2.Position == GetSleepingSlotPos(i)))
				{
					if (pawn2.RaceProps.Animal && !Prefs.AnimalNameMode.ShouldDisplayAnimalName(pawn2))
					{
						break;
					}
					GenMapUI.DrawThingLabel(GetMultiOwnersLabelScreenPosFor(i), pawn2.LabelShort, defaultThingLabelColor);
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
				if (list[i] is Pawn { CurJob: not null } pawn && pawn.GetPosture().InBed())
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
			Log.Error("Could not find pawn " + curOccupant?.ToString() + " on any of sleeping slots.");
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

		public IntVec3 GetFootSlotPos(int index)
		{
			return BedUtility.GetFeetSlotPos(index, base.Position, base.Rotation, def.size);
		}

		public bool IsOwner(Pawn p)
		{
			int? assignedSleepingSlot;
			return IsOwner(p, out assignedSleepingSlot);
		}

		public bool IsOwner(Pawn p, out int? assignedSleepingSlot)
		{
			int num = GetComp<CompAssignableToPawn>().AssignedPawnsForReading.IndexOf(p);
			if (num >= 0)
			{
				assignedSleepingSlot = num;
				return true;
			}
			assignedSleepingSlot = null;
			return false;
		}

		private void RemoveAllOwners(bool destroyed = false)
		{
			for (int num = OwnersForReading.Count - 1; num >= 0; num--)
			{
				Pawn pawn = OwnersForReading[num];
				pawn.ownership.UnclaimBed();
				string key = "MessageBedLostAssignment";
				if (destroyed)
				{
					key = "MessageBedDestroyed";
				}
				Messages.Message(key.Translate(def, pawn), new LookTargets(this, pawn), MessageTypeDefOf.CautionInput, historical: false);
			}
		}

		private void NotifyRoomBedTypeChanged()
		{
			this.GetRoom()?.Notify_BedTypeChanged();
		}

		public void NotifyRoomAssignedPawnsChanged()
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
			Vector2 vector = drawPos.MapToUIPosition();
			if (!base.Rotation.IsHorizontal && SleepingSlotsCount == 2)
			{
				vector = AdjustOwnerLabelPosToAvoidOverlapping(vector, slotIndex);
			}
			return vector;
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

		private bool RemoveLeastDesirableInteractionCells(List<IntVec3> cells, Map map)
		{
			cells.RemoveAll(map, (Map innerMap, IntVec3 cell) => !cell.Standable(innerMap) || !TouchPathEndModeUtility.IsAdjacentOrInsideAndAllowedToTouch(cell, new LocalTargetInfo(this), map.pathing.Normal));
			if (cells.RemoveAll_IfNotAll(map, delegate(Map innerMap, IntVec3 cell)
			{
				Building building = map.edificeGrid[cell];
				return building == null || building.def?.IsBed != true;
			}))
			{
				return cells.RemoveAll_IfNotAll(map, (Map innerMap, IntVec3 cell) => cell.GetDoor(map) == null);
			}
			return false;
		}

		public IntVec3? FindPreferredInteractionCell(IntVec3 occupantLocation, CellSearchPattern customSearchPattern = null)
		{
			CellRect cellRect = this.OccupiedRect();
			if (!cellRect.Contains(occupantLocation))
			{
				Log.Error($"interiorLocation {occupantLocation} is not within the bounds of this bed {cellRect}.");
				return null;
			}
			tmpOrderedInteractionCells.Clear();
			(customSearchPattern ?? defaultBedInteractionCellsOrder).AddCellsToList(tmpOrderedInteractionCells, this, cellRect, occupantLocation, base.Rotation);
			RemoveLeastDesirableInteractionCells(tmpOrderedInteractionCells, base.Map);
			if (tmpOrderedInteractionCells.Count == 0)
			{
				return null;
			}
			return tmpOrderedInteractionCells[0];
		}
	}
}
