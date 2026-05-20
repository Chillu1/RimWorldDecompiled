using System.Collections.Generic;
using Verse.AI;

namespace Verse;

public class PhysicalInteractionReservationManager : IExposable
{
	public class PhysicalInteractionReservation : IExposable
	{
		public LocalTargetInfo target;

		public Pawn claimant;

		public Job job;

		public void ExposeData()
		{
			Scribe_TargetInfo.Look(ref target, "target");
			Scribe_References.Look(ref claimant, "claimant");
			Scribe_References.Look(ref job, "job");
		}
	}

	private List<PhysicalInteractionReservation> reservations = new List<PhysicalInteractionReservation>();

	public void Reserve(Pawn claimant, Job job, LocalTargetInfo target)
	{
		if (!IsReservedBy(claimant, target))
		{
			PhysicalInteractionReservation physicalInteractionReservation = new PhysicalInteractionReservation();
			physicalInteractionReservation.target = target;
			physicalInteractionReservation.claimant = claimant;
			physicalInteractionReservation.job = job;
			reservations.Add(physicalInteractionReservation);
		}
	}

	public void Release(Pawn claimant, Job job, LocalTargetInfo target)
	{
		for (int i = 0; i < reservations.Count; i++)
		{
			PhysicalInteractionReservation physicalInteractionReservation = reservations[i];
			if (physicalInteractionReservation.target == target && physicalInteractionReservation.claimant == claimant && physicalInteractionReservation.job == job)
			{
				reservations.RemoveAt(i);
				return;
			}
		}
		string obj = claimant?.ToString();
		LocalTargetInfo localTargetInfo = target;
		Log.Warning(obj + " tried to release reservation on target " + localTargetInfo.ToString() + ", but it's not reserved by him.");
	}

	public void TryRelease(Pawn claimant, Job job, LocalTargetInfo target)
	{
		if (IsReservedBy(claimant, target))
		{
			Release(claimant, job, target);
		}
	}

	public bool IsReservedBy(Pawn claimant, LocalTargetInfo target)
	{
		for (int i = 0; i < reservations.Count; i++)
		{
			PhysicalInteractionReservation physicalInteractionReservation = reservations[i];
			if (physicalInteractionReservation.target == target && physicalInteractionReservation.claimant == claimant)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsReserved(LocalTargetInfo target)
	{
		return FirstReserverOf(target) != null;
	}

	public Pawn FirstReserverOf(LocalTargetInfo target)
	{
		for (int i = 0; i < reservations.Count; i++)
		{
			PhysicalInteractionReservation physicalInteractionReservation = reservations[i];
			if (physicalInteractionReservation.target == target)
			{
				return physicalInteractionReservation.claimant;
			}
		}
		return null;
	}

	public void ReserversOf(LocalTargetInfo target, HashSet<Pawn> reserversOut)
	{
		for (int i = 0; i < reservations.Count; i++)
		{
			PhysicalInteractionReservation physicalInteractionReservation = reservations[i];
			if (physicalInteractionReservation.target == target)
			{
				reserversOut.Add(physicalInteractionReservation.claimant);
			}
		}
	}

	public LocalTargetInfo FirstReservationFor(Pawn claimant)
	{
		for (int num = reservations.Count - 1; num >= 0; num--)
		{
			if (reservations[num].claimant == claimant)
			{
				return reservations[num].target;
			}
		}
		return LocalTargetInfo.Invalid;
	}

	public void ReleaseAllForTarget(LocalTargetInfo target)
	{
		reservations.RemoveAll((PhysicalInteractionReservation x) => x.target == target);
	}

	public void ReleaseClaimedBy(Pawn claimant, Job job)
	{
		for (int num = reservations.Count - 1; num >= 0; num--)
		{
			if (reservations[num].claimant == claimant && reservations[num].job == job)
			{
				reservations.RemoveAt(num);
			}
		}
	}

	public void ReleaseAllClaimedBy(Pawn claimant)
	{
		for (int num = reservations.Count - 1; num >= 0; num--)
		{
			if (reservations[num].claimant == claimant)
			{
				reservations.RemoveAt(num);
			}
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref reservations, "reservations", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && reservations.RemoveAll((PhysicalInteractionReservation x) => x.claimant.DestroyedOrNull()) != 0)
		{
			Log.Warning("Some physical interaction reservations had null or destroyed claimant.");
		}
	}
}
