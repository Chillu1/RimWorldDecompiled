using System.Linq;
using Verse;

namespace RimWorld
{
	public class Pawn_Ownership : IExposable
	{
		private Pawn pawn;

		private Building_Bed intOwnedBed;

		public Building_Bed OwnedBed
		{
			get
			{
				return intOwnedBed;
			}
			private set
			{
				if (intOwnedBed != value)
				{
					intOwnedBed = value;
					ThoughtUtility.RemovePositiveBedroomThoughts(pawn);
				}
			}
		}

		public Building_Grave AssignedGrave
		{
			get;
			private set;
		}

		public Building_Throne AssignedThrone
		{
			get;
			private set;
		}

		public Room OwnedRoom
		{
			get
			{
				if (OwnedBed == null)
				{
					return null;
				}
				Room room = OwnedBed.GetRoom();
				if (room == null)
				{
					return null;
				}
				if (room.Owners.Contains(pawn))
				{
					return room;
				}
				return null;
			}
		}

		public Pawn_Ownership(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Building_Grave refee = AssignedGrave;
			Building_Throne refee2 = AssignedThrone;
			Scribe_References.Look(ref intOwnedBed, "ownedBed");
			Scribe_References.Look(ref refee, "assignedGrave");
			Scribe_References.Look(ref refee2, "assignedThrone");
			AssignedGrave = refee;
			AssignedThrone = refee2;
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}
			if (intOwnedBed != null)
			{
				CompAssignableToPawn compAssignableToPawn = intOwnedBed.TryGetComp<CompAssignableToPawn>();
				if (compAssignableToPawn != null && !compAssignableToPawn.AssignedPawns.Contains(pawn))
				{
					Building_Bed newBed = intOwnedBed;
					UnclaimBed();
					ClaimBedIfNonMedical(newBed);
				}
			}
			if (AssignedGrave != null)
			{
				CompAssignableToPawn compAssignableToPawn2 = AssignedGrave.TryGetComp<CompAssignableToPawn>();
				if (compAssignableToPawn2 != null && !compAssignableToPawn2.AssignedPawns.Contains(pawn))
				{
					Building_Grave assignedGrave = AssignedGrave;
					UnclaimGrave();
					ClaimGrave(assignedGrave);
				}
			}
			if (AssignedThrone != null)
			{
				CompAssignableToPawn compAssignableToPawn3 = AssignedThrone.TryGetComp<CompAssignableToPawn>();
				if (compAssignableToPawn3 != null && !compAssignableToPawn3.AssignedPawns.Contains(pawn))
				{
					Building_Throne assignedThrone = AssignedThrone;
					UnclaimThrone();
					ClaimThrone(assignedThrone);
				}
			}
		}

		public bool ClaimBedIfNonMedical(Building_Bed newBed)
		{
			if (newBed.OwnersForReading.Contains(pawn) || newBed.Medical)
			{
				return false;
			}
			UnclaimBed();
			if (newBed.OwnersForReading.Count == newBed.SleepingSlotsCount)
			{
				newBed.OwnersForReading[newBed.OwnersForReading.Count - 1].ownership.UnclaimBed();
			}
			newBed.CompAssignableToPawn.ForceAddPawn(pawn);
			OwnedBed = newBed;
			if (newBed.Medical)
			{
				Log.Warning(pawn.LabelCap + " claimed medical bed.");
				UnclaimBed();
			}
			return true;
		}

		public bool UnclaimBed()
		{
			if (OwnedBed == null)
			{
				return false;
			}
			OwnedBed.CompAssignableToPawn.ForceRemovePawn(pawn);
			OwnedBed = null;
			return true;
		}

		public bool ClaimGrave(Building_Grave newGrave)
		{
			if (newGrave.AssignedPawn == pawn)
			{
				return false;
			}
			UnclaimGrave();
			if (newGrave.AssignedPawn != null)
			{
				newGrave.AssignedPawn.ownership.UnclaimGrave();
			}
			newGrave.CompAssignableToPawn.ForceAddPawn(pawn);
			newGrave.GetStoreSettings().Priority = StoragePriority.Critical;
			AssignedGrave = newGrave;
			return true;
		}

		public bool UnclaimGrave()
		{
			if (AssignedGrave == null)
			{
				return false;
			}
			AssignedGrave.CompAssignableToPawn.ForceRemovePawn(pawn);
			AssignedGrave.GetStoreSettings().Priority = StoragePriority.Important;
			AssignedGrave = null;
			return true;
		}

		public bool ClaimThrone(Building_Throne newThrone)
		{
			if (newThrone.AssignedPawn == pawn)
			{
				return false;
			}
			UnclaimThrone();
			if (newThrone.AssignedPawn != null)
			{
				newThrone.AssignedPawn.ownership.UnclaimThrone();
			}
			newThrone.CompAssignableToPawn.ForceAddPawn(pawn);
			AssignedThrone = newThrone;
			return true;
		}

		public bool UnclaimThrone()
		{
			if (AssignedThrone == null)
			{
				return false;
			}
			AssignedThrone.CompAssignableToPawn.ForceRemovePawn(pawn);
			AssignedThrone = null;
			return true;
		}

		public void UnclaimAll()
		{
			UnclaimBed();
			UnclaimGrave();
			UnclaimThrone();
		}

		public void Notify_ChangedGuestStatus()
		{
			if (OwnedBed != null && ((OwnedBed.ForPrisoners && !pawn.IsPrisoner) || (!OwnedBed.ForPrisoners && pawn.IsPrisoner)))
			{
				UnclaimBed();
			}
		}
	}
}
