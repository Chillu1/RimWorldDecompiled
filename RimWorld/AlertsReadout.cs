using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class AlertsReadout
	{
		private List<Alert> activeAlerts = new List<Alert>(16);

		private int curAlertIndex;

		private float lastFinalY;

		private int mouseoverAlertIndex = -1;

		private readonly List<Alert> AllAlerts = new List<Alert>();

		private const int StartTickDelay = 600;

		public const float AlertListWidth = 164f;

		private const int AlertCycleLength = 24;

		private const int UpdateAlertsFromQuestsIntervalFrames = 20;

		private readonly List<AlertPriority> PriosInDrawOrder;

		public AlertsReadout()
		{
			AllAlerts.Clear();
			foreach (Type item2 in typeof(Alert).AllLeafSubclasses())
			{
				if (!(item2 == typeof(Alert_Custom)) && !(item2 == typeof(Alert_CustomCritical)))
				{
					AllAlerts.Add((Alert)Activator.CreateInstance(item2));
				}
			}
			if (PriosInDrawOrder == null)
			{
				PriosInDrawOrder = new List<AlertPriority>();
				foreach (AlertPriority value in Enum.GetValues(typeof(AlertPriority)))
				{
					PriosInDrawOrder.Add(value);
				}
				PriosInDrawOrder.Reverse();
			}
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
			if (Time.frameCount % 20 == 0)
			{
				List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
				for (int j = 0; j < questsListForReading.Count; j++)
				{
					List<QuestPart> partsListForReading = questsListForReading[j].PartsListForReading;
					for (int k = 0; k < partsListForReading.Count; k++)
					{
						QuestPartActivable questPartActivable = partsListForReading[k] as QuestPartActivable;
						if (questPartActivable == null)
						{
							continue;
						}
						Alert cachedAlert = questPartActivable.CachedAlert;
						if (cachedAlert != null)
						{
							bool flag = questsListForReading[j].State != QuestState.Ongoing || questPartActivable.State != QuestPartState.Enabled;
							bool alertDirty = questPartActivable.AlertDirty;
							CheckAddOrRemoveAlert(cachedAlert, flag | alertDirty);
							if (alertDirty)
							{
								questPartActivable.ClearCachedAlert();
							}
						}
					}
				}
			}
			for (int num = activeAlerts.Count - 1; num >= 0; num--)
			{
				Alert alert = activeAlerts[num];
				try
				{
					activeAlerts[num].AlertActiveUpdate();
				}
				catch (Exception ex)
				{
					Log.ErrorOnce("Exception updating alert " + alert.ToString() + ": " + ex.ToString(), 743575);
					activeAlerts.RemoveAt(num);
				}
			}
			if (mouseoverAlertIndex >= 0 && mouseoverAlertIndex < activeAlerts.Count)
			{
				IEnumerable<GlobalTargetInfo> allCulprits = activeAlerts[mouseoverAlertIndex].GetReport().AllCulprits;
				if (allCulprits != null)
				{
					foreach (GlobalTargetInfo item in allCulprits)
					{
						TargetHighlighter.Highlight(item);
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
				Log.ErrorOnce("Exception processing alert " + alert.ToString() + ": " + ex.ToString(), 743575);
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
			float num = 0f;
			for (int i = 0; i < activeAlerts.Count; i++)
			{
				num += activeAlerts[i].Height;
			}
			float num2 = Find.LetterStack.LastTopY - num;
			Rect rect = new Rect((float)UI.screenWidth - 154f, num2, 154f, lastFinalY - num2);
			float num3 = GenUI.BackgroundDarkAlphaForText();
			if (num3 > 0.001f)
			{
				GUI.color = new Color(1f, 1f, 1f, num3);
				Widgets.DrawShadowAround(rect);
				GUI.color = Color.white;
			}
			float num4 = num2;
			if (num4 < 0f)
			{
				num4 = 0f;
			}
			for (int j = 0; j < PriosInDrawOrder.Count; j++)
			{
				AlertPriority alertPriority2 = PriosInDrawOrder[j];
				for (int k = 0; k < activeAlerts.Count; k++)
				{
					Alert alert2 = activeAlerts[k];
					if (alert2.Priority == alertPriority2)
					{
						if (!flag)
						{
							alertPriority = alertPriority2;
							flag = true;
						}
						Rect rect2 = alert2.DrawAt(num4, alertPriority2 != alertPriority);
						if (Mouse.IsOver(rect2))
						{
							alert = alert2;
							mouseoverAlertIndex = k;
						}
						num4 += rect2.height;
					}
				}
			}
			lastFinalY = num4;
			UIHighlighter.HighlightOpportunity(rect, "Alerts");
			if (alert != null)
			{
				alert.DrawInfoPane();
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Alerts, KnowledgeAmount.FrameDisplayed);
				CheckAddOrRemoveAlert(alert);
			}
		}
	}
}
