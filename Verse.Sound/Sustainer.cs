using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound
{
	public class Sustainer
	{
		public SoundDef def;

		public SoundInfo info;

		internal GameObject worldRootObject;

		private int lastMaintainTick;

		private int lastMaintainFrame;

		private float endRealTime = -1f;

		private List<SubSustainer> subSustainers = new List<SubSustainer>();

		public SoundParams externalParams = new SoundParams();

		public SustainerScopeFader scopeFader = new SustainerScopeFader();

		public bool Ended => endRealTime >= 0f;

		public float TimeSinceEnd => Time.realtimeSinceStartup - endRealTime;

		public float CameraDistanceSquared
		{
			get
			{
				if (info.IsOnCamera)
				{
					return 0f;
				}
				if (worldRootObject == null)
				{
					if (Prefs.DevMode)
					{
						Log.Error(string.Concat("Sustainer ", def, " info is ", info, " but its worldRootObject is null"));
					}
					return 0f;
				}
				return (Find.CameraDriver.MapPosition - worldRootObject.transform.position.ToIntVec3()).LengthHorizontalSquared;
			}
		}

		public Sustainer(SoundDef def, SoundInfo info)
		{
			this.def = def;
			this.info = info;
			if (def.subSounds.Count > 0)
			{
				foreach (KeyValuePair<string, float> definedParameter in info.DefinedParameters)
				{
					externalParams[definedParameter.Key] = definedParameter.Value;
				}
				if (def.HasSubSoundsInWorld)
				{
					if (info.IsOnCamera)
					{
						Log.Error("Playing sound " + def.ToString() + " on camera, but it has sub-sounds in the world.");
					}
					worldRootObject = new GameObject("SustainerRootObject_" + def.defName);
					UpdateRootObjectPosition();
				}
				else if (!info.IsOnCamera)
				{
					info = SoundInfo.OnCamera(info.Maintenance);
				}
				Find.SoundRoot.sustainerManager.RegisterSustainer(this);
				if (!info.IsOnCamera)
				{
					Find.SoundRoot.sustainerManager.UpdateAllSustainerScopes();
				}
				for (int i = 0; i < def.subSounds.Count; i++)
				{
					subSustainers.Add(new SubSustainer(this, def.subSounds[i]));
				}
			}
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				lastMaintainTick = Find.TickManager.TicksGame;
				lastMaintainFrame = Time.frameCount;
			});
		}

		public void SustainerUpdate()
		{
			if (!Ended)
			{
				if (info.Maintenance == MaintenanceType.PerTick)
				{
					if (Find.TickManager.TicksGame > lastMaintainTick + 1)
					{
						End();
						return;
					}
				}
				else if (info.Maintenance == MaintenanceType.PerFrame && Time.frameCount > lastMaintainFrame + 1)
				{
					End();
					return;
				}
			}
			else if (TimeSinceEnd > def.sustainFadeoutTime)
			{
				Cleanup();
			}
			if (def.subSounds.Count > 0)
			{
				if (!info.IsOnCamera && info.Maker.HasThing)
				{
					UpdateRootObjectPosition();
				}
				scopeFader.SustainerScopeUpdate();
				for (int i = 0; i < subSustainers.Count; i++)
				{
					subSustainers[i].SubSustainerUpdate();
				}
			}
		}

		private void UpdateRootObjectPosition()
		{
			if (worldRootObject != null)
			{
				worldRootObject.transform.position = info.Maker.Cell.ToVector3ShiftedWithAltitude(0f);
			}
		}

		public void Maintain()
		{
			if (Ended)
			{
				Log.Error("Tried to maintain ended sustainer: " + def);
			}
			else if (info.Maintenance == MaintenanceType.PerTick)
			{
				lastMaintainTick = Find.TickManager.TicksGame;
			}
			else if (info.Maintenance == MaintenanceType.PerFrame)
			{
				lastMaintainFrame = Time.frameCount;
			}
		}

		public void End()
		{
			endRealTime = Time.realtimeSinceStartup;
			if (def.sustainFadeoutTime < 0.001f)
			{
				Cleanup();
			}
		}

		private void Cleanup()
		{
			if (def.subSounds.Count > 0)
			{
				Find.SoundRoot.sustainerManager.DeregisterSustainer(this);
				for (int i = 0; i < subSustainers.Count; i++)
				{
					subSustainers[i].Cleanup();
				}
			}
			if (def.sustainStopSound != null)
			{
				if (worldRootObject != null)
				{
					Map map = info.Maker.Map;
					if (map != null)
					{
						SoundInfo soundInfo = SoundInfo.InMap(new TargetInfo(worldRootObject.transform.position.ToIntVec3(), map));
						def.sustainStopSound.PlayOneShot(soundInfo);
					}
				}
				else
				{
					def.sustainStopSound.PlayOneShot(SoundInfo.OnCamera());
				}
			}
			if (worldRootObject != null)
			{
				Object.Destroy(worldRootObject);
			}
			DebugSoundEventsLog.Notify_SustainerEnded(this, info);
		}

		public string DebugString()
		{
			string defName = def.defName;
			defName = defName + "\n  inScopePercent=" + scopeFader.inScopePercent;
			defName = defName + "\n  CameraDistanceSquared=" + CameraDistanceSquared;
			foreach (SubSustainer subSustainer in subSustainers)
			{
				defName = defName + "\n  sub: " + subSustainer;
			}
			return defName;
		}
	}
}
