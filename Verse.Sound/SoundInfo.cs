using System.Collections.Generic;

namespace Verse.Sound;

public struct SoundInfo
{
	private Dictionary<string, float> parameters;

	public float volumeFactor;

	public float pitchFactor;

	public bool testPlay;

	public bool forcedPlayOnCamera;

	public bool IsOnCamera { get; private set; }

	public TargetInfo Maker { get; private set; }

	public MaintenanceType Maintenance { get; private set; }

	public IEnumerable<KeyValuePair<string, float>> DefinedParameters
	{
		get
		{
			if (parameters == null)
			{
				yield break;
			}
			foreach (KeyValuePair<string, float> parameter in parameters)
			{
				yield return parameter;
			}
		}
	}

	public static SoundInfo OnCamera(MaintenanceType maint = MaintenanceType.None)
	{
		SoundInfo result = default(SoundInfo);
		result.IsOnCamera = true;
		result.Maintenance = maint;
		result.Maker = TargetInfo.Invalid;
		result.testPlay = false;
		result.volumeFactor = (result.pitchFactor = 1f);
		return result;
	}

	public static SoundInfo InMap(TargetInfo maker, MaintenanceType maint = MaintenanceType.None)
	{
		SoundInfo result = default(SoundInfo);
		result.IsOnCamera = false;
		result.Maintenance = maint;
		result.Maker = maker;
		result.testPlay = false;
		result.volumeFactor = (result.pitchFactor = 1f);
		return result;
	}

	public void SetParameter(string key, float value)
	{
		if (parameters == null)
		{
			parameters = new Dictionary<string, float>();
		}
		parameters[key] = value;
	}

	public static implicit operator SoundInfo(TargetInfo source)
	{
		return InMap(source);
	}

	public static implicit operator SoundInfo(Thing sourceThing)
	{
		return InMap(sourceThing);
	}

	public override string ToString()
	{
		string text = null;
		if (parameters != null && parameters.Count > 0)
		{
			text = "parameters=";
			foreach (KeyValuePair<string, float> parameter in parameters)
			{
				text = text + parameter.Key.ToString() + "-" + parameter.Value + " ";
			}
		}
		string text2 = null;
		if (Maker.HasThing || Maker.Cell.IsValid)
		{
			text2 = Maker.ToString();
		}
		string text3 = null;
		if (Maintenance != MaintenanceType.None)
		{
			text3 = ", Maint=" + Maintenance;
		}
		return "(" + (IsOnCamera ? "Camera" : "World from ") + text2 + text + text3 + ")";
	}
}
