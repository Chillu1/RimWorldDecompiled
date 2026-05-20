using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse.AI;

[StaticConstructorOnStartup]
public sealed class ReservationManager : IExposable
{
	public class Reservation : IExposable
	{
		private Pawn claimant;

		private Job job;

		private LocalTargetInfo target;

		private ReservationLayerDef layer;

		private int maxPawns;

		private int stackCount = -1;

		public Pawn Claimant => claimant;

		public Job Job => job;

		public LocalTargetInfo Target => target;

		public ReservationLayerDef Layer => layer;

		public int MaxPawns => maxPawns;

		public int StackCount => stackCount;

		public Faction Faction => claimant.Faction;

		public Reservation()
		{
		}

		public Reservation(Pawn claimant, Job job, int maxPawns, int stackCount, LocalTargetInfo target, ReservationLayerDef layer)
		{
			this.claimant = claimant;
			this.job = job;
			this.maxPawns = maxPawns;
			this.stackCount = stackCount;
			this.target = target;
			this.layer = layer;
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref claimant, "claimant");
			Scribe_References.Look(ref job, "job");
			Scribe_TargetInfo.Look(ref target, "target");
			Scribe_Values.Look(ref maxPawns, "maxPawns", 0);
			Scribe_Values.Look(ref stackCount, "stackCount", 0);
			Scribe_Defs.Look(ref layer, "layer");
		}

		public override string ToString()
		{
			return ((claimant != null) ? claimant.LabelShort : "null") + ":" + job.ToStringSafe() + ", " + target.ToStringSafe() + ", " + layer.ToStringSafe() + ", " + maxPawns + ", " + stackCount;
		}
	}

	private Map map;

	private List<Reservation> reservations = new List<Reservation>();

	private static readonly Material DebugReservedThingIcon = MaterialPool.MatFrom("UI/Overlays/ReservedForWork", ShaderDatabase.Cutout);

	public const int StackCount_All = -1;

	private List<Pawn> tmpReservers = new List<Pawn>();

	public List<Reservation> ReservationsReadOnly => reservations;

	public ReservationManager(Map map)
	{
		this.map = map;
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref reservations, "reservations", LookMode.Deep);
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		for (int num = reservations.Count - 1; num >= 0; num--)
		{
			Reservation reservation = reservations[num];
			if (reservation.Target.Thing != null && reservation.Target.Thing.Destroyed)
			{
				Log.Error("Loaded reservation with destroyed target: " + reservation?.ToString() + ". Deleting it...");
				reservations.Remove(reservation);
			}
			if (reservation.Claimant != null && reservation.Claimant.Destroyed)
			{
				Log.Error("Loaded reservation with destroyed claimant: " + reservation?.ToString() + ". Deleting it...");
				reservations.Remove(reservation);
			}
			if (reservation.Claimant == null)
			{
				Log.Error("Loaded reservation with null claimant: " + reservation?.ToString() + ". Deleting it...");
				reservations.Remove(reservation);
			}
			if (reservation.Job == null)
			{
				Log.Error("Loaded reservation with null job: " + reservation?.ToString() + ". Deleting it...");
				reservations.Remove(reservation);
			}
		}
	}

	public bool IsReserved(LocalTargetInfo target)
	{
		for (int i = 0; i < reservations.Count; i++)
		{
			if (reservations[i].Target == target)
			{
				return true;
			}
		}
		return false;
	}

	public void ReserversOf(LocalTargetInfo target, HashSet<Pawn> reserversOut)
	{
		map.physicalInteractionReservationManager.ReserversOf(target, reserversOut);
		for (int i = 0; i < reservations.Count; i++)
		{
			Reservation reservation = reservations[i];
			if (reservation.Target == target)
			{
				reserversOut.Add(reservation.Claimant);
			}
		}
	}

	public bool HasReservedOfDef(Pawn claimant, ThingDef thingDef)
	{
		foreach (Reservation reservation in reservations)
		{
			if (reservation.Target.HasThing && reservation.Target.Thing.def == thingDef && reservation.Claimant == claimant)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanReserve(Pawn claimant, LocalTargetInfo target, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
	{
		if (claimant == null)
		{
			Log.Error("CanReserve with null claimant");
			return false;
		}
		if (!claimant.Spawned || claimant.Map != map)
		{
			return false;
		}
		if (!target.IsValid || target.ThingDestroyed)
		{
			return false;
		}
		if (target.HasThing && target.Thing.SpawnedOrAnyParentSpawned && target.Thing.MapHeld != map)
		{
			return false;
		}
		int num = ((!target.HasThing) ? 1 : target.Thing.stackCount);
		int num2 = ((stackCount == -1) ? num : stackCount);
		if (num2 > num)
		{
			return false;
		}
		if (!ignoreOtherReservations)
		{
			if (map.physicalInteractionReservationManager.IsReserved(target) && !map.physicalInteractionReservationManager.IsReservedBy(claimant, target))
			{
				return false;
			}
			if (IsAlreadyReserved(claimant, target, layer, num2))
			{
				return true;
			}
			if (target.HasThing && target.Thing is Building building && building.def.hasInteractionCell)
			{
				IntVec3 interactionCell = building.InteractionCell;
				Building edifice = interactionCell.GetEdifice(claimant.Map);
				Pawn reserver2;
				if (edifice != null)
				{
					if (claimant.Map.reservationManager.TryGetReserver(edifice, claimant.Faction, out var reserver) && reserver.Spawned && reserver != claimant)
					{
						return false;
					}
				}
				else if (claimant.Map.reservationManager.TryGetReserver(interactionCell, claimant.Faction, out reserver2) && reserver2.Spawned && reserver2 != claimant)
				{
					return false;
				}
			}
			else if (IsAlreadyReserved(claimant, target, layer, num2))
			{
				return true;
			}
			tmpReservers.Clear();
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < reservations.Count; i++)
			{
				Reservation reservation = reservations[i];
				if (!(reservation.Target != target) && reservation.Layer == layer && reservation.Claimant != claimant && !tmpReservers.Contains(reservation.Claimant) && RespectsReservationsOf(claimant, reservation.Claimant))
				{
					if (reservation.MaxPawns != maxPawns)
					{
						return false;
					}
					tmpReservers.Add(reservation.Claimant);
					num3++;
					num4 = ((reservation.StackCount != -1) ? (num4 + reservation.StackCount) : (num4 + num));
					if (num3 >= maxPawns || num2 + num4 > num)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private bool IsAlreadyReserved(Pawn claimant, LocalTargetInfo target, ReservationLayerDef layer, int toReserveResolved)
	{
		for (int i = 0; i < reservations.Count; i++)
		{
			Reservation reservation = reservations[i];
			if (reservation.Target == target && reservation.Layer == layer && reservation.Claimant == claimant && (reservation.StackCount == -1 || reservation.StackCount >= toReserveResolved))
			{
				return true;
			}
		}
		return false;
	}

	public int CanReserveStack(Pawn claimant, LocalTargetInfo target, int maxPawns = 1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
	{
		if (claimant == null)
		{
			Log.Error("CanReserve with null claimant");
			return 0;
		}
		if (!claimant.Spawned || claimant.Map != map)
		{
			return 0;
		}
		if (!target.IsValid || target.ThingDestroyed)
		{
			return 0;
		}
		if (target.HasThing && target.Thing.SpawnedOrAnyParentSpawned && target.Thing.MapHeld != map)
		{
			return 0;
		}
		int num = ((!target.HasThing) ? 1 : target.Thing.stackCount);
		int num2 = 0;
		if (!ignoreOtherReservations)
		{
			if (map.physicalInteractionReservationManager.IsReserved(target) && !map.physicalInteractionReservationManager.IsReservedBy(claimant, target))
			{
				return 0;
			}
			int num3 = 0;
			for (int i = 0; i < reservations.Count; i++)
			{
				Reservation reservation = reservations[i];
				if (!(reservation.Target != target) && reservation.Layer == layer && reservation.Claimant != claimant && RespectsReservationsOf(claimant, reservation.Claimant))
				{
					if (reservation.MaxPawns != maxPawns)
					{
						return 0;
					}
					num3++;
					num2 = ((reservation.StackCount != -1) ? (num2 + reservation.StackCount) : (num2 + num));
					if (num3 >= maxPawns || num2 >= num)
					{
						return 0;
					}
				}
			}
		}
		return Mathf.Max(num - num2, 0);
	}

	public bool Reserve(Pawn claimant, Job job, LocalTargetInfo target, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool errorOnFailed = true, bool ignoreOtherReservations = false, bool canReserversStartJobs = true)
	{
		if (maxPawns > 1 && stackCount == -1)
		{
			Log.ErrorOnce("Reserving with maxPawns > 1 and stackCount = All; this will not have a useful effect (suppressing future warnings)", 83269);
		}
		if (job == null)
		{
			Log.Warning(claimant.ToStringSafe() + " tried to reserve thing " + target.ToStringSafe() + " without a valid job");
			return false;
		}
		int num = ((!target.HasThing) ? 1 : target.Thing.stackCount);
		int num2 = ((stackCount == -1) ? num : stackCount);
		for (int i = 0; i < reservations.Count; i++)
		{
			Reservation reservation = reservations[i];
			if (reservation.Target == target && reservation.Claimant == claimant && reservation.Job == job && reservation.Layer == layer && (reservation.StackCount == -1 || reservation.StackCount >= num2))
			{
				return true;
			}
		}
		if (!target.IsValid || target.ThingDestroyed)
		{
			return false;
		}
		if (!CanReserve(claimant, target, maxPawns, stackCount, layer, ignoreOtherReservations))
		{
			if (job.playerForced && CanReserve(claimant, target, maxPawns, stackCount, layer, ignoreOtherReservations: true))
			{
				reservations.Add(new Reservation(claimant, job, maxPawns, stackCount, target, layer));
				foreach (Reservation item in reservations.ToList())
				{
					if (item.Claimant == claimant || item.Layer != layer || !RespectsReservationsOf(claimant, item.Claimant))
					{
						continue;
					}
					if (item.Target == target)
					{
						item.Claimant.jobs.EndCurrentOrQueuedJob(item.Job, JobCondition.InterruptForced, canReserversStartJobs);
					}
					else
					{
						if (!target.HasThing || !(target.Thing is Building building) || !building.def.hasInteractionCell)
						{
							continue;
						}
						IntVec3 interactionCell = building.InteractionCell;
						Building edifice = interactionCell.GetEdifice(claimant.Map);
						if (item.Claimant != claimant && item.Layer == layer && RespectsReservationsOf(claimant, item.Claimant))
						{
							if (edifice != null && item.Target == edifice)
							{
								item.Claimant.jobs.EndCurrentOrQueuedJob(item.Job, JobCondition.InterruptForced);
							}
							else if (item.Target == interactionCell)
							{
								item.Claimant.jobs.EndCurrentOrQueuedJob(item.Job, JobCondition.InterruptForced);
							}
						}
					}
				}
				return true;
			}
			if (errorOnFailed)
			{
				LogCouldNotReserveError(claimant, job, target, maxPawns, stackCount, layer);
			}
			return false;
		}
		Reservation reservation2 = new Reservation(claimant, job, maxPawns, stackCount, target, layer);
		reservations.Add(reservation2);
		map.events.Notify_ReservationAdded(reservation2);
		return true;
	}

	public void Release(LocalTargetInfo target, Pawn claimant, Job job)
	{
		if (target.ThingDestroyed)
		{
			LocalTargetInfo localTargetInfo = target;
			Log.Warning("Releasing destroyed thing " + localTargetInfo.ToString() + " for " + claimant);
		}
		Reservation reservation = null;
		for (int i = 0; i < reservations.Count; i++)
		{
			Reservation reservation2 = reservations[i];
			if (reservation2.Target == target && reservation2.Claimant == claimant && reservation2.Job == job)
			{
				reservation = reservation2;
				break;
			}
		}
		if (reservation == null && !target.ThingDestroyed)
		{
			string[] obj = new string[5] { "Tried to release ", null, null, null, null };
			LocalTargetInfo localTargetInfo = target;
			obj[1] = localTargetInfo.ToString();
			obj[2] = " that wasn't reserved by ";
			obj[3] = claimant?.ToString();
			obj[4] = ".";
			Log.Error(string.Concat(obj));
		}
		else
		{
			reservations.Remove(reservation);
			map.events.Notify_ReservationRemoved(reservation);
		}
	}

	public void ReleaseAllForTarget(Thing t)
	{
		if (t == null)
		{
			return;
		}
		for (int num = reservations.Count - 1; num >= 0; num--)
		{
			if (reservations[num].Target.Thing == t)
			{
				reservations.RemoveAt(num);
			}
		}
	}

	public void ReleaseClaimedBy(Pawn claimant, Job job)
	{
		for (int num = reservations.Count - 1; num >= 0; num--)
		{
			if (reservations[num].Claimant == claimant && reservations[num].Job == job)
			{
				reservations.RemoveAt(num);
			}
		}
	}

	public void ReleaseAllClaimedBy(Pawn claimant)
	{
		if (claimant == null)
		{
			return;
		}
		for (int num = reservations.Count - 1; num >= 0; num--)
		{
			if (reservations[num].Claimant == claimant)
			{
				reservations.RemoveAt(num);
			}
		}
	}

	public LocalTargetInfo FirstReservationFor(Pawn claimant)
	{
		if (claimant == null)
		{
			return LocalTargetInfo.Invalid;
		}
		for (int i = 0; i < reservations.Count; i++)
		{
			if (reservations[i].Claimant == claimant)
			{
				return reservations[i].Target;
			}
		}
		return LocalTargetInfo.Invalid;
	}

	public bool IsReservedByAnyoneOf(LocalTargetInfo target, Faction faction)
	{
		Pawn reserver;
		return TryGetReserver(target, faction, out reserver);
	}

	public bool TryGetReserver(LocalTargetInfo target, Faction faction, out Pawn reserver)
	{
		reserver = null;
		if (!target.IsValid)
		{
			return false;
		}
		for (int i = 0; i < reservations.Count; i++)
		{
			Reservation reservation = reservations[i];
			if (reservation.Target == target && reservation.Claimant.Faction == faction)
			{
				reserver = reservation.Claimant;
				return true;
			}
		}
		return false;
	}

	public bool OnlyReservationsForJobDef(LocalTargetInfo target, JobDef requiredJobDef, bool requireAtLeastOne = false)
	{
		if (!target.IsValid)
		{
			return false;
		}
		bool flag = false;
		for (int i = 0; i < reservations.Count; i++)
		{
			Reservation reservation = reservations[i];
			if (!(reservation.Target != target))
			{
				if (reservation.Job.def != requiredJobDef)
				{
					return false;
				}
				flag = true;
			}
		}
		return !requireAtLeastOne || flag;
	}

	public bool IsReservedAndRespected(LocalTargetInfo target, Pawn claimant)
	{
		return FirstRespectedReserver(target, claimant) != null;
	}

	public bool IsReservedAndRespected(LocalTargetInfo target, Faction faction)
	{
		if (!target.IsValid)
		{
			return false;
		}
		for (int i = 0; i < reservations.Count; i++)
		{
			Reservation reservation = reservations[i];
			if (!(reservation.Target != target) && RespectsReservationsOf(faction, reservation.Claimant))
			{
				return true;
			}
		}
		return false;
	}

	public Pawn FirstRespectedReserver(LocalTargetInfo target, Pawn claimant, ReservationLayerDef layer = null)
	{
		if (!target.IsValid)
		{
			return null;
		}
		for (int i = 0; i < reservations.Count; i++)
		{
			Reservation reservation = reservations[i];
			if (reservation.Target == target && (layer == null || reservation.Layer == layer) && RespectsReservationsOf(claimant, reservation.Claimant))
			{
				return reservation.Claimant;
			}
		}
		return null;
	}

	public bool ReservedBy(LocalTargetInfo target, Pawn claimant, Job job = null)
	{
		if (!target.IsValid)
		{
			return false;
		}
		for (int i = 0; i < reservations.Count; i++)
		{
			Reservation reservation = reservations[i];
			if (reservation.Target == target && reservation.Claimant == claimant && (job == null || reservation.Job == job))
			{
				return true;
			}
		}
		return false;
	}

	public bool ReservedBy<TDriver>(LocalTargetInfo target, Pawn claimant, LocalTargetInfo? targetAIsNot = null, LocalTargetInfo? targetBIsNot = null, LocalTargetInfo? targetCIsNot = null)
	{
		if (!target.IsValid)
		{
			return false;
		}
		for (int i = 0; i < reservations.Count; i++)
		{
			Reservation reservation = reservations[i];
			if (!(reservation.Target == target) || reservation.Claimant != claimant || reservation.Job == null || !typeof(TDriver).IsAssignableFrom(reservation.Job.def.driverClass))
			{
				continue;
			}
			if (targetAIsNot.HasValue)
			{
				LocalTargetInfo targetA = reservation.Job.targetA;
				LocalTargetInfo? localTargetInfo = targetAIsNot;
				if (!(targetA != localTargetInfo))
				{
					continue;
				}
			}
			if (targetBIsNot.HasValue)
			{
				LocalTargetInfo targetA = reservation.Job.targetB;
				LocalTargetInfo? localTargetInfo = targetBIsNot;
				if (!(targetA != localTargetInfo))
				{
					continue;
				}
			}
			if (targetCIsNot.HasValue)
			{
				LocalTargetInfo targetA = reservation.Job.targetC;
				LocalTargetInfo? localTargetInfo = targetCIsNot;
				if (!(targetA != localTargetInfo))
				{
					continue;
				}
			}
			return true;
		}
		return false;
	}

	public IEnumerable<Thing> AllReservedThings()
	{
		return reservations.Select((Reservation res) => res.Target.Thing);
	}

	private static bool RespectsReservationsOf(Faction faction, Pawn claimant)
	{
		if (faction == null || claimant?.Faction == null)
		{
			return false;
		}
		if (faction == claimant.Faction)
		{
			return true;
		}
		if (!faction.HostileTo(claimant.Faction))
		{
			return true;
		}
		if (faction == claimant.HostFaction)
		{
			return true;
		}
		return false;
	}

	private static bool RespectsReservationsOf(Pawn newClaimant, Pawn oldClaimant)
	{
		if (newClaimant == oldClaimant)
		{
			return true;
		}
		if (newClaimant.Faction == null || oldClaimant.Faction == null)
		{
			return false;
		}
		if (newClaimant.Faction == oldClaimant.Faction)
		{
			return true;
		}
		if (!newClaimant.Faction.HostileTo(oldClaimant.Faction))
		{
			return true;
		}
		if (oldClaimant.HostFaction != null && oldClaimant.HostFaction == newClaimant.HostFaction)
		{
			return true;
		}
		if (newClaimant.HostFaction != null)
		{
			if (oldClaimant.HostFaction != null)
			{
				return true;
			}
			if (newClaimant.HostFaction == oldClaimant.Faction)
			{
				return true;
			}
		}
		return false;
	}

	internal string DebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("All reservation in ReservationManager:");
		for (int i = 0; i < reservations.Count; i++)
		{
			stringBuilder.AppendLine("[" + i + "] " + reservations[i].ToString());
		}
		return stringBuilder.ToString();
	}

	internal void DebugDrawReservations()
	{
		for (int i = 0; i < reservations.Count; i++)
		{
			Reservation reservation = reservations[i];
			if (reservation.Target.Thing != null)
			{
				if (reservation.Target.Thing.Spawned)
				{
					Thing thing = reservation.Target.Thing;
					Vector3 s = new Vector3(thing.RotatedSize.x, 1f, thing.RotatedSize.z);
					Matrix4x4 matrix = default(Matrix4x4);
					matrix.SetTRS(thing.DrawPos + Vector3.up * 0.1f, Quaternion.identity, s);
					Graphics.DrawMesh(MeshPool.plane10, matrix, DebugReservedThingIcon, 0);
					GenDraw.DrawLineBetween(reservation.Claimant.DrawPos, reservation.Target.Thing.DrawPos);
				}
				else
				{
					Graphics.DrawMesh(MeshPool.plane03, reservation.Claimant.DrawPos + Vector3.up + new Vector3(0.5f, 0f, 0.5f), Quaternion.identity, DebugReservedThingIcon, 0);
				}
			}
			else
			{
				Graphics.DrawMesh(MeshPool.plane10, reservation.Target.Cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), Quaternion.identity, DebugReservedThingIcon, 0);
				GenDraw.DrawLineBetween(reservation.Claimant.DrawPos, reservation.Target.Cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays));
			}
		}
	}

	private void LogCouldNotReserveError(Pawn claimant, Job job, LocalTargetInfo target, int maxPawns, int stackCount, ReservationLayerDef layer)
	{
		Job curJob = claimant.CurJob;
		string text = "null";
		int num = -1;
		if (curJob != null)
		{
			text = curJob.ToString();
			if (claimant.jobs.curDriver != null)
			{
				num = claimant.jobs.curDriver.CurToilIndex;
			}
		}
		string text2 = ((!target.HasThing || target.Thing.def.stackLimit == 1) ? "" : ("(current stack count: " + target.Thing.stackCount + ")"));
		string text3 = "Could not reserve " + target.ToStringSafe() + text2 + " (layer: " + layer.ToStringSafe() + ") for " + claimant.ToStringSafe() + " for job " + job.ToStringSafe() + " (now doing job " + text + "(curToil=" + num + ")) for maxPawns " + maxPawns + " and stackCount " + stackCount + ".";
		int num2 = 0;
		bool flag = false;
		for (int i = 0; i < reservations.Count; i++)
		{
			Reservation reservation = reservations[i];
			if (!(reservation.Target == target) || (layer != null && reservation.Layer != layer) || !RespectsReservationsOf(claimant, reservation.Claimant))
			{
				continue;
			}
			if (!flag)
			{
				text3 += "\nExisting reservers:";
				flag = true;
			}
			string text4 = "null";
			int num3 = -1;
			Job curJob2 = reservation.Claimant.CurJob;
			if (curJob2 != null)
			{
				text4 = curJob2.ToStringSafe();
				if (reservation.Claimant.jobs.curDriver != null)
				{
					num3 = reservation.Claimant.jobs.curDriver.CurToilIndex;
				}
			}
			text3 += $"\n   [{num2++}] {reservation.Claimant.ToStringSafe()} (job: {text4}, toil: {num3}, maxPawns: {reservation.MaxPawns}, stackCount: {reservation.StackCount})";
		}
		if (!flag)
		{
			text3 += " No existing reserver.";
		}
		Pawn pawn = map.physicalInteractionReservationManager.FirstReserverOf(target);
		if (pawn != null)
		{
			text3 = text3 + " Physical interaction reserver: " + pawn.ToStringSafe();
		}
		Log.Error(text3);
	}
}
