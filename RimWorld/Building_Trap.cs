using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Building_Trap : Building
	{
		private bool autoRearm;

		private List<Pawn> touchingPawns = new List<Pawn>();

		private const float KnowerSpringChanceFactorSameFaction = 0.005f;

		private const float KnowerSpringChanceFactorWildAnimal = 0.2f;

		private const float KnowerSpringChanceFactorFactionlessHuman = 0.3f;

		private const float KnowerSpringChanceFactorOther = 0f;

		private const ushort KnowerPathFindCost = 800;

		private const ushort KnowerPathWalkCost = 40;

		private bool CanSetAutoRearm
		{
			get
			{
				if (base.Faction == Faction.OfPlayer && def.blueprintDef != null)
				{
					return def.IsResearchFinished;
				}
				return false;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref autoRearm, "autoRearm", defaultValue: false);
			Scribe_Collections.Look(ref touchingPawns, "testees", LookMode.Reference);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				autoRearm = (CanSetAutoRearm && map.areaManager.Home[base.Position]);
			}
		}

		public override void Tick()
		{
			if (base.Spawned)
			{
				List<Thing> thingList = base.Position.GetThingList(base.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Pawn pawn = thingList[i] as Pawn;
					if (pawn != null && !touchingPawns.Contains(pawn))
					{
						touchingPawns.Add(pawn);
						CheckSpring(pawn);
					}
				}
				for (int j = 0; j < touchingPawns.Count; j++)
				{
					Pawn pawn2 = touchingPawns[j];
					if (!pawn2.Spawned || pawn2.Position != base.Position)
					{
						touchingPawns.Remove(pawn2);
					}
				}
			}
			base.Tick();
		}

		private void CheckSpring(Pawn p)
		{
			if (Rand.Chance(SpringChance(p)))
			{
				Map map = base.Map;
				Spring(p);
				if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
				{
					Find.LetterStack.ReceiveLetter("LetterFriendlyTrapSprungLabel".Translate(p.LabelShort, p).CapitalizeFirst(), "LetterFriendlyTrapSprung".Translate(p.LabelShort, p).CapitalizeFirst(), LetterDefOf.NegativeEvent, new TargetInfo(base.Position, map));
				}
			}
		}

		protected virtual float SpringChance(Pawn p)
		{
			float num = 1f;
			if (KnowsOfTrap(p))
			{
				if (p.Faction != null)
				{
					num = ((p.Faction != base.Faction) ? 0f : 0.005f);
				}
				else if (p.RaceProps.Animal)
				{
					num = 0.2f;
					num *= def.building.trapPeacefulWildAnimalsSpringChanceFactor;
				}
				else
				{
					num = 0.3f;
				}
			}
			num *= this.GetStatValue(StatDefOf.TrapSpringChance) * p.GetStatValue(StatDefOf.PawnTrapSpringChance);
			return Mathf.Clamp01(num);
		}

		public bool KnowsOfTrap(Pawn p)
		{
			if (p.Faction != null && !p.Faction.HostileTo(base.Faction))
			{
				return true;
			}
			if (p.Faction == null && p.RaceProps.Animal && !p.InAggroMentalState)
			{
				return true;
			}
			if (p.guest != null && p.guest.Released)
			{
				return true;
			}
			if (!p.IsPrisoner && base.Faction != null && p.HostFaction == base.Faction)
			{
				return true;
			}
			if (p.RaceProps.Humanlike && p.IsFormingCaravan())
			{
				return true;
			}
			if (p.IsPrisoner && p.guest.ShouldWaitInsteadOfEscaping && base.Faction == p.HostFaction)
			{
				return true;
			}
			if (p.Faction == null && p.RaceProps.Humanlike)
			{
				return true;
			}
			return false;
		}

		public override ushort PathFindCostFor(Pawn p)
		{
			if (!KnowsOfTrap(p))
			{
				return 0;
			}
			return 800;
		}

		public override ushort PathWalkCostFor(Pawn p)
		{
			if (!KnowsOfTrap(p))
			{
				return 0;
			}
			return 40;
		}

		public override bool IsDangerousFor(Pawn p)
		{
			return KnowsOfTrap(p);
		}

		public void Spring(Pawn p)
		{
			bool spawned = base.Spawned;
			Map map = base.Map;
			SpringSub(p);
			if (def.building.trapDestroyOnSpring)
			{
				if (!base.Destroyed)
				{
					Destroy();
				}
				if (spawned)
				{
					CheckAutoRebuild(map);
				}
			}
		}

		public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
		{
			bool spawned = base.Spawned;
			Map map = base.Map;
			base.Kill(dinfo, exactCulprit);
			if (spawned)
			{
				CheckAutoRebuild(map);
			}
		}

		protected abstract void SpringSub(Pawn p);

		private void CheckAutoRebuild(Map map)
		{
			if (autoRearm && CanSetAutoRearm && map != null && GenConstruct.CanPlaceBlueprintAt(def, base.Position, base.Rotation, map, godMode: false, null, null, base.Stuff).Accepted)
			{
				GenConstruct.PlaceBlueprintForBuild(def, base.Position, map, base.Rotation, Faction.OfPlayer, base.Stuff);
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (CanSetAutoRearm)
			{
				Command_Toggle command_Toggle = new Command_Toggle();
				command_Toggle.defaultLabel = "CommandAutoRearm".Translate();
				command_Toggle.defaultDesc = "CommandAutoRearmDesc".Translate();
				command_Toggle.hotKey = KeyBindingDefOf.Misc3;
				command_Toggle.icon = TexCommand.RearmTrap;
				command_Toggle.isActive = (() => autoRearm);
				command_Toggle.toggleAction = delegate
				{
					autoRearm = !autoRearm;
				};
				yield return command_Toggle;
			}
		}
	}
}
