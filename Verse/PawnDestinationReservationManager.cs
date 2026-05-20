using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse;

[StaticConstructorOnStartup]
public sealed class PawnDestinationReservationManager : IExposable
{
	public class PawnDestinationReservation : IExposable
	{
		public IntVec3 target;

		public Pawn claimant;

		public Job job;

		public bool obsolete;

		public void ExposeData()
		{
			Scribe_Values.Look(ref target, "target");
			Scribe_References.Look(ref claimant, "claimant");
			Scribe_References.Look(ref job, "job");
			Scribe_Values.Look(ref obsolete, "obsolete", defaultValue: false);
		}
	}

	public class PawnDestinationSet : IExposable
	{
		public List<PawnDestinationReservation> list = new List<PawnDestinationReservation>();

		public void ExposeData()
		{
			Scribe_Collections.Look(ref list, "list", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && list.RemoveAll((PawnDestinationReservation x) => x.claimant.DestroyedOrNull()) != 0)
			{
				Log.Warning("Some destination reservations had null or destroyed claimant.");
			}
		}
	}

	private Dictionary<Faction, PawnDestinationSet> reservedDestinations = new Dictionary<Faction, PawnDestinationSet>();

	private static readonly Material DestinationMat = MaterialPool.MatFrom("UI/Overlays/ReservedDestination");

	private static readonly Material DestinationSelectionMat = MaterialPool.MatFrom("UI/Overlays/ReservedDestinationSelection");

	private List<Faction> reservedDestinationsKeysWorkingList;

	private List<PawnDestinationSet> reservedDestinationsValuesWorkingList;

	public PawnDestinationSet GetPawnDestinationSetFor(Faction faction)
	{
		if (!reservedDestinations.ContainsKey(faction))
		{
			reservedDestinations.Add(faction, new PawnDestinationSet());
		}
		return reservedDestinations[faction];
	}

	public void Reserve(Pawn p, Job job, IntVec3 loc)
	{
		if (p.Faction == null)
		{
			return;
		}
		if (p.Drafted && p.Faction == Faction.OfPlayer && IsReserved(loc, out var claimant) && claimant != p && !claimant.HostileTo(p) && claimant.Faction != p.Faction)
		{
			MentalStateDef mentalStateDef = claimant.MentalStateDef;
			if (mentalStateDef == null || mentalStateDef.category != MentalStateCategory.Aggro)
			{
				MentalStateDef mentalStateDef2 = claimant.MentalStateDef;
				if (mentalStateDef2 == null || mentalStateDef2.category != MentalStateCategory.Malicious)
				{
					claimant.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
			}
		}
		ObsoleteAllClaimedBy(p);
		GetPawnDestinationSetFor(p.Faction).list.Add(new PawnDestinationReservation
		{
			target = loc,
			claimant = p,
			job = job
		});
	}

	public PawnDestinationReservation MostRecentReservationFor(Pawn p)
	{
		if (p.Faction == null)
		{
			return null;
		}
		List<PawnDestinationReservation> list = GetPawnDestinationSetFor(p.Faction).list;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].claimant == p && !list[i].obsolete)
			{
				return list[i];
			}
		}
		return null;
	}

	public IntVec3 FirstObsoleteReservationFor(Pawn p)
	{
		if (p.Faction == null)
		{
			return IntVec3.Invalid;
		}
		List<PawnDestinationReservation> list = GetPawnDestinationSetFor(p.Faction).list;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].claimant == p && list[i].obsolete)
			{
				return list[i].target;
			}
		}
		return IntVec3.Invalid;
	}

	public Job FirstObsoleteReservationJobFor(Pawn p)
	{
		if (p.Faction == null)
		{
			return null;
		}
		List<PawnDestinationReservation> list = GetPawnDestinationSetFor(p.Faction).list;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].claimant == p && list[i].obsolete)
			{
				return list[i].job;
			}
		}
		return null;
	}

	public bool IsReserved(IntVec3 loc)
	{
		Pawn claimant;
		return IsReserved(loc, out claimant);
	}

	public bool IsReserved(IntVec3 loc, out Pawn claimant)
	{
		foreach (KeyValuePair<Faction, PawnDestinationSet> reservedDestination in reservedDestinations)
		{
			List<PawnDestinationReservation> list = reservedDestination.Value.list;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].target == loc)
				{
					claimant = list[i].claimant;
					return true;
				}
			}
		}
		claimant = null;
		return false;
	}

	public bool CanReserve(IntVec3 c, Pawn searcher, bool draftedOnly = false)
	{
		if (searcher.Faction == null)
		{
			return true;
		}
		if (searcher.Faction == Faction.OfPlayer)
		{
			return CanReserveInt(c, searcher.Faction, searcher, draftedOnly);
		}
		foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
		{
			if (!item.HostileTo(searcher.Faction) && !CanReserveInt(c, item, searcher, draftedOnly))
			{
				return false;
			}
		}
		return true;
	}

	private bool CanReserveInt(IntVec3 c, Faction faction, Pawn ignoreClaimant = null, bool draftedOnly = false)
	{
		if (faction == null)
		{
			return true;
		}
		List<PawnDestinationReservation> list = GetPawnDestinationSetFor(faction).list;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].target == c && (ignoreClaimant == null || list[i].claimant != ignoreClaimant) && (!draftedOnly || list[i].claimant.Drafted))
			{
				return false;
			}
		}
		return true;
	}

	public Pawn FirstReserverOf(IntVec3 c, Faction faction)
	{
		if (faction == null)
		{
			return null;
		}
		List<PawnDestinationReservation> list = GetPawnDestinationSetFor(faction).list;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].target == c)
			{
				return list[i].claimant;
			}
		}
		return null;
	}

	public void ReleaseAllObsoleteClaimedBy(Pawn p)
	{
		if (p.Faction == null)
		{
			return;
		}
		List<PawnDestinationReservation> list = GetPawnDestinationSetFor(p.Faction).list;
		int num = 0;
		while (num < list.Count)
		{
			if (list[num].claimant == p && list[num].obsolete)
			{
				list[num] = list[list.Count - 1];
				list.RemoveLast();
			}
			else
			{
				num++;
			}
		}
	}

	public void ReleaseAllClaimedBy(Pawn p)
	{
		if (p.Faction == null)
		{
			return;
		}
		List<PawnDestinationReservation> list = GetPawnDestinationSetFor(p.Faction).list;
		int num = 0;
		while (num < list.Count)
		{
			if (list[num].claimant == p)
			{
				list[num] = list[list.Count - 1];
				list.RemoveLast();
			}
			else
			{
				num++;
			}
		}
	}

	public void ReleaseClaimedBy(Pawn p, Job job)
	{
		if (p.Faction == null)
		{
			return;
		}
		List<PawnDestinationReservation> list = GetPawnDestinationSetFor(p.Faction).list;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].claimant == p && list[i].job == job)
			{
				list[i].job = null;
				if (list[i].obsolete)
				{
					list[i] = list[list.Count - 1];
					list.RemoveLast();
					i--;
				}
			}
		}
	}

	public void Notify_FactionRemoved(Faction faction)
	{
		if (reservedDestinations.ContainsKey(faction))
		{
			reservedDestinations.Remove(faction);
		}
	}

	public void ObsoleteAllClaimedBy(Pawn p)
	{
		if (p.Faction == null)
		{
			return;
		}
		List<PawnDestinationReservation> list = GetPawnDestinationSetFor(p.Faction).list;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].claimant == p)
			{
				list[i].obsolete = true;
				if (list[i].job == null)
				{
					list[i] = list[list.Count - 1];
					list.RemoveLast();
					i--;
				}
			}
		}
	}

	public void DebugDrawDestinations()
	{
		foreach (PawnDestinationReservation item in GetPawnDestinationSetFor(Faction.OfPlayer).list)
		{
			if (!(item.claimant.Position == item.target))
			{
				IntVec3 target = item.target;
				Vector3 s = new Vector3(1f, 1f, 1f);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(target.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), Quaternion.identity, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, DestinationMat, 0);
				if (Find.Selector.IsSelected(item.claimant))
				{
					Graphics.DrawMesh(MeshPool.plane10, matrix, DestinationSelectionMat, 0);
				}
			}
		}
	}

	public void DebugDrawReservations()
	{
		foreach (KeyValuePair<Faction, PawnDestinationSet> reservedDestination in reservedDestinations)
		{
			foreach (PawnDestinationReservation item in reservedDestination.Value.list)
			{
				IntVec3 target = item.target;
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				materialPropertyBlock.SetColor("_Color", reservedDestination.Key.Color);
				Vector3 s = new Vector3(1f, 1f, 1f);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(target.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), Quaternion.identity, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, DestinationMat, 0, Camera.main, 0, materialPropertyBlock);
				if (Find.Selector.IsSelected(item.claimant))
				{
					Graphics.DrawMesh(MeshPool.plane10, matrix, DestinationSelectionMat, 0);
				}
			}
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref reservedDestinations, "reservedDestinations", LookMode.Reference, LookMode.Deep, ref reservedDestinationsKeysWorkingList, ref reservedDestinationsValuesWorkingList);
	}
}
