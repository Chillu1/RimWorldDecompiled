using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class AlertsReadout
{
	private List<Alert> activeAlerts = new List<Alert>(16);

	private int curAlertIndex;

	private float lastFinalY;

	private int mouseoverAlertIndex = -1;

	public static List<Type> allAlertTypesCached;

	private readonly List<Alert> AllAlerts = new List<Alert>();

	private const int StartTickDelay = 600;

	private const int AlertCycleLength = 24;

	private const int UpdateAlertsFromQuestsIntervalFrames = 20;

	private const int UpdateAlertsFromPreceptsIntervalFrames = 20;

	private const int UpdateAlertsFromScenarioIntervalFrames = 20;

	private const int UpdateAlertsFromSignalActionsIntervalFrames = 20;

	private readonly List<AlertPriority> PriosInDrawOrder;

	private Dictionary<Precept, List<Alert>> activePreceptAlerts = new Dictionary<Precept, List<Alert>>();

	private List<Alert> activeScenarioAlerts = new List<Alert>();

	private List<Alert> activeSignalActionAlerts = new List<Alert>();

	public float AlertsHeight
	{
		get
		{
			float num = 0f;
			for (int i = 0; i < activeAlerts.Count; i++)
			{
				num += activeAlerts[i].Height;
			}
			return num;
		}
	}

	public AlertsReadout()
	{
		DeepProfiler.Start("Instantiating Alerts");
		if (allAlertTypesCached == null)
		{
			allAlertTypesCached = new List<Type>();
			foreach (Type item2 in typeof(Alert).AllLeafSubclasses())
			{
				if (!typeof(Alert_Custom).IsAssignableFrom(item2) && !typeof(Alert_CustomCritical).IsAssignableFrom(item2))
				{
					allAlertTypesCached.Add(item2);
				}
			}
		}
		AllAlerts.Clear();
		foreach (Type item3 in allAlertTypesCached)
		{
			Alert alert = (Alert)Activator.CreateInstance(item3);
			if (alert.EnabledWithActiveExpansions)
			{
				AllAlerts.Add(alert);
			}
		}
		DeepProfiler.End();
		if (PriosInDrawOrder != null)
		{
			return;
		}
		PriosInDrawOrder = new List<AlertPriority>();
		foreach (AlertPriority value in Enum.GetValues(typeof(AlertPriority)))
		{
			PriosInDrawOrder.Add(value);
		}
		PriosInDrawOrder.Reverse();
	}

	public void AlertsReadoutUpdate()
	{
		if (Mathf.Max(Find.TickManager.TicksGame, Find.TutorialState.endTick) < 600)
		{
			return;
		}
		if (Find.Storyteller.def.disableAlerts)
		{
			activeAlerts.Clear();
			return;
		}
		curAlertIndex++;
		if (curAlertIndex >= 24)
		{
			curAlertIndex = 0;
		}
		for (int i = curAlertIndex; i < AllAlerts.Count; i += 24)
		{
			CheckAddOrRemoveAlert(AllAlerts[i]);
		}
		using (new ProfilerBlock("Alerts from quests"))
		{
			if (Time.frameCount % 20 == 0)
			{
				List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
				for (int j = 0; j < questsListForReading.Count; j++)
				{
					List<QuestPart> partsListForReading = questsListForReading[j].PartsListForReading;
					for (int k = 0; k < partsListForReading.Count; k++)
					{
						if (partsListForReading[k] is QuestPartActivable { CachedAlert: { } cachedAlert } questPartActivable)
						{
							bool flag = questsListForReading[j].State != QuestState.Ongoing || questPartActivable.State != QuestPartState.Enabled;
							bool alertDirty = questPartActivable.AlertDirty;
							CheckAddOrRemoveAlert(cachedAlert, flag || alertDirty);
							if (alertDirty)
							{
								questPartActivable.ClearCachedAlert();
							}
						}
					}
				}
			}
		}
		using (new ProfilerBlock("Alerts from precepts"))
		{
			if (ModsConfig.IdeologyActive && Time.frameCount % 20 == 0)
			{
				foreach (List<Alert> value2 in activePreceptAlerts.Values)
				{
					value2.Clear();
				}
				foreach (Ideo allIdeo in Faction.OfPlayer.ideos.AllIdeos)
				{
					foreach (Precept item in allIdeo.PreceptsListForReading)
					{
						if (!activePreceptAlerts.TryGetValue(item, out var value))
						{
							value = new List<Alert>();
							activePreceptAlerts[item] = value;
						}
						foreach (Alert alert2 in item.GetAlerts())
						{
							CheckAddOrRemoveAlert(alert2);
							value.Add(alert2);
						}
					}
				}
				for (int l = 0; l < activeAlerts.Count; l++)
				{
					if (activeAlerts[l] is Alert_Precept { sourcePrecept: var sourcePrecept } alert_Precept)
					{
						CheckAddOrRemoveAlert(activeAlerts[l], sourcePrecept != null && (!activePreceptAlerts.ContainsKey(sourcePrecept) || !activePreceptAlerts[sourcePrecept].Contains(alert_Precept)));
					}
				}
			}
		}
		using (new ProfilerBlock("Alerts from scenario"))
		{
			if (Time.frameCount % 20 == 0)
			{
				activeScenarioAlerts.Clear();
				foreach (ScenPart allPart in Find.Scenario.AllParts)
				{
					foreach (Alert alert3 in allPart.GetAlerts())
					{
						CheckAddOrRemoveAlert(alert3);
						activeScenarioAlerts.Add(alert3);
					}
				}
				for (int m = 0; m < activeAlerts.Count; m++)
				{
					if (activeAlerts[m] is Alert_Scenario alert_Scenario)
					{
						CheckAddOrRemoveAlert(alert_Scenario, !activeScenarioAlerts.Contains(alert_Scenario));
					}
				}
				for (int n = 0; n < activeScenarioAlerts.Count; n++)
				{
					CheckAddOrRemoveAlert(activeScenarioAlerts[n]);
				}
			}
		}
		using (new ProfilerBlock("Alerts from delayed actions"))
		{
			if (Time.frameCount % 20 == 0)
			{
				activeSignalActionAlerts.Clear();
				List<Map> maps = Find.Maps;
				for (int num = 0; num < maps.Count; num++)
				{
					List<Thing> list = maps[num].listerThings.ThingsInGroup(ThingRequestGroup.ActionDelay);
					for (int num2 = 0; num2 < list.Count; num2++)
					{
						SignalAction_Delay signalAction_Delay = list[num2] as SignalAction_Delay;
						if (signalAction_Delay.Activated && signalAction_Delay.Alert != null)
						{
							activeSignalActionAlerts.Add(signalAction_Delay.Alert);
						}
					}
				}
				for (int num3 = 0; num3 < activeAlerts.Count; num3++)
				{
					if (activeAlerts[num3] is Alert_ActionDelay alert_ActionDelay)
					{
						CheckAddOrRemoveAlert(alert_ActionDelay, !activeSignalActionAlerts.Contains(alert_ActionDelay));
					}
				}
				for (int num4 = 0; num4 < activeSignalActionAlerts.Count; num4++)
				{
					CheckAddOrRemoveAlert(activeSignalActionAlerts[num4]);
				}
			}
		}
		for (int num5 = activeAlerts.Count - 1; num5 >= 0; num5--)
		{
			Alert alert = activeAlerts[num5];
			try
			{
				activeAlerts[num5].AlertActiveUpdate();
			}
			catch (Exception ex)
			{
				Log.ErrorOnce("Exception updating alert " + alert?.ToString() + ": " + ex, 743575);
				activeAlerts.RemoveAt(num5);
			}
		}
		if (mouseoverAlertIndex >= 0 && mouseoverAlertIndex < activeAlerts.Count)
		{
			IEnumerable<GlobalTargetInfo> allCulprits = activeAlerts[mouseoverAlertIndex].GetReport().AllCulprits;
			if (allCulprits != null)
			{
				foreach (GlobalTargetInfo item2 in allCulprits)
				{
					TargetHighlighter.Highlight(item2);
				}
			}
		}
		mouseoverAlertIndex = -1;
	}

	private void CheckAddOrRemoveAlert(Alert alert, bool forceRemove = false)
	{
		try
		{
			alert.Recalculate();
			if (!forceRemove && alert.Active)
			{
				if (!activeAlerts.Contains(alert))
				{
					activeAlerts.Add(alert);
					alert.Notify_Started();
				}
			}
			else
			{
				activeAlerts.Remove(alert);
			}
		}
		catch (Exception ex)
		{
			Log.ErrorOnce("Exception processing alert " + alert?.ToString() + ": " + ex, 743575);
			activeAlerts.Remove(alert);
		}
	}

	public void AlertsReadoutOnGUI()
	{
		if (Event.current.type == EventType.Layout || Event.current.type == EventType.MouseDrag || activeAlerts.Count == 0)
		{
			return;
		}
		Alert alert = null;
		AlertPriority alertPriority = AlertPriority.Critical;
		bool flag = false;
		float alertsHeight = AlertsHeight;
		float num = Find.LetterStack.LastTopY - alertsHeight;
		Rect rect = new Rect((float)UI.screenWidth - 154f, num, 154f, lastFinalY - num);
		float num2 = GenUI.BackgroundDarkAlphaForText();
		if (num2 > 0.001f)
		{
			GUI.color = new Color(1f, 1f, 1f, num2);
			Widgets.DrawShadowAround(rect);
			GUI.color = Color.white;
		}
		float num3 = num;
		if (num3 < 0f)
		{
			num3 = 0f;
		}
		for (int i = 0; i < PriosInDrawOrder.Count; i++)
		{
			AlertPriority alertPriority2 = PriosInDrawOrder[i];
			for (int j = 0; j < activeAlerts.Count; j++)
			{
				Alert alert2 = activeAlerts[j];
				if (alert2.Priority == alertPriority2)
				{
					if (!flag)
					{
						alertPriority = alertPriority2;
						flag = true;
					}
					Rect rect2 = alert2.DrawAt(num3, alertPriority2 != alertPriority);
					if (Mouse.IsOver(rect2))
					{
						alert = alert2;
						mouseoverAlertIndex = j;
					}
					num3 += rect2.height;
				}
			}
		}
		lastFinalY = num3;
		UIHighlighter.HighlightOpportunity(rect, "Alerts");
		if (alert != null)
		{
			alert.DrawInfoPane();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Alerts, KnowledgeAmount.FrameDisplayed);
			CheckAddOrRemoveAlert(alert);
		}
	}
}
