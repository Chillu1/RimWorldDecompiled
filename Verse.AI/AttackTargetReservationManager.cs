using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class AttackTargetReservationManager : IExposable
{
	public class AttackTargetReservation : IExposable
	{
		public IAttackTarget target;

		public Pawn claimant;

		public Job job;

		public void ExposeData()
		{
			Scribe_References.Look(ref target, "target");
			Scribe_References.Look(ref claimant, "claimant");
			Scribe_References.Look(ref job, "job");
		}
	}

	private Map map;

	private List<AttackTargetReservation> reservations = new List<AttackTargetReservation>();

	public AttackTargetReservationManager(Map map)
	{
		this.map = map;
	}

	public void Reserve(Pawn claimant, Job job, IAttackTarget target)
	{
		if (target == null)
		{
			Log.Warning(claimant?.ToString() + " tried to reserve null attack target.");
		}
		else if (!IsReservedBy(claimant, target))
		{
			AttackTargetReservation attackTargetReservation = new AttackTargetReservation();
			attackTargetReservation.target = target;
			attackTargetReservation.claimant = claimant;
			attackTargetReservation.job = job;
			reservations.Add(attackTargetReservation);
		}
	}

	public void Release(Pawn claimant, Job job, IAttackTarget target)
	{
		if (target == null)
		{
			Log.Warning(claimant?.ToString() + " tried to release reservation on null attack target.");
			return;
		}
		for (int i = 0; i < reservations.Count; i++)
		{
			AttackTargetReservation attackTargetReservation = reservations[i];
			if (attackTargetReservation.target == target && attackTargetReservation.claimant == claimant && attackTargetReservation.job == job)
			{
				reservations.RemoveAt(i);
				return;
			}
		}
		Log.Warning(claimant?.ToString() + " with job " + job?.ToString() + " tried to release reservation on target " + target?.ToString() + ", but it's not reserved by him.");
	}

	public bool CanReserve(Pawn claimant, IAttackTarget target)
	{
		if (IsReservedBy(claimant, target))
		{
			return true;
		}
		int reservationsCount = GetReservationsCount(target, claimant.Faction);
		int maxPreferredReservationsCount = GetMaxPreferredReservationsCount(target);
		return reservationsCount < maxPreferredReservationsCount;
	}

	public bool IsReservedBy(Pawn claimant, IAttackTarget target)
	{
		for (int i = 0; i < reservations.Count; i++)
		{
			AttackTargetReservation attackTargetReservation = reservations[i];
			if (attackTargetReservation.target == target && attackTargetReservation.claimant == claimant)
			{
				return true;
			}
		}
		return false;
	}

	public void ReleaseAllForTarget(IAttackTarget target)
	{
		reservations.RemoveAll((AttackTargetReservation x) => x.target == target);
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

	public IAttackTarget FirstReservationFor(Pawn claimant)
	{
		for (int num = reservations.Count - 1; num >= 0; num--)
		{
			if (reservations[num].claimant == claimant)
			{
				return reservations[num].target;
			}
		}
		return null;
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref reservations, "reservations", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			reservations.RemoveAll((AttackTargetReservation x) => x.target == null);
			if (reservations.RemoveAll((AttackTargetReservation x) => x.claimant.DestroyedOrNull()) != 0)
			{
				Log.Warning("Some attack target reservations had null or destroyed claimant.");
			}
		}
	}

	private int GetReservationsCount(IAttackTarget target, Faction faction)
	{
		int num = 0;
		for (int i = 0; i < reservations.Count; i++)
		{
			AttackTargetReservation attackTargetReservation = reservations[i];
			if (attackTargetReservation.target == target && attackTargetReservation.claimant.Faction == faction)
			{
				num++;
			}
		}
		return num;
	}

	private int GetMaxPreferredReservationsCount(IAttackTarget target)
	{
		int num = 0;
		CellRect cellRect = target.Thing.OccupiedRect();
		foreach (IntVec3 item in cellRect.ExpandedBy(1))
		{
			if (!cellRect.Contains(item) && item.InBounds(map) && item.Standable(map))
			{
				num++;
			}
		}
		return num;
	}
}
