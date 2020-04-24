using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse.AI.Group
{
	public sealed class LordManager : IExposable
	{
		public Map map;

		public List<Lord> lords = new List<Lord>();

		public LordManager(Map map)
		{
			this.map = map;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref lords, "lords", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				for (int i = 0; i < lords.Count; i++)
				{
					lords[i].lordManager = this;
				}
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
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
				catch (Exception ex)
				{
					Lord lord = lords[i];
					Log.Error(string.Format("Exception while ticking lord with job {0}: \r\n{1}", (lord == null) ? "NULL" : lord.LordJob.ToString(), ex.ToString()));
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
			if (DebugViewSettings.drawDuties)
			{
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
							text = text + "\nMentalState=" + allPawn.MentalState.ToString();
						}
						Vector2 vector = allPawn.DrawPos.MapToUIPosition();
						Widgets.Label(new Rect(vector.x - 100f, vector.y - 100f, 200f, 200f), text);
					}
				}
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}

		public void AddLord(Lord newLord)
		{
			lords.Add(newLord);
			newLord.lordManager = this;
			Find.SignalManager.RegisterReceiver(newLord);
		}

		public void RemoveLord(Lord oldLord)
		{
			lords.Remove(oldLord);
			Find.SignalManager.DeregisterReceiver(oldLord);
			oldLord.Cleanup();
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
					stringBuilder.AppendLine("  " + lord.ownedPawns[j].LabelShort + " (" + lord.ownedPawns[j].Faction + ")");
				}
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
