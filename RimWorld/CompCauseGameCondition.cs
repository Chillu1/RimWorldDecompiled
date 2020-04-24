using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompCauseGameCondition : ThingComp
	{
		protected CompInitiatable initiatableComp;

		protected Site siteLink;

		private Dictionary<Map, GameCondition> causedConditions = new Dictionary<Map, GameCondition>();

		private static List<Map> tmpDeadConditionMaps = new List<Map>();

		public CompProperties_CausesGameCondition Props => (CompProperties_CausesGameCondition)props;

		public GameConditionDef ConditionDef => Props.conditionDef;

		public IEnumerable<GameCondition> CausedConditions => causedConditions.Values;

		public bool Active
		{
			get
			{
				if (initiatableComp != null)
				{
					return initiatableComp.Initiated;
				}
				return true;
			}
		}

		public int MyTile
		{
			get
			{
				if (siteLink != null)
				{
					return siteLink.Tile;
				}
				if (parent.SpawnedOrAnyParentSpawned)
				{
					return parent.Tile;
				}
				return -1;
			}
		}

		public void LinkWithSite(Site site)
		{
			siteLink = site;
		}

		public override void PostPostMake()
		{
			base.PostPostMake();
			CacheComps();
		}

		private void CacheComps()
		{
			initiatableComp = parent.GetComp<CompInitiatable>();
		}

		public override void PostExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				causedConditions.RemoveAll((KeyValuePair<Map, GameCondition> x) => !Find.Maps.Contains(x.Key));
			}
			Scribe_References.Look(ref siteLink, "siteLink");
			Scribe_Collections.Look(ref causedConditions, "causedConditions", LookMode.Reference, LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				causedConditions.RemoveAll((KeyValuePair<Map, GameCondition> x) => x.Value == null);
				foreach (KeyValuePair<Map, GameCondition> causedCondition in causedConditions)
				{
					causedCondition.Value.conditionCauser = parent;
				}
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				CacheComps();
			}
		}

		public bool InAoE(int tile)
		{
			if (MyTile == -1 || !Active)
			{
				return false;
			}
			return Find.WorldGrid.TraversalDistanceBetween(MyTile, tile, passImpassable: true, Props.worldRange + 1) <= Props.worldRange;
		}

		protected GameCondition GetConditionInstance(Map map)
		{
			if (!causedConditions.TryGetValue(map, out GameCondition value) && Props.preventConditionStacking)
			{
				value = map.GameConditionManager.GetActiveCondition(Props.conditionDef);
				if (value != null)
				{
					causedConditions.Add(map, value);
					SetupCondition(value, map);
				}
			}
			return value;
		}

		public override void CompTick()
		{
			if (Active)
			{
				foreach (Map map in Find.Maps)
				{
					if (InAoE(map.Tile))
					{
						EnforceConditionOn(map);
					}
				}
			}
			tmpDeadConditionMaps.Clear();
			foreach (KeyValuePair<Map, GameCondition> causedCondition in causedConditions)
			{
				if (causedCondition.Value.Expired || !causedCondition.Key.GameConditionManager.ConditionIsActive(causedCondition.Value.def))
				{
					tmpDeadConditionMaps.Add(causedCondition.Key);
				}
			}
			foreach (Map tmpDeadConditionMap in tmpDeadConditionMaps)
			{
				causedConditions.Remove(tmpDeadConditionMap);
			}
		}

		private GameCondition EnforceConditionOn(Map map)
		{
			GameCondition gameCondition = GetConditionInstance(map);
			if (gameCondition == null)
			{
				gameCondition = CreateConditionOn(map);
			}
			else
			{
				gameCondition.TicksLeft = gameCondition.TransitionTicks;
			}
			return gameCondition;
		}

		protected virtual GameCondition CreateConditionOn(Map map)
		{
			GameCondition gameCondition = GameConditionMaker.MakeCondition(ConditionDef);
			gameCondition.Duration = gameCondition.TransitionTicks;
			gameCondition.conditionCauser = parent;
			map.gameConditionManager.RegisterCondition(gameCondition);
			causedConditions.Add(map, gameCondition);
			SetupCondition(gameCondition, map);
			return gameCondition;
		}

		protected virtual void SetupCondition(GameCondition condition, Map map)
		{
			condition.suppressEndMessage = true;
		}

		protected void ReSetupAllConditions()
		{
			foreach (KeyValuePair<Map, GameCondition> causedCondition in causedConditions)
			{
				SetupCondition(causedCondition.Value, causedCondition.Key);
			}
		}

		public override void PostDeSpawn(Map map)
		{
			Messages.Message("MessageConditionCauserDespawned".Translate(parent.def.LabelCap), new TargetInfo(parent.Position, map), MessageTypeDefOf.NeutralEvent);
		}

		public override string CompInspectStringExtra()
		{
			if (Prefs.DevMode)
			{
				GameCondition gameCondition = parent.Map.GameConditionManager.ActiveConditions.Find((GameCondition c) => c.def == Props.conditionDef);
				if (gameCondition == null)
				{
					return base.CompInspectStringExtra();
				}
				return "[DEV] Current map condition\n[DEV] Ticks Passed: " + gameCondition.TicksPassed + "\n[DEV] Ticks Left: " + gameCondition.TicksLeft;
			}
			return base.CompInspectStringExtra();
		}

		public virtual void RandomizeSettings()
		{
		}
	}
}
