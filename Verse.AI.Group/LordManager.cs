using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse.AI.Group;

public sealed class LordManager : IExposable, IDisposable
{
	public Map map;

	public List<Lord> lords = new List<Lord>();

	public List<StencilDrawerForCells> stencilDrawers = new List<StencilDrawerForCells>();

	public LordManager(Map map)
	{
		this.map = map;
		map.events.BuildingSpawned += Notify_BuildingSpawned;
		map.events.BuildingDespawned += Notify_BuildingDespawned;
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref lords, "lords", LookMode.Deep);
		Scribe_Collections.Look(ref stencilDrawers, "stencilDrawers", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			for (int i = 0; i < lords.Count; i++)
			{
				lords[i].lordManager = this;
			}
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (stencilDrawers == null)
			{
				stencilDrawers = new List<StencilDrawerForCells>();
			}
			for (int j = 0; j < lords.Count; j++)
			{
				Find.SignalManager.RegisterReceiver(lords[j]);
			}
		}
	}

	public void LordManagerTick()
	{
		for (int i = 0; i < lords.Count; i++)
		{
			try
			{
				lords[i].LordTick();
			}
			catch (Exception arg)
			{
				Lord lord = lords[i];
				Log.Error(string.Format("Exception while ticking lord with job {0}: \r\n{1}", (lord?.LordJob == null) ? "NULL" : lord.LordJob.ToString(), arg));
			}
		}
		for (int num = lords.Count - 1; num >= 0; num--)
		{
			LordToil curLordToil = lords[num].CurLordToil;
			if (curLordToil == null || curLordToil.ShouldFail)
			{
				RemoveLord(lords[num]);
			}
		}
		for (int num2 = stencilDrawers.Count - 1; num2 >= 0; num2--)
		{
			if (!lords.Contains(stencilDrawers[num2].sourceLord) && stencilDrawers[num2].ticksLeftWithoutLord <= 0)
			{
				stencilDrawers.RemoveAt(num2);
			}
		}
	}

	public void LordManagerUpdate()
	{
		if (DebugViewSettings.drawLords)
		{
			for (int i = 0; i < lords.Count; i++)
			{
				lords[i].DebugDraw();
			}
		}
		foreach (StencilDrawerForCells stencilDrawer in stencilDrawers)
		{
			stencilDrawer.Draw();
			if (!lords.Contains(stencilDrawer.sourceLord))
			{
				stencilDrawer.ticksLeftWithoutLord--;
			}
		}
	}

	public void LordManagerOnGUI()
	{
		if (DebugViewSettings.drawLords)
		{
			for (int i = 0; i < lords.Count; i++)
			{
				lords[i].DebugOnGUI();
			}
		}
		if (!DebugViewSettings.drawDuties)
		{
			return;
		}
		Text.Anchor = TextAnchor.MiddleCenter;
		Text.Font = GameFont.Tiny;
		foreach (Pawn allPawn in map.mapPawns.AllPawns)
		{
			if (allPawn.Spawned)
			{
				string text = "";
				if (!allPawn.Dead && allPawn.mindState.duty != null)
				{
					text = allPawn.mindState.duty.ToString();
				}
				if (allPawn.InMentalState)
				{
					text = text + "\nMentalState=" + allPawn.MentalState;
				}
				Vector2 vector = allPawn.DrawPos.MapToUIPosition();
				Widgets.Label(new Rect(vector.x - 100f, vector.y - 100f, 200f, 200f), text);
			}
		}
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public void AddLord(Lord newLord)
	{
		lords.Add(newLord);
		newLord.lordManager = this;
		Find.SignalManager.RegisterReceiver(newLord);
		map.events.Notify_LordAdded(newLord);
	}

	public void RemoveLord(Lord oldLord)
	{
		map.events.Notify_LordRemoved(oldLord);
		lords.Remove(oldLord);
		Find.SignalManager.DeregisterReceiver(oldLord);
		oldLord.Cleanup();
	}

	public bool TryGetLordByJob<T>(Faction faction, out T lord) where T : LordJob
	{
		for (int i = 0; i < lords.Count; i++)
		{
			if (lords[i].faction == faction && lords[i].LordJob is T val)
			{
				lord = val;
				return true;
			}
		}
		lord = null;
		return false;
	}

	public Lord LordOf(Pawn p)
	{
		for (int i = 0; i < lords.Count; i++)
		{
			Lord lord = lords[i];
			for (int j = 0; j < lord.ownedPawns.Count; j++)
			{
				if (lord.ownedPawns[j] == p)
				{
					return lord;
				}
			}
		}
		return null;
	}

	public Lord LordOf(Building b)
	{
		for (int i = 0; i < lords.Count; i++)
		{
			Lord lord = lords[i];
			for (int j = 0; j < lord.ownedBuildings.Count; j++)
			{
				if (lord.ownedBuildings[j] == b)
				{
					return lord;
				}
			}
		}
		return null;
	}

	public Lord LordOf(Corpse c)
	{
		for (int i = 0; i < lords.Count; i++)
		{
			Lord lord = lords[i];
			for (int j = 0; j < lord.ownedCorpses.Count; j++)
			{
				if (lord.ownedCorpses[j] == c)
				{
					return lord;
				}
			}
		}
		return null;
	}

	private void Notify_BuildingSpawned(Building b)
	{
		for (int i = 0; i < lords.Count; i++)
		{
			lords[i].Notify_BuildingSpawnedOnMap(b);
		}
		BreachingGridDebug.Notify_BuildingStateChanged(b);
	}

	private void Notify_BuildingDespawned(Building b)
	{
		for (int i = 0; i < lords.Count; i++)
		{
			lords[i].Notify_BuildingDespawnedOnMap(b);
		}
		BreachingGridDebug.Notify_BuildingStateChanged(b);
	}

	public void Notify_MapRemoved()
	{
		for (int num = lords.Count - 1; num >= 0; num--)
		{
			lords[num].Notify_MapRemoved();
		}
		for (int num2 = lords.Count - 1; num2 >= 0; num2--)
		{
			RemoveLord(lords[num2]);
		}
	}

	public void LogLords()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("======= Lords =======");
		stringBuilder.AppendLine("Count: " + lords.Count);
		for (int i = 0; i < lords.Count; i++)
		{
			Lord lord = lords[i];
			stringBuilder.AppendLine();
			stringBuilder.Append("#" + (i + 1) + ": ");
			if (lord.LordJob == null)
			{
				stringBuilder.AppendLine("no-job");
			}
			else
			{
				stringBuilder.AppendLine(lord.LordJob.GetType().Name);
			}
			stringBuilder.Append("Current toil: ");
			if (lord.CurLordToil == null)
			{
				stringBuilder.AppendLine("null");
			}
			else
			{
				stringBuilder.AppendLine(lord.CurLordToil.GetType().Name);
			}
			stringBuilder.AppendLine("Members (count: " + lord.ownedPawns.Count + "):");
			for (int j = 0; j < lord.ownedPawns.Count; j++)
			{
				stringBuilder.AppendLine("  " + lord.ownedPawns[j].LabelShort + " (" + lord.ownedPawns[j].Faction?.ToString() + ")");
			}
		}
		Log.Message(stringBuilder.ToString());
	}

	public void Dispose()
	{
		foreach (Lord lord in lords)
		{
			lord.Dispose();
		}
	}
}
