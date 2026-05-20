using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public sealed class GameConditionManager : IExposable
{
	private class MapBrightnessTracker : IExposable
	{
		private float brightness = 1f;

		private float targetBrightness = 1f;

		private float lerp = 1f;

		private float lerpSeconds;

		public float CurBrightness => Mathf.Lerp(brightness, targetBrightness, lerp);

		public bool Changing => lerp < 1f;

		public bool DarknessVisible
		{
			get
			{
				if (!Changing)
				{
					return CurBrightness < 1f;
				}
				return true;
			}
		}

		public void Change(float newBrightness, float lerpSeconds = 5f)
		{
			if (ModLister.CheckAnomaly("Map brightness"))
			{
				brightness = CurBrightness;
				targetBrightness = newBrightness;
				lerp = 0f;
				this.lerpSeconds = lerpSeconds;
			}
		}

		public void Tick()
		{
			if (ModsConfig.AnomalyActive && lerp < 1f)
			{
				lerp += lerpSeconds / 60f * Time.deltaTime;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref brightness, "brightness", 1f);
			Scribe_Values.Look(ref targetBrightness, "targetBrightness", 1f);
			Scribe_Values.Look(ref lerp, "lerp", 1f);
			Scribe_Values.Look(ref lerpSeconds, "lerpSeconds", 5f);
		}
	}

	public Map ownerMap;

	private List<GameCondition> activeConditions = new List<GameCondition>();

	private MapBrightnessTracker mapBrightnessTracker;

	private bool cachedAlwaysDark;

	private const float TextPadding = 6f;

	public List<GameCondition> ActiveConditions => activeConditions;

	public GameConditionManager Parent
	{
		get
		{
			if (ownerMap != null)
			{
				return Find.World.gameConditionManager;
			}
			return null;
		}
	}

	public float MapBrightness
	{
		get
		{
			if (!ModsConfig.AnomalyActive)
			{
				return 1f;
			}
			return mapBrightnessTracker?.CurBrightness ?? 1f;
		}
	}

	public bool BrightnessChanging
	{
		get
		{
			if (ModsConfig.AnomalyActive)
			{
				return mapBrightnessTracker?.Changing ?? false;
			}
			return false;
		}
	}

	public bool DarknessVisible
	{
		get
		{
			if (ModsConfig.AnomalyActive && DebugViewSettings.drawDarknessOverlay)
			{
				return mapBrightnessTracker?.DarknessVisible ?? false;
			}
			return false;
		}
	}

	public bool IsAlwaysDarkOutside => cachedAlwaysDark;

	public GameConditionManager(Map map)
	{
		ownerMap = map;
		mapBrightnessTracker = new MapBrightnessTracker();
	}

	public GameConditionManager(World world)
	{
	}

	public bool ElectricityDisabled(Map map)
	{
		foreach (GameCondition activeCondition in activeConditions)
		{
			if (activeCondition.ElectricityDisabled && activeCondition.CanApplyOnMap(map) && !activeCondition.HiddenByOtherCondition(map))
			{
				return true;
			}
		}
		return Parent?.ElectricityDisabled(map) ?? false;
	}

	public float FishPopulationOffsetFactorPerDay(Map map, out GameCondition culprit)
	{
		culprit = null;
		if (!ModsConfig.OdysseyActive)
		{
			return 0f;
		}
		foreach (GameCondition activeCondition in activeConditions)
		{
			if (activeCondition is GameCondition_GillRot gameCondition_GillRot && activeCondition.CanApplyOnMap(map) && !activeCondition.HiddenByOtherCondition(map))
			{
				culprit = activeCondition;
				return gameCondition_GillRot.fishPopulationOffsetFactorPerDay;
			}
		}
		return 0.025f;
	}

	public void RegisterCondition(GameCondition cond)
	{
		activeConditions.Add(cond);
		cond.startTick = Mathf.Max(cond.startTick, Find.TickManager.TicksGame);
		cond.gameConditionManager = this;
		cond.Init();
		if (!cachedAlwaysDark)
		{
			cachedAlwaysDark = cond.Permanent && cond is GameCondition_NoSunlight;
		}
		foreach (Map map in Find.Maps)
		{
			map.events.Notify_GameConditionAdded(cond);
		}
	}

	public void OnConditionEnd(GameCondition cond)
	{
		activeConditions.Remove(cond);
		if (cachedAlwaysDark && cond.Permanent && cond is GameCondition_NoSunlight)
		{
			RecalcAlwaysDark();
		}
		foreach (Map map in Find.Maps)
		{
			map.events.Notify_GameConditionRemoved(cond);
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref activeConditions, "activeConditions", LookMode.Deep);
		Scribe_Deep.Look(ref mapBrightnessTracker, "mapBrightnessTracker");
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			for (int i = 0; i < activeConditions.Count; i++)
			{
				activeConditions[i].gameConditionManager = this;
			}
		}
		else if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (mapBrightnessTracker == null && ownerMap != null)
			{
				mapBrightnessTracker = new MapBrightnessTracker();
			}
			RecalcAlwaysDark();
		}
	}

	public void GameConditionManagerTick()
	{
		for (int num = activeConditions.Count - 1; num >= 0; num--)
		{
			GameCondition gameCondition = activeConditions[num];
			if (gameCondition.Expired)
			{
				gameCondition.End();
			}
			else
			{
				gameCondition.GameConditionTick();
			}
		}
		mapBrightnessTracker?.Tick();
	}

	public void SetTargetBrightness(float target, float lerpSeconds = 5f)
	{
		mapBrightnessTracker?.Change(target, lerpSeconds);
		ownerMap?.mapDrawer.WholeMapChanged(MapMeshFlagDefOf.GroundGlow);
	}

	public void GameConditionManagerDraw(Map map)
	{
		for (int num = activeConditions.Count - 1; num >= 0; num--)
		{
			activeConditions[num].GameConditionDraw(map);
		}
		Parent?.GameConditionManagerDraw(map);
	}

	public void DoSteadyEffects(IntVec3 c, Map map)
	{
		for (int i = 0; i < activeConditions.Count; i++)
		{
			activeConditions[i].DoCellSteadyEffects(c, map);
		}
		Parent?.DoSteadyEffects(c, map);
	}

	private void RecalcAlwaysDark()
	{
		cachedAlwaysDark = false;
		for (int i = 0; i < activeConditions.Count; i++)
		{
			if (activeConditions[i].Permanent && activeConditions[i] is GameCondition_NoSunlight)
			{
				cachedAlwaysDark = true;
				break;
			}
		}
	}

	public bool ConditionIsActive(GameConditionDef def)
	{
		return GetActiveCondition(def) != null;
	}

	public GameCondition GetActiveCondition(GameConditionDef def)
	{
		for (int i = 0; i < activeConditions.Count; i++)
		{
			if (def == activeConditions[i].def)
			{
				return activeConditions[i];
			}
		}
		return Parent?.GetActiveCondition(def);
	}

	public T GetActiveCondition<T>() where T : GameCondition
	{
		for (int i = 0; i < activeConditions.Count; i++)
		{
			if (activeConditions[i] is T result)
			{
				return result;
			}
		}
		GameConditionManager parent = Parent;
		if (parent == null)
		{
			return null;
		}
		return parent.GetActiveCondition<T>();
	}

	public PsychicDroneLevel GetHighestPsychicDroneLevelFor(Gender gender, Map map)
	{
		PsychicDroneLevel psychicDroneLevel = PsychicDroneLevel.None;
		for (int i = 0; i < ActiveConditions.Count; i++)
		{
			if (activeConditions[i].CanApplyOnMap(map) && activeConditions[i] is GameCondition_PsychicEmanation gameCondition_PsychicEmanation && gameCondition_PsychicEmanation.gender == gender && (int)gameCondition_PsychicEmanation.level > (int)psychicDroneLevel)
			{
				psychicDroneLevel = gameCondition_PsychicEmanation.level;
			}
		}
		return psychicDroneLevel;
	}

	public void GetChildren(List<GameConditionManager> outChildren)
	{
		if (this == Find.World.gameConditionManager)
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				outChildren.Add(maps[i].gameConditionManager);
			}
		}
	}

	public float TotalHeightAt(float width)
	{
		float num = 0f;
		for (int i = 0; i < activeConditions.Count; i++)
		{
			if ((ownerMap == null || !activeConditions[i].HiddenByOtherCondition(ownerMap)) && activeConditions[i].def.displayOnUI)
			{
				num += Text.CalcHeight(activeConditions[i].LabelCap, width - 6f);
			}
		}
		if (Parent != null)
		{
			num += Parent.TotalHeightAt(width);
		}
		return num;
	}

	public void DoConditionsUI(Rect rect)
	{
		Widgets.BeginGroup(rect);
		float num = 0f;
		for (int i = 0; i < activeConditions.Count; i++)
		{
			if (!activeConditions[i].def.displayOnUI || (ownerMap != null && (!activeConditions[i].CanApplyOnMap(ownerMap) || activeConditions[i].HiddenByOtherCondition(ownerMap))))
			{
				continue;
			}
			string labelCap = activeConditions[i].LabelCap;
			Rect rect2 = new Rect(0f, num, rect.width, Text.CalcHeight(labelCap, rect.width - 6f));
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.DrawHighlightIfMouseover(rect2);
			Rect rect3 = rect2;
			rect3.width -= 6f;
			Widgets.Label(rect3, labelCap);
			if (Mouse.IsOver(rect2))
			{
				TooltipHandler.TipRegion(rect2, new TipSignal(activeConditions[i].TooltipString, 0x3A2DF42A ^ i));
			}
			if (Widgets.ButtonInvisible(rect2))
			{
				if (activeConditions[i].conditionCauser != null && !activeConditions[i].hideSource && CameraJumper.CanJump(activeConditions[i].conditionCauser))
				{
					CameraJumper.TryJumpAndSelect(activeConditions[i].conditionCauser);
				}
				else if (activeConditions[i].quest != null)
				{
					Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
					((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(activeConditions[i].quest);
				}
			}
			num += rect2.height;
		}
		rect.yMin += num;
		GUI.EndGroup();
		Text.Anchor = TextAnchor.UpperLeft;
		Parent?.DoConditionsUI(rect);
	}

	public void GetAllGameConditionsAffectingMap(Map map, List<GameCondition> listToFill)
	{
		for (int i = 0; i < activeConditions.Count; i++)
		{
			if (activeConditions[i].CanApplyOnMap(map))
			{
				listToFill.Add(activeConditions[i]);
			}
		}
		if (Parent != null)
		{
			Parent.GetAllGameConditionsAffectingMap(map, listToFill);
		}
	}

	internal float AggregateTemperatureOffset()
	{
		float num = 0f;
		for (int i = 0; i < activeConditions.Count; i++)
		{
			if (activeConditions[i].CanApplyOnMap(ownerMap))
			{
				num += activeConditions[i].TemperatureOffset();
			}
		}
		if (Parent != null)
		{
			num += Parent.AggregateTemperatureOffset();
		}
		return num;
	}

	internal float AggregateAnimalDensityFactor(Map map)
	{
		float num = 1f;
		for (int i = 0; i < activeConditions.Count; i++)
		{
			if (activeConditions[i].CanApplyOnMap(ownerMap))
			{
				num *= activeConditions[i].AnimalDensityFactor(map);
			}
		}
		if (Parent != null)
		{
			num *= Parent.AggregateAnimalDensityFactor(map);
		}
		return num;
	}

	internal float AggregatePlantDensityFactor(Map map)
	{
		float num = 1f;
		for (int i = 0; i < activeConditions.Count; i++)
		{
			if (activeConditions[i].CanApplyOnMap(ownerMap))
			{
				num *= activeConditions[i].PlantDensityFactor(map);
			}
		}
		if (Parent != null)
		{
			num *= Parent.AggregatePlantDensityFactor(map);
		}
		return num;
	}

	internal float AggregateSkyGazeJoyGainFactor(Map map)
	{
		float num = 1f;
		for (int i = 0; i < activeConditions.Count; i++)
		{
			if (activeConditions[i].CanApplyOnMap(ownerMap))
			{
				num *= activeConditions[i].SkyGazeJoyGainFactor(map);
			}
		}
		if (Parent != null)
		{
			num *= Parent.AggregateSkyGazeJoyGainFactor(map);
		}
		return num;
	}

	internal float AggregateSkyGazeChanceFactor(Map map)
	{
		float num = 1f;
		for (int i = 0; i < activeConditions.Count; i++)
		{
			if (activeConditions[i].CanApplyOnMap(ownerMap))
			{
				num *= activeConditions[i].SkyGazeChanceFactor(map);
			}
		}
		if (Parent != null)
		{
			num *= Parent.AggregateSkyGazeChanceFactor(map);
		}
		return num;
	}

	internal bool AllowEnjoyableOutsideNow(Map map)
	{
		GameConditionDef reason;
		return AllowEnjoyableOutsideNow(map, out reason);
	}

	internal bool AllowEnjoyableOutsideNow(Map map, out GameConditionDef reason)
	{
		for (int i = 0; i < activeConditions.Count; i++)
		{
			GameCondition gameCondition = activeConditions[i];
			if (activeConditions[i].CanApplyOnMap(ownerMap) && !gameCondition.AllowEnjoyableOutsideNow(map))
			{
				reason = gameCondition.def;
				return false;
			}
		}
		reason = null;
		if (Parent != null)
		{
			return Parent.AllowEnjoyableOutsideNow(map, out reason);
		}
		return true;
	}

	public string DebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (GameCondition activeCondition in activeConditions)
		{
			stringBuilder.AppendLine(Scribe.saver.DebugOutputFor(activeCondition));
		}
		return stringBuilder.ToString();
	}
}
