using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse.AI.Group;

public class PsychicRitualRoleAssignments : ILordJobAssignmentsManager<PsychicRitualRoleDef>, IExposable
{
	protected Dictionary<PsychicRitualRoleDef, List<Pawn>> roleAssignments;

	protected Dictionary<Pawn, PsychicRitualRoleDef> pawnAssignments;

	protected List<Pawn> spectators;

	protected TargetInfo target;

	protected Dictionary<Pawn, PsychicRitualRoleDef> forcedAssignments;

	private List<PsychicRitualRoleDef> roleAssignmentsKeys;

	private List<List<Pawn>> roleAssignmentsValues;

	private List<Pawn> pawnAssignmentsKeys;

	private List<PsychicRitualRoleDef> pawnAssignmentsValues;

	private List<Pawn> forcedAssignmentKeys;

	private List<PsychicRitualRoleDef> forcedAssignmentValues;

	public IEnumerable<Pawn> AllAssignedPawns => pawnAssignments.Keys;

	public int AssignedPawnCount => pawnAssignments.Count;

	public List<Pawn> SpectatorsForReading => spectators;

	public bool SpectatorsAllowed => false;

	public IReadOnlyDictionary<PsychicRitualRoleDef, List<Pawn>> RoleAssignments => roleAssignments;

	public virtual TargetInfo Target => target;

	public bool PawnParticipating(Pawn pawn)
	{
		return pawnAssignments.ContainsKey(pawn);
	}

	public bool Required(Pawn pawn)
	{
		return ForcedRole(pawn) != null;
	}

	protected PsychicRitualRoleAssignments()
	{
	}

	public PsychicRitualRoleAssignments(List<PsychicRitualRoleDef> roles, TargetInfo target, Dictionary<Pawn, PsychicRitualRoleDef> forcedAssignments = null)
	{
		this.target = target;
		int num = 0;
		roleAssignments = new Dictionary<PsychicRitualRoleDef, List<Pawn>>(roles.Count);
		foreach (PsychicRitualRoleDef role in roles)
		{
			num += role.MaxCount;
			roleAssignments[role] = new List<Pawn>(role.MaxCount);
		}
		pawnAssignments = new Dictionary<Pawn, PsychicRitualRoleDef>(num);
		spectators = new List<Pawn>();
		if (forcedAssignments == null)
		{
			return;
		}
		this.forcedAssignments = new Dictionary<Pawn, PsychicRitualRoleDef>(forcedAssignments);
		foreach (var (pawn2, psychicRitualRoleDef2) in forcedAssignments)
		{
			if (!TryAssign(pawn2, psychicRitualRoleDef2, out var reason))
			{
				throw new Exception($"Cannot assign pawn {pawn2.ToStringSafe()} to forced role {psychicRitualRoleDef2.ToStringSafe()}. Reason: {reason.ToPlayerReadable()}");
			}
		}
	}

	public void AddForcedRole(Pawn pawn, PsychicRitualRoleDef role, PsychicRitualRoleDef.Context context = PsychicRitualRoleDef.Context.Dialog_BeginPsychicRitual)
	{
		if (forcedAssignments == null)
		{
			forcedAssignments = new Dictionary<Pawn, PsychicRitualRoleDef>();
		}
		forcedAssignments[pawn] = role;
		if (!TryAssign(pawn, role, out var reason, context))
		{
			throw new InvalidOperationException($"Cannot force pawn {pawn.ToStringSafe()} to have role {role.ToStringSafe()}. Reason: {reason.ToPlayerReadable()}");
		}
	}

	public virtual bool CanParticipate(Pawn pawn, out TaggedString reason)
	{
		reason = TaggedString.Empty;
		foreach (PsychicRitualRoleDef key in roleAssignments.Keys)
		{
			if (key.PawnCanDo(PsychicRitualRoleDef.Context.Dialog_BeginPsychicRitual, pawn, target, out var reason2))
			{
				reason = TaggedString.Empty;
				return true;
			}
			if (reason.NullOrEmpty())
			{
				reason = reason2.ToPlayerReadable().CapitalizeFirst().EndWithPeriod();
			}
		}
		return false;
	}

	public virtual string PawnNotAssignableReason(Pawn pawn, PsychicRitualRoleDef role)
	{
		if (role != null)
		{
			PsychicRitualRoleDef psychicRitualRoleDef = forcedAssignments?.TryGetValue(pawn);
			if (psychicRitualRoleDef != null && role != psychicRitualRoleDef)
			{
				return "RoleIsLocked".Translate(role.label);
			}
			if (!role.PawnCanDo(PsychicRitualRoleDef.Context.Dialog_BeginPsychicRitual, pawn, target, out var reason))
			{
				return reason.ToPlayerReadable().CapitalizeFirst().EndWithPeriod();
			}
			return null;
		}
		if (CanParticipate(pawn, out var reason2))
		{
			return null;
		}
		return reason2;
	}

	public int RoleAssignedCount(PsychicRitualRoleDef role)
	{
		if (!roleAssignments.TryGetValue(role, out var value))
		{
			return 0;
		}
		return value.Count;
	}

	public bool PawnSpectating(Pawn pawn)
	{
		return false;
	}

	public bool TryAssignSpectate(Pawn pawn, Pawn insertBefore = null)
	{
		if (IsChanter(pawn))
		{
			return false;
		}
		if (TryAssign(pawn, PsychicRitualRoleDefOf.ChanterAdvanced, out var reason, PsychicRitualRoleDef.Context.Dialog_BeginPsychicRitual, insertBefore))
		{
			return true;
		}
		return TryAssign(pawn, PsychicRitualRoleDefOf.Chanter, out reason, PsychicRitualRoleDef.Context.Dialog_BeginPsychicRitual, insertBefore);
	}

	public PsychicRitualRoleDef ForcedRole(Pawn pawn)
	{
		return forcedAssignments?.TryGetValue(pawn);
	}

	public bool IsChanter(Pawn pawn)
	{
		if (roleAssignments.ContainsKey(PsychicRitualRoleDefOf.ChanterAdvanced) && roleAssignments[PsychicRitualRoleDefOf.ChanterAdvanced].Contains(pawn))
		{
			return true;
		}
		if (roleAssignments.ContainsKey(PsychicRitualRoleDefOf.Chanter) && roleAssignments[PsychicRitualRoleDefOf.Chanter].Contains(pawn))
		{
			return true;
		}
		return false;
	}

	public IEnumerable<IGrouping<string, PsychicRitualRoleDef>> RoleGroups()
	{
		return ((IEnumerable<PsychicRitualRoleDef>)roleAssignments.Keys).GroupBy((Func<PsychicRitualRoleDef, string>)((PsychicRitualRoleDef role) => role.Label));
	}

	public IEnumerable<Pawn> AssignedPawns(PsychicRitualRoleDef role)
	{
		IEnumerable<Pawn> enumerable = roleAssignments.TryGetValue(role);
		return enumerable ?? Enumerable.Empty<Pawn>();
	}

	public PsychicRitualRoleDef RoleForPawn(Pawn pawn, bool includeForced = true)
	{
		if (pawnAssignments.TryGetValue(pawn, out var value))
		{
			return value;
		}
		return null;
	}

	public Pawn FirstAssignedPawn(PsychicRitualRoleDef role)
	{
		if (role == null)
		{
			Log.Error("Tried to get first assigned pawn for null role.");
			return null;
		}
		if (!roleAssignments.TryGetValue(role, out var value))
		{
			return null;
		}
		if (value.Count == 0)
		{
			return null;
		}
		return value[0];
	}

	public bool TryUnassignAnyRole(Pawn pawn)
	{
		if (!pawnAssignments.TryGetValue(pawn, out var value))
		{
			return false;
		}
		PsychicRitualRoleDef psychicRitualRoleDef = forcedAssignments?.TryGetValue(pawn);
		if (value == psychicRitualRoleDef)
		{
			return false;
		}
		pawnAssignments.Remove(pawn);
		roleAssignments[value].Remove(pawn);
		return true;
	}

	public void RemoveParticipant(Pawn pawn)
	{
		TryUnassignAnyRole(pawn);
	}

	public void RemoveAllParticipantsAndTargets()
	{
		pawnAssignments.Clear();
		forcedAssignments?.Clear();
		roleAssignments.ClearValueLists();
		spectators.Clear();
		target = TargetInfo.Invalid;
	}

	public void TryAssignAnyUnassigned(List<Pawn> pawns)
	{
		foreach (Pawn pawn in pawns)
		{
			if (PawnParticipating(pawn))
			{
				continue;
			}
			PsychicRitualRoleDef psychicRitualRoleDef = forcedAssignments?.TryGetValue(pawn);
			if (psychicRitualRoleDef != null && TryAssign(pawn, psychicRitualRoleDef, out var reason))
			{
				break;
			}
			foreach (PsychicRitualRoleDef key in roleAssignments.Keys)
			{
				if (TryAssign(pawn, key, out reason))
				{
					break;
				}
			}
		}
	}

	public bool TryAssign(Pawn pawn, PsychicRitualRoleDef role, out PsychicRitualRoleDef.Reason reason, PsychicRitualRoleDef.Context context = PsychicRitualRoleDef.Context.Dialog_BeginPsychicRitual, Pawn insertBefore = null)
	{
		reason = PsychicRitualRoleDef.Reason.None;
		if (role == null)
		{
			return false;
		}
		if (pawn == null)
		{
			return false;
		}
		PsychicRitualRoleDef psychicRitualRoleDef = forcedAssignments?.TryGetValue(pawn);
		if (psychicRitualRoleDef != null && role != psychicRitualRoleDef)
		{
			return false;
		}
		if (!roleAssignments.TryGetValue(role, out var value))
		{
			return false;
		}
		if (!role.PawnCanDo(context, pawn, target, out reason))
		{
			return false;
		}
		if (value.Count >= role.MaxCount)
		{
			return false;
		}
		TryUnassignAnyRole(pawn);
		if (insertBefore != null)
		{
			value.Insert(value.IndexOf(insertBefore), pawn);
		}
		else
		{
			value.Add(pawn);
		}
		pawnAssignments[pawn] = role;
		return true;
	}

	public void SetTarget(TargetInfo target)
	{
		this.target = target;
	}

	public IEnumerable<string> BlockingIssues()
	{
		foreach (var (role, pawns) in roleAssignments)
		{
			if (role.MinCount > 0 && pawns.Count < role.MinCount)
			{
				if (role.MinCount == 1)
				{
					yield return "MessageLordJobNeedsAtLeastOneRolePawn".Translate(role.label.CapitalizeFirst());
				}
				else
				{
					yield return "MessageLordJobNeedsAtLeastNumRolePawn".Translate(role.LabelCap, role.MinCount);
				}
			}
			foreach (Pawn pawn in pawns)
			{
				foreach (string item in role.BlockingIssues(pawn, Target))
				{
					yield return item;
				}
				PsychicRitualRoleDef psychicRitualRoleDef2 = forcedAssignments?.TryGetValue(pawn);
				if (psychicRitualRoleDef2 != null && psychicRitualRoleDef2 != role)
				{
					yield return "RoleIsLocked".Translate(role.label);
				}
			}
		}
	}

	public void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			if (roleAssignments != null)
			{
				foreach (KeyValuePair<PsychicRitualRoleDef, List<Pawn>> roleAssignment in roleAssignments)
				{
					roleAssignment.Value?.RemoveAll((Pawn pawn) => pawn?.Discarded ?? true);
				}
			}
			pawnAssignments?.RemoveAll((KeyValuePair<Pawn, PsychicRitualRoleDef> kvp) => kvp.Key == null || kvp.Key.Discarded);
			forcedAssignments?.RemoveAll((KeyValuePair<Pawn, PsychicRitualRoleDef> kvp) => kvp.Key == null || kvp.Key.Discarded);
		}
		Scribe_Collections.Look(ref roleAssignments, "roleAssignments", LookMode.Def, LookMode.Reference, ref roleAssignmentsKeys, ref roleAssignmentsValues, logNullErrors: true, saveDestroyedKeys: false, saveDestroyedValues: true);
		Scribe_Collections.Look(ref pawnAssignments, "pawnAssignments", LookMode.Reference, LookMode.Def, ref pawnAssignmentsKeys, ref pawnAssignmentsValues, logNullErrors: false, saveDestroyedKeys: true);
		Scribe_Collections.Look(ref forcedAssignments, "forcedAssignments", LookMode.Reference, LookMode.Def, ref forcedAssignmentKeys, ref forcedAssignmentValues, logNullErrors: false, saveDestroyedKeys: true);
		Scribe_Collections.Look(ref spectators, "spectators", true, LookMode.Reference);
		Scribe_TargetInfo.Look(ref target, saveDestroyedThings: true, "target");
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		if (roleAssignments != null)
		{
			foreach (KeyValuePair<PsychicRitualRoleDef, List<Pawn>> roleAssignment2 in roleAssignments)
			{
				roleAssignment2.Value?.RemoveAll((Pawn pawn) => pawn == null);
			}
		}
		pawnAssignments?.RemoveAll((KeyValuePair<Pawn, PsychicRitualRoleDef> kvp) => kvp.Key == null);
		forcedAssignments?.RemoveAll((KeyValuePair<Pawn, PsychicRitualRoleDef> kvp) => kvp.Key == null);
	}
}
