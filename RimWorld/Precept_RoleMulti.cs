using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Precept_RoleMulti : Precept_Role
{
	public List<IdeoRoleInstance> chosenPawns = new List<IdeoRoleInstance>();

	private List<IdeoRoleInstance> chosenPawnsCache = new List<IdeoRoleInstance>();

	private List<IdeoRoleInstance> pawnsToRemove = new List<IdeoRoleInstance>();

	public override IEnumerable<Pawn> ChosenPawns()
	{
		foreach (IdeoRoleInstance chosenPawn in chosenPawns)
		{
			yield return chosenPawn.pawn;
		}
	}

	public override Pawn ChosenPawnSingle()
	{
		return null;
	}

	public override void Init(Ideo ideo, FactionDef generatingFor = null)
	{
		base.Init(ideo, generatingFor);
		active = true;
	}

	public override void Assign(Pawn p, bool addThoughts)
	{
		if (!IsAssigned(p))
		{
			IdeoRoleInstance ideoRoleInstance = chosenPawnsCache.FirstOrDefault((IdeoRoleInstance c) => c.pawn == p);
			if (ideoRoleInstance != null)
			{
				chosenPawnsCache.Remove(ideoRoleInstance);
				chosenPawns.Add(ideoRoleInstance);
			}
			else
			{
				chosenPawns.Add(new IdeoRoleInstance(this)
				{
					pawn = p
				});
				FillOrUpdateAbilities();
			}
			Notify_PawnAssigned(p);
		}
	}

	public override void FillOrUpdateAbilities()
	{
		foreach (IdeoRoleInstance chosenPawn in chosenPawns)
		{
			chosenPawn.abilities = FillOrUpdateAbilityList(chosenPawn.pawn, chosenPawn.abilities);
		}
	}

	public override List<Ability> AbilitiesFor(Pawn p)
	{
		for (int i = 0; i < chosenPawns.Count; i++)
		{
			if (chosenPawns[i].pawn == p)
			{
				return chosenPawns[i].abilities;
			}
		}
		return null;
	}

	public override bool IsAssigned(Pawn p)
	{
		for (int i = 0; i < chosenPawns.Count; i++)
		{
			if (chosenPawns[i].pawn == p)
			{
				return true;
			}
		}
		return false;
	}

	public override void Unassign(Pawn p, bool generateThoughts)
	{
		if (IsAssigned(p))
		{
			IdeoRoleInstance ideoRoleInstance = chosenPawns.FirstOrDefault((IdeoRoleInstance c) => c.pawn == p);
			if (ideoRoleInstance != null)
			{
				chosenPawns.Remove(ideoRoleInstance);
				chosenPawnsCache.Add(ideoRoleInstance);
				p.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtMaker.MakeThought(ThoughtDefOf.IdeoRoleLost, this));
				Notify_PawnUnassigned(p);
			}
		}
	}

	public override void RecacheActivity()
	{
		pawnsToRemove.Clear();
		foreach (IdeoRoleInstance chosenPawn in chosenPawns)
		{
			if (chosenPawn.pawn == null || !ValidatePawn(chosenPawn.pawn))
			{
				pawnsToRemove.Add(chosenPawn);
			}
		}
		foreach (IdeoRoleInstance item in pawnsToRemove)
		{
			Unassign(item.pawn, generateThoughts: false);
			if (chosenPawns.Contains(item))
			{
				chosenPawns.Remove(item);
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (!GameDataSaveLoader.IsSavingOrLoadingExternalIdeo)
		{
			Scribe_Collections.Look(ref chosenPawns, "chosenPawns", LookMode.Deep, this);
			Scribe_Collections.Look(ref chosenPawnsCache, "chosenPawnsCache", LookMode.Deep, this);
		}
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		chosenPawns.RemoveAll((IdeoRoleInstance c) => c.pawn == null || !ValidatePawn(c.pawn));
		chosenPawnsCache.RemoveAll((IdeoRoleInstance c) => c.pawn == null);
		foreach (IdeoRoleInstance chosenPawn in chosenPawns)
		{
			chosenPawn.sourceRole = this;
		}
	}

	public override void CopyTo(Precept precept)
	{
		base.CopyTo(precept);
		Precept_RoleMulti obj = (Precept_RoleMulti)precept;
		obj.chosenPawns.Clear();
		obj.chosenPawns.AddRange(chosenPawns);
		obj.chosenPawnsCache.Clear();
		obj.chosenPawnsCache.AddRange(chosenPawnsCache);
	}
}
