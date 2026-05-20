using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class Pawn_AbilityTracker : IExposable
{
	public Pawn pawn;

	public List<Ability> abilities = new List<Ability>();

	private bool allAbilitiesCachedDirty = true;

	private List<Ability> allAbilitiesCached = new List<Ability>();

	private readonly List<Ability> tmpAbilities = new List<Ability>();

	public List<Ability> AllAbilitiesForReading
	{
		get
		{
			if (allAbilitiesCachedDirty)
			{
				allAbilitiesCached.Clear();
				allAbilitiesCached.AddRange(abilities);
				if (pawn.health != null)
				{
					foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
					{
						if (!hediff.def.abilities.NullOrEmpty())
						{
							allAbilitiesCached.AddRange(hediff.AllAbilitiesForReading);
						}
					}
				}
				if (pawn.equipment != null)
				{
					CompEquippableAbility compEquippableAbility = pawn.equipment.Primary?.TryGetComp<CompEquippableAbility>();
					if (compEquippableAbility != null && compEquippableAbility.AbilityForReading != null)
					{
						allAbilitiesCached.Add(compEquippableAbility.AbilityForReading);
					}
				}
				if (pawn.apparel != null)
				{
					foreach (Apparel item in pawn.apparel.WornApparel)
					{
						allAbilitiesCached.AddRange(item.AllAbilitiesForReading);
					}
				}
				if (ModsConfig.AnomalyActive && pawn.IsMutant)
				{
					allAbilitiesCached.AddRange(pawn.mutant.AllAbilitiesForReading);
				}
				if (pawn.royalty != null)
				{
					allAbilitiesCached.AddRange(pawn.royalty.AllAbilitiesForReading);
				}
				if (ModsConfig.IdeologyActive)
				{
					Precept_Role precept_Role = pawn.Ideo?.GetRole(pawn);
					if (precept_Role != null && precept_Role.Active && !precept_Role.AbilitiesFor(pawn).NullOrEmpty())
					{
						foreach (Ability item2 in precept_Role.AbilitiesFor(pawn))
						{
							bool flag = false;
							if (!item2.def.requiredMemes.NullOrEmpty())
							{
								foreach (MemeDef requiredMeme in item2.def.requiredMemes)
								{
									if (!pawn.Ideo.memes.Contains(requiredMeme))
									{
										flag = true;
										break;
									}
								}
							}
							if (!flag)
							{
								allAbilitiesCached.Add(item2);
							}
						}
					}
				}
				if (ModsConfig.AnomalyActive && pawn.IsMutant && pawn.mutant.Def.abilityWhitelist.Any())
				{
					allAbilitiesCached = allAbilitiesCached.Where((Ability a) => pawn.mutant.Def.abilityWhitelist.Contains(a.def)).ToList();
				}
				allAbilitiesCached.SortBy((Ability a) => a.def.category?.displayOrder ?? 0, (Ability a) => a.def.displayOrder, (Ability a) => a.def.level);
				allAbilitiesCachedDirty = false;
			}
			return allAbilitiesCached;
		}
	}

	public Pawn_AbilityTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void AbilitiesTick()
	{
		for (int i = 0; i < AllAbilitiesForReading.Count; i++)
		{
			AllAbilitiesForReading[i].AbilityTick();
		}
	}

	public void GainAbility(AbilityDef def)
	{
		if (!abilities.Any((Ability a) => a.def == def))
		{
			abilities.Add(AbilityUtility.MakeAbility(def, pawn));
		}
		Notify_TemporaryAbilitiesChanged();
	}

	public void RemoveAbility(AbilityDef def)
	{
		Ability ability = abilities.FirstOrDefault((Ability x) => x.def == def);
		if (ability != null)
		{
			abilities.Remove(ability);
		}
		Notify_TemporaryAbilitiesChanged();
	}

	public Ability GetAbility(AbilityDef def, bool includeTemporary = false)
	{
		List<Ability> list = (includeTemporary ? AllAbilitiesForReading : abilities);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def == def)
			{
				return list[i];
			}
		}
		return null;
	}

	public List<Ability> AICastableAbilities(LocalTargetInfo target, bool offensive)
	{
		tmpAbilities.Clear();
		foreach (Ability item in AllAbilitiesForReading)
		{
			if (item.def.ai_IsOffensive == offensive && (item.AICanTargetNow(target) || item.AICanTargetNow(pawn)))
			{
				tmpAbilities.Add(item);
			}
		}
		return tmpAbilities;
	}

	public IEnumerable<Gizmo> GetGizmos()
	{
		bool visiblePrimary = pawn.IsColonistPlayerControlled || pawn.IsColonyMechPlayerControlled || pawn.IsColonySubhumanPlayerControlled || pawn.IsColonyAnimal;
		foreach (Ability a in AllAbilitiesForReading)
		{
			if (visiblePrimary || DebugSettings.ShowDevGizmos)
			{
				bool visibleSecondary = (pawn.Drafted || a.def.displayGizmoWhileUndrafted) && a.GizmosVisible();
				if (visibleSecondary || (!pawn.IsColonistPlayerControlled && !pawn.IsColonyMechPlayerControlled && DebugSettings.ShowDevGizmos))
				{
					foreach (Command gizmo in a.GetGizmos())
					{
						if (gizmo is Command_Ability command_Ability)
						{
							command_Ability.devGizmo = !visiblePrimary && !visibleSecondary && DebugSettings.ShowDevGizmos;
						}
						yield return gizmo;
					}
				}
			}
			foreach (Gizmo item in a.GetGizmosExtra())
			{
				yield return item;
			}
		}
	}

	public void Notify_TemporaryAbilitiesChanged()
	{
		allAbilitiesCachedDirty = true;
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref abilities, "abilities", LookMode.Deep, pawn);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			abilities.RemoveAll((Ability a) => a.def == null || a.def == AbilityDefOf.Speech);
		}
	}
}
