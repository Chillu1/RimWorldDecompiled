using System.Linq;
using Verse;

namespace RimWorld;

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

	public Building_Grave AssignedGrave { get; private set; }

	public Building_Throne AssignedThrone { get; private set; }

	public Building AssignedMeditationSpot { get; private set; }

	public Building_Bed AssignedDeathrestCasket { get; private set; }

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

	public Room Bedroom
	{
		get
		{
			if (OwnedBed == null)
			{
				return null;
			}
			return OwnedBed.GetRoom();
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
		Building refee3 = AssignedMeditationSpot;
		Building_Bed refee4 = AssignedDeathrestCasket;
		Scribe_References.Look(ref intOwnedBed, "ownedBed");
		Scribe_References.Look(ref refee3, "assignedMeditationSpot");
		Scribe_References.Look(ref refee, "assignedGrave");
		Scribe_References.Look(ref refee2, "assignedThrone");
		Scribe_References.Look(ref refee4, "assignedDeathrestCasket");
		AssignedGrave = refee;
		AssignedThrone = refee2;
		AssignedMeditationSpot = refee3;
		AssignedDeathrestCasket = refee4;
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
		if (newBed.IsOwner(pawn) || newBed.Medical)
		{
			return false;
		}
		if (ModsConfig.BiotechActive && newBed.def == ThingDefOf.DeathrestCasket)
		{
			UnclaimDeathrestCasket();
			newBed.CompAssignableToPawn.ForceAddPawn(pawn);
			AssignedDeathrestCasket = newBed;
			return true;
		}
		UnclaimBed();
		if (newBed.OwnersForReading.Count == newBed.SleepingSlotsCount)
		{
			newBed.OwnersForReading[newBed.OwnersForReading.Count - 1].ownership.UnclaimBed();
		}
		newBed.CompAssignableToPawn.ForceAddPawn(pawn);
		OwnedBed = newBed;
		if (pawn.IsFreeman && newBed.CompAssignableToPawn.IdeoligionForbids(pawn))
		{
			Log.Error("Assigned " + pawn.GetUniqueLoadID() + " to a bed against their or occupants' ideo.");
		}
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

	public bool ClaimMeditationSpot(Building newSpot)
	{
		if (newSpot.GetAssignedPawn() == pawn)
		{
			return false;
		}
		UnclaimMeditationSpot();
		if (newSpot.GetAssignedPawn() != null)
		{
			newSpot.GetAssignedPawn().ownership.UnclaimMeditationSpot();
		}
		newSpot.TryGetComp<CompAssignableToPawn>().ForceAddPawn(pawn);
		AssignedMeditationSpot = newSpot;
		return true;
	}

	public bool UnclaimMeditationSpot()
	{
		if (AssignedMeditationSpot == null)
		{
			return false;
		}
		AssignedMeditationSpot.TryGetComp<CompAssignableToPawn>().ForceRemovePawn(pawn);
		AssignedMeditationSpot = null;
		return true;
	}

	public bool ClaimDeathrestCasket(Building_Bed deathrestCasket)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (deathrestCasket.CompAssignableToPawn.AssignedPawns.Contains(pawn))
		{
			return false;
		}
		UnclaimDeathrestCasket();
		if (deathrestCasket.GetAssignedPawn() != null)
		{
			deathrestCasket.GetAssignedPawn().ownership.UnclaimDeathrestCasket();
		}
		deathrestCasket.CompAssignableToPawn.ForceAddPawn(pawn);
		AssignedDeathrestCasket = deathrestCasket;
		return true;
	}

	public bool UnclaimDeathrestCasket()
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (AssignedDeathrestCasket == null)
		{
			return false;
		}
		AssignedDeathrestCasket.CompAssignableToPawn.ForceRemovePawn(pawn);
		AssignedDeathrestCasket = null;
		return true;
	}

	public void UnclaimAll()
	{
		UnclaimBed();
		UnclaimGrave();
		UnclaimThrone();
		UnclaimDeathrestCasket();
	}

	public void Notify_ChangedGuestStatus()
	{
		if (OwnedBed != null && ((OwnedBed.ForPrisoners && !pawn.IsPrisoner && !PawnUtility.IsBeingArrested(pawn)) || (!OwnedBed.ForPrisoners && pawn.IsPrisoner) || (OwnedBed.ForColonists && pawn.HostFaction == null)))
		{
			UnclaimBed();
		}
		if (AssignedDeathrestCasket != null && ((AssignedDeathrestCasket.ForPrisoners && !pawn.IsPrisoner && !PawnUtility.IsBeingArrested(pawn)) || (!AssignedDeathrestCasket.ForPrisoners && pawn.IsPrisoner) || (AssignedDeathrestCasket.ForColonists && pawn.HostFaction == null)))
		{
			UnclaimDeathrestCasket();
		}
	}
}
