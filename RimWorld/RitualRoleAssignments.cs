using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class RitualRoleAssignments : ILordJobAssignmentsManager<RitualRole>, ILordJobCandidatePool, IExposable
{
	private List<Pawn> allPawns;

	private List<Pawn> nonAssignablePawns;

	private List<RitualRole> allRoles;

	private Dictionary<string, Pawn> forcedRoles;

	private Dictionary<string, List<Pawn>> assignedRoles = new Dictionary<string, List<Pawn>>();

	private Precept_Role roleChangeSelection;

	private List<Pawn> spectators = new List<Pawn>();

	private List<Pawn> requiredPawns;

	private Precept_Ritual ritual;

	private TargetInfo ritualTarget;

	private Pawn selectedPawn;

	private List<Pawn> tmpParticipants = new List<Pawn>();

	private List<Pawn> tmpForcedRolePawns;

	private List<string> tmpForcedRoleIds;

	private List<string> tmpAssignedRoleIds;

	private List<List<Pawn>> tmpAssignedRolePawns;

	private static List<Pawn> tmpOrderedPawns = new List<Pawn>(32);

	public List<Pawn> SpectatorsForReading => spectators;

	public Precept_Ritual Ritual => ritual;

	public bool SpectatorsAllowed => true;

	public List<Pawn> Participants
	{
		get
		{
			tmpParticipants.Clear();
			foreach (Pawn allPawn in allPawns)
			{
				if (PawnParticipating(allPawn))
				{
					tmpParticipants.Add(allPawn);
				}
			}
			return tmpParticipants;
		}
	}

	public List<Pawn> AllCandidatePawns => allPawns;

	public List<Pawn> NonAssignablePawns => nonAssignablePawns;

	public List<RitualRole> AllRolesForReading => allRoles;

	public List<Pawn> ExtraRequiredPawnsForReading => requiredPawns;

	public Dictionary<string, Pawn> ForcedRolesForReading => forcedRoles;

	public Precept_Role RoleChangeSelection => roleChangeSelection;

	public void ExposeData()
	{
		Scribe_Collections.Look(ref allPawns, "allPawns", true, LookMode.Reference);
		Scribe_Collections.Look(ref nonAssignablePawns, "nonAssignablePawns", true, LookMode.Reference);
		Scribe_Collections.Look(ref spectators, "spectators", true, LookMode.Reference);
		Scribe_Collections.Look(ref requiredPawns, "requiredPawns", true, LookMode.Reference);
		Scribe_Collections.Look(ref forcedRoles, "forcedRoles", LookMode.Value, LookMode.Reference, ref tmpForcedRoleIds, ref tmpForcedRolePawns, logNullErrors: true, saveDestroyedKeys: false, saveDestroyedValues: true);
		Scribe_Collections.Look(ref assignedRoles, "assignedRoles", LookMode.Value, LookMode.Reference, ref tmpAssignedRoleIds, ref tmpAssignedRolePawns, logNullErrors: true, saveDestroyedKeys: false, saveDestroyedValues: true);
		Scribe_References.Look(ref ritual, "ritual");
		Scribe_TargetInfo.Look(ref ritualTarget, saveDestroyedThings: true, "ritualTarget");
		Scribe_References.Look(ref selectedPawn, "selectedPawn", saveDestroyedThings: true);
		Scribe_References.Look(ref roleChangeSelection, "roleChangeSelection");
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			allRoles = ((ritual != null) ? new List<RitualRole>(ritual.behavior.def.roles) : new List<RitualRole>());
		}
	}

	protected RitualRoleAssignments()
	{
	}

	public RitualRoleAssignments(Precept_Ritual ritual, TargetInfo ritualTarget)
	{
		this.ritual = ritual;
		this.ritualTarget = ritualTarget;
	}

	public void Setup(List<Pawn> allPawns, List<Pawn> nonAssignablePawns, Dictionary<string, Pawn> forcedRoles = null, List<Pawn> requiredPawns = null, Pawn selectedPawn = null)
	{
		this.allPawns = allPawns;
		this.nonAssignablePawns = nonAssignablePawns;
		this.forcedRoles = forcedRoles;
		allRoles = ((ritual != null) ? new List<RitualRole>(ritual.behavior.def.roles) : new List<RitualRole>());
		this.selectedPawn = selectedPawn;
		this.requiredPawns = new List<Pawn>();
		if (forcedRoles != null)
		{
			this.requiredPawns.AddRange(forcedRoles.Values);
		}
		if (requiredPawns != null)
		{
			this.requiredPawns.AddRange(requiredPawns);
		}
		allPawns.SortBy((Pawn p) => p.Faction == null || !p.Faction.IsPlayer, (Pawn p) => !Faction.OfPlayer.ideos.Has(p.Ideo), (Pawn p) => !p.IsFreeNonSlaveColonist);
	}

	public bool Forced(Pawn pawn)
	{
		if (forcedRoles != null)
		{
			return forcedRoles.ContainsValue(pawn);
		}
		return false;
	}

	public RitualRole ForcedRole(Pawn pawn)
	{
		if (forcedRoles == null)
		{
			return null;
		}
		string text = null;
		foreach (KeyValuePair<string, Pawn> forcedRole in forcedRoles)
		{
			if (forcedRole.Value == pawn)
			{
				text = forcedRole.Key;
			}
		}
		if (text == null)
		{
			return null;
		}
		foreach (RitualRole allRole in allRoles)
		{
			if (allRole.id == text)
			{
				return allRole;
			}
		}
		return null;
	}

	public void RemoveParticipant(Pawn pawn)
	{
		TryUnassignAnyRole(pawn);
		spectators.Remove(pawn);
		allPawns.Remove(pawn);
		allPawns.Add(pawn);
	}

	public bool TryUnassignAnyRole(Pawn pawn)
	{
		foreach (KeyValuePair<string, List<Pawn>> assignedRole in assignedRoles)
		{
			if (assignedRole.Value.Remove(pawn))
			{
				if (CanEverSpectate(pawn))
				{
					spectators.Add(pawn);
				}
				return true;
			}
		}
		return false;
	}

	public bool TryAssign(Pawn pawn, RitualRole role, out PsychicRitualRoleDef.Reason reason, PsychicRitualRoleDef.Context context = PsychicRitualRoleDef.Context.Dialog_BeginPsychicRitual, Pawn insertBefore = null)
	{
		reason = PsychicRitualRoleDef.Reason.None;
		if (forcedRoles != null && forcedRoles.ContainsValue(pawn))
		{
			return false;
		}
		if (PawnNotAssignableReason(pawn, role, out var _) != null)
		{
			return false;
		}
		if (role == null)
		{
			return TryAssignSpectate(pawn);
		}
		if (role.AppliesToPawn(pawn, out var _, ritualTarget, null, this, null, skipReason: true) && (role.maxCount <= 0 || AssignedPawns(role).Count() < role.maxCount))
		{
			TryUnassignAnyRole(pawn);
			spectators.Remove(pawn);
			if (!assignedRoles.TryGetValue(role.id, out var value))
			{
				value = new List<Pawn>();
				assignedRoles.Add(role.id, value);
			}
			if (insertBefore != null && value.Contains(insertBefore))
			{
				value.Insert(value.IndexOf(insertBefore), pawn);
			}
			else
			{
				value.Add(pawn);
			}
			if (role is RitualRoleIdeoRoleChanger)
			{
				UpdateRoleChangeTargetRole(pawn);
			}
			return true;
		}
		return false;
	}

	public bool TryAssignSpectate(Pawn pawn, Pawn insertBefore = null)
	{
		if (spectators.Contains(pawn) || !CanEverSpectate(pawn) || PawnNotAssignableReason(pawn, null, out var _) != null)
		{
			return false;
		}
		TryUnassignAnyRole(pawn);
		if (!spectators.Contains(pawn))
		{
			if (insertBefore != null && spectators.Contains(insertBefore))
			{
				spectators.Insert(spectators.IndexOf(insertBefore), pawn);
			}
			else
			{
				spectators.Add(pawn);
			}
		}
		return RoleForPawn(pawn) == null;
	}

	public RitualRole GetRole(string roleId)
	{
		if (!AllRolesForReading.NullOrEmpty())
		{
			foreach (RitualRole item in AllRolesForReading)
			{
				if (item.id == roleId)
				{
					return item;
				}
			}
		}
		return null;
	}

	public bool CanParticipate(Pawn pawn, out TaggedString reason)
	{
		if (forcedRoles != null && forcedRoles.ContainsValue(pawn))
		{
			reason = TaggedString.Empty;
			return true;
		}
		if (Required(pawn))
		{
			reason = TaggedString.Empty;
			return true;
		}
		if (pawn == selectedPawn)
		{
			reason = TaggedString.Empty;
			return true;
		}
		foreach (RitualRole item in AllRolesForReading)
		{
			if (PawnNotAssignableReason(pawn, item, out var _) == null)
			{
				reason = TaggedString.Empty;
				return true;
			}
		}
		if (CanEverSpectate(pawn))
		{
			reason = TaggedString.Empty;
			return true;
		}
		reason = PawnNotAssignableReason(pawn, null);
		return false;
	}

	public Pawn FirstAssignedPawn(RitualRole role)
	{
		return FirstAssignedPawn(role.id);
	}

	public Pawn FirstAssignedPawn(string roleId)
	{
		if (forcedRoles != null && forcedRoles.TryGetValue(roleId, out var value))
		{
			return value;
		}
		if (assignedRoles.TryGetValue(roleId, out var value2) && value2.Count > 0)
		{
			return value2[0];
		}
		return null;
	}

	public bool CanEverSpectate(Pawn pawn)
	{
		if (ritual != null && ritual.ritualOnlyForIdeoMembers && pawn.Ideo != ritual.ideo && !ritual.def.allowSpectatorsFromOtherIdeos)
		{
			return false;
		}
		if (ritual != null && ritual.behavior.def.spectatorFilter != null && !ritual.behavior.def.spectatorFilter.Allowed(pawn))
		{
			return false;
		}
		if (ritual != null && !ritual.behavior.PawnCanFillRole(pawn, null, out var _, ritualTarget))
		{
			return false;
		}
		if (pawn.RaceProps.Humanlike && !pawn.IsPrisoner)
		{
			return GatheringsUtility.ShouldPawnKeepAttendingRitual(pawn, ritual, ritual?.behavior.def.spectatorsIgnoreBleeding ?? false);
		}
		return false;
	}

	public IEnumerable<Pawn> SpectatorCandidates()
	{
		foreach (Pawn allPawn in allPawns)
		{
			if (CanEverSpectate(allPawn) && RoleForPawn(allPawn) == null)
			{
				Precept_Ritual precept_Ritual = ritual;
				if (precept_Ritual == null || precept_Ritual.behavior?.ShouldInitAsSpectator(allPawn, this) != false)
				{
					yield return allPawn;
				}
			}
		}
	}

	public IEnumerable<Pawn> CandidatesForRole(string roleId, TargetInfo ritualTarget, bool includeAssigned = false, bool includeAssignedForSameRole = false, bool includeForced = true)
	{
		return CandidatesForRole(GetRole(roleId), ritualTarget, includeAssigned, includeAssignedForSameRole, includeForced);
	}

	public IEnumerable<Pawn> CandidatesForRole(RitualRole role, TargetInfo ritualTarget, bool includeAssigned = false, bool includeAssignedForSameRole = false, bool includeForced = true)
	{
		if (forcedRoles != null && forcedRoles.TryGetValue(role.id, out var value))
		{
			yield return value;
			yield break;
		}
		foreach (Pawn allPawn in allPawns)
		{
			if (role.AppliesToPawn(allPawn, out var _, ritualTarget, null, this, null, skipReason: true) && ShouldIncludePawn(allPawn))
			{
				yield return allPawn;
			}
		}
		bool ShouldIncludePawn(Pawn pawn)
		{
			if (includeAssigned || (includeAssignedForSameRole && RoleForPawn(pawn) == role) || RoleForPawn(pawn) == null)
			{
				return GatheringsUtility.ShouldPawnKeepAttendingRitual(pawn, ritual, role != null && role.ignoreBleeding);
			}
			return false;
		}
	}

	public IEnumerable<Pawn> AssignedPawns(RitualRole role)
	{
		if (forcedRoles != null)
		{
			foreach (KeyValuePair<string, Pawn> forcedRole in forcedRoles)
			{
				if (forcedRole.Key == role.id)
				{
					yield return forcedRole.Value;
				}
			}
		}
		if (!assignedRoles.TryGetValue(role.id, out var value))
		{
			yield break;
		}
		foreach (Pawn item in value)
		{
			yield return item;
		}
	}

	public bool AnyPawnAssigned(string roleId)
	{
		if (forcedRoles == null || !forcedRoles.ContainsKey(roleId))
		{
			if (assignedRoles.ContainsKey(roleId))
			{
				return assignedRoles[roleId].Any();
			}
			return false;
		}
		return true;
	}

	public bool AnyPawnAssigned(RitualRole role)
	{
		return AnyPawnAssigned(role.id);
	}

	public IEnumerable<Pawn> AssignedPawns(string roleId)
	{
		return AssignedPawns(GetRole(roleId));
	}

	public RitualRole RoleForPawn(Pawn pawn, bool includeForced = true)
	{
		if (spectators.Contains(pawn))
		{
			return null;
		}
		if (includeForced && forcedRoles != null)
		{
			foreach (KeyValuePair<string, Pawn> forcedRole in forcedRoles)
			{
				if (forcedRole.Value == pawn)
				{
					return GetRole(forcedRole.Key);
				}
			}
		}
		foreach (KeyValuePair<string, List<Pawn>> assignedRole in assignedRoles)
		{
			if (!assignedRole.Value.NullOrEmpty() && assignedRole.Value.Contains(pawn))
			{
				return GetRole(assignedRole.Key);
			}
		}
		return null;
	}

	public bool PawnParticipating(Pawn pawn)
	{
		if (RoleForPawn(pawn) == null)
		{
			return PawnSpectating(pawn);
		}
		return true;
	}

	public bool PawnSpectating(Pawn pawn)
	{
		return spectators.Contains(pawn);
	}

	public void FillPawns(Dialog_BeginRitual.PawnFilter filter, TargetInfo ritualTarget)
	{
		if (!requiredPawns.NullOrEmpty())
		{
			foreach (Pawn requiredPawn in requiredPawns)
			{
				if (forcedRoles == null || !forcedRoles.ContainsValue(requiredPawn))
				{
					TryAssignSpectate(requiredPawn);
				}
			}
		}
		string reason;
		PsychicRitualRoleDef.Reason reason2;
		if (selectedPawn != null && RoleForPawn(selectedPawn) == null)
		{
			foreach (RitualRole item in AllRolesForReading)
			{
				if (item.defaultForSelectedColonist && item.AppliesToPawn(selectedPawn, out reason, ritualTarget, null, this))
				{
					TryAssign(selectedPawn, item, out reason2);
					break;
				}
			}
		}
		foreach (RitualRole role in AllRolesForReading)
		{
			tmpOrderedPawns.Clear();
			tmpOrderedPawns.AddRange(allPawns.Where((Pawn pawn) => filter == null || filter(pawn, !(role is RitualRoleForced), role.allowOtherIdeos)));
			role.OrderByDesirability(tmpOrderedPawns);
			foreach (Pawn tmpOrderedPawn in tmpOrderedPawns)
			{
				if (RoleForPawn(tmpOrderedPawn) == null)
				{
					if (role.maxCount > 0 && AssignedPawns(role).Count() >= role.maxCount)
					{
						break;
					}
					if (role.AppliesToPawn(tmpOrderedPawn, out reason, ritualTarget, null, this, null, skipReason: true))
					{
						TryAssign(tmpOrderedPawn, role, out reason2);
					}
				}
			}
		}
		foreach (Pawn item2 in SpectatorCandidates())
		{
			TryAssignSpectate(item2);
		}
		List<Pawn> pawnsToRemove = new List<Pawn>();
		foreach (Pawn allPawn in allPawns)
		{
			RitualRole ritualRole = RoleForPawn(allPawn);
			if (ritualRole != null && ritualRole.required && ritualRole.substitutable && PawnNotAssignableReason(allPawn, ritualRole, out var _) != null)
			{
				RemoveParticipant(allPawn);
				pawnsToRemove.Add(allPawn);
			}
		}
		allPawns.RemoveAll((Pawn p) => pawnsToRemove.Contains(p));
	}

	public bool Required(Pawn pawn)
	{
		return requiredPawns.NotNullAndContains(pawn);
	}

	public bool RoleSubstituted(string roleId)
	{
		if (ritual.behavior.def.roles.NullOrEmpty())
		{
			return false;
		}
		RitualRole role = ritual.behavior.def.roles.FirstOrDefault((RitualRole r) => r.id == roleId);
		if (role == null)
		{
			return false;
		}
		if (!role.substitutable || role.precept == null)
		{
			return false;
		}
		Precept precept = ritual.ideo.PreceptsListForReading.FirstOrDefault((Precept p) => p.def == role.precept);
		if (precept == null)
		{
			return false;
		}
		bool result = false;
		foreach (Pawn item in AssignedPawns(roleId))
		{
			if (item.Ideo?.GetRole(item) != precept)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public string PawnNotAssignableReason(Pawn p, RitualRole role)
	{
		bool stillAddToPawnList;
		return PawnNotAssignableReason(p, role, out stillAddToPawnList);
	}

	public string PawnNotAssignableReason(Pawn p, RitualRole role, out bool stillAddToPawnList)
	{
		return PawnNotAssignableReason(p, role, ritual, this, ritualTarget, out stillAddToPawnList);
	}

	public static string PawnNotAssignableReason(Pawn p, RitualRole role, Precept_Ritual ritual, RitualRoleAssignments assignments, TargetInfo ritualTarget, out bool stillAddToPawnList)
	{
		stillAddToPawnList = false;
		if (p.Downed && (role == null || !role.allowDowned))
		{
			return "MessageRitualPawnDowned".Translate(p);
		}
		bool flag = role?.ignoreBleeding ?? ritual?.behavior.def.spectatorsIgnoreBleeding ?? false;
		if (p.health.hediffSet.BleedRateTotal > 0f && !flag)
		{
			return "MessageRitualPawnInjured".Translate(p);
		}
		if (p.InMentalState && (role == null || !role.allowNonAggroMentalState || p.InAggroMentalState))
		{
			return "MessageRitualPawnMentalState".Translate(p);
		}
		if (p.IsPrisoner && role == null)
		{
			return "MessageRitualRoleMustNotBePrisonerToSpectate".Translate(ritual?.behavior?.def.spectatorGerund ?? ((string)"Spectate".Translate())).CapitalizeFirst();
		}
		if (ModsConfig.BiotechActive && role != null && !role.allowBaby && (p.DevelopmentalStage == DevelopmentalStage.Baby || p.DevelopmentalStage == DevelopmentalStage.Newborn))
		{
			return "MessageRitualRoleCannotBeBaby".Translate(role.LabelCap);
		}
		if (ModsConfig.AnomalyActive && p.IsSubhuman)
		{
			return "MessageRitualCannotBeMutant".Translate();
		}
		LordJob lordJob = p.GetLord()?.LordJob;
		if (lordJob != null && !(lordJob is LordJob_VoluntarilyJoinable) && assignments != null && assignments.requiredPawns?.Contains(p) == false)
		{
			stillAddToPawnList = true;
			return "MessageRitualRoleBusy".Translate(p);
		}
		if (p.IsPrisoner)
		{
			if (p.guest.Released)
			{
				stillAddToPawnList = true;
				return "MessageRitualPawnReleased".Translate(p);
			}
			if (!p.guest.PrisonerIsSecure)
			{
				stillAddToPawnList = true;
				return "MessageRitualPawnPrisonerNotSecured".Translate(p);
			}
		}
		else if (ritualTarget.IsValid && (ritual == null || !ritual.ignoreExtremeTemperatures) && !p.SafeTemperatureRange().IncludesEpsilon(ritualTarget.Cell.GetTemperature(ritualTarget.Map)))
		{
			return "MessageRitualWontAttendExtremeTemperature".Translate(p);
		}
		if (p.IsSlave)
		{
			if (p.guest.Released)
			{
				stillAddToPawnList = true;
				return "MessageRitualPawnReleased".Translate(p);
			}
			if (p.InMentalState && p.MentalStateDef != MentalStateDefOf.Rebellion)
			{
				stillAddToPawnList = true;
				return "MessageRitualPawnMentalState".Translate(p);
			}
			if (!p.guest.SlaveIsSecure)
			{
				stillAddToPawnList = true;
				return "MessageRitualPawnSlaveNotSecured".Translate(p);
			}
		}
		if (role == null && !p.RaceProps.Humanlike)
		{
			return "MessageRitualRoleMustBeHumanlike".Translate("Spectators".Translate());
		}
		if (role == null && ritual != null && !ritual.def.allowSpectatorsFromOtherIdeos && ritual.ritualOnlyForIdeoMembers && p.Ideo != ritual.ideo)
		{
			return "MessageRitualRoleMustHaveIdeoToSpectate".Translate(ritual.ideo.MemberNamePlural, ritual?.behavior?.def.spectatorGerund ?? ((string)"Spectate".Translate()));
		}
		if (ritual != null && !ritual.behavior.PawnCanFillRole(p, role, out var s, ritualTarget))
		{
			return s;
		}
		if (role != null && !role.AppliesToPawn(p, out s, ritualTarget, null, assignments))
		{
			return s;
		}
		return null;
	}

	public void SetRoleChangeSelection(Precept_Role role)
	{
		roleChangeSelection = role;
	}

	public void UpdateRoleChangeTargetRole(Pawn pawn)
	{
		Precept_Role precept_Role = null;
		if (pawn.Ideo?.GetRole(pawn) == null)
		{
			precept_Role = RitualUtility.AllRolesForPawn(pawn).FirstOrDefault((Precept_Role r) => r.Active && r.RequirementsMet(pawn));
		}
		SetRoleChangeSelection(precept_Role);
	}

	public IEnumerable<IGrouping<string, RitualRole>> RoleGroups()
	{
		return from r in AllRolesForReading
			group r by r.mergeId ?? r.id;
	}
}
