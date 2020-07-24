using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Pawn_AbilityTracker : IExposable
	{
		public Pawn pawn;

		public List<Ability> abilities = new List<Ability>();

		public Pawn_AbilityTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void AbilitiesTick()
		{
			for (int i = 0; i < abilities.Count; i++)
			{
				abilities[i].AbilityTick();
			}
		}

		public void GainAbility(AbilityDef def)
		{
			if (!abilities.Any((Ability a) => a.def == def))
			{
				abilities.Add(Activator.CreateInstance(def.abilityClass, pawn, def) as Ability);
			}
		}

		public void RemoveAbility(AbilityDef def)
		{
			Ability ability = abilities.FirstOrDefault((Ability x) => x.def == def);
			if (ability != null)
			{
				abilities.Remove(ability);
			}
		}

		public Ability GetAbility(AbilityDef def)
		{
			for (int i = 0; i < abilities.Count; i++)
			{
				if (abilities[i].def == def)
				{
					return abilities[i];
				}
			}
			return null;
		}

		public IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Ability item in from a in abilities
				orderby a.def.level, a.def.EntropyGain
				select a)
			{
				if (!pawn.Drafted && !item.def.displayGizmoWhileUndrafted)
				{
					continue;
				}
				foreach (Command gizmo in item.GetGizmos())
				{
					yield return gizmo;
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref abilities, "abilities", LookMode.Deep, pawn);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				abilities.RemoveAll((Ability a) => a.def == null);
			}
		}
	}
}
