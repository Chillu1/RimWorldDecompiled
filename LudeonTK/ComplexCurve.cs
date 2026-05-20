using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace LudeonTK;

public class ComplexCurve : IEquatable<ComplexCurve>
{
	private List<UnityEngine.Keyframe> keyframes;

	private WrapMode preWrapMode = WrapMode.ClampForever;

	private WrapMode postWrapMode = WrapMode.ClampForever;

	[Unsaved(false)]
	private AnimationCurve curve;

	public UnityEngine.Keyframe[] Keys
	{
		get
		{
			return curve.keys;
		}
		set
		{
			curve.keys = value;
			keyframes = value.ToList();
		}
	}

	public int Length => curve.length;

	public WrapMode PreWrapMode
	{
		get
		{
			return curve.preWrapMode;
		}
		set
		{
			curve.preWrapMode = value;
		}
	}

	public WrapMode PostWrapMode
	{
		get
		{
			return curve.postWrapMode;
		}
		set
		{
			curve.postWrapMode = value;
		}
	}

	public static ComplexCurve LinearNormalized => new ComplexCurve(AnimationCurve.Linear(0f, 0f, 1f, 1f));

	public ComplexCurve()
	{
	}

	public ComplexCurve(params UnityEngine.Keyframe[] keyframes)
	{
		curve = new AnimationCurve(keyframes);
	}

	public ComplexCurve(AnimationCurve curve)
	{
		this.curve = curve;
	}

	public float Evaluate(float time)
	{
		return curve.Evaluate(time);
	}

	public int AddKey(float time, float value)
	{
		return curve.AddKey(time, value);
	}

	public int AddKey(UnityEngine.Keyframe keyframe)
	{
		return curve.AddKey(keyframe);
	}

	public int MoveKey(int index, UnityEngine.Keyframe keyframe)
	{
		return curve.MoveKey(index, keyframe);
	}

	public void RemoveKey(int index)
	{
		curve.RemoveKey(index);
	}

	public void SmoothTangents(int index, float weight)
	{
		curve.SmoothTangents(index, weight);
	}

	public static ComplexCurve Constant(float timeStart, float timeEnd, float value)
	{
		return new ComplexCurve(AnimationCurve.Constant(timeStart, timeEnd, value));
	}

	public static ComplexCurve Linear(float timeStart, float valueStart, float timeEnd, float valueEnd)
	{
		return new ComplexCurve(AnimationCurve.Linear(timeStart, valueStart, timeEnd, valueEnd));
	}

	public static ComplexCurve EaseInOut(float timeStart, float valueStart, float timeEnd, float valueEnd)
	{
		return new ComplexCurve(AnimationCurve.EaseInOut(timeStart, valueStart, timeEnd, valueEnd));
	}

	public static ComplexCurve Empty()
	{
		return new ComplexCurve(new AnimationCurve());
	}

	public AnimationCurve GetInternalCurveCopy()
	{
		return new AnimationCurve(curve.keys)
		{
			preWrapMode = curve.preWrapMode,
			postWrapMode = curve.postWrapMode
		};
	}

	public void PostLoad()
	{
		curve = new AnimationCurve
		{
			preWrapMode = preWrapMode,
			postWrapMode = postWrapMode,
			keys = keyframes?.ToArray()
		};
	}

	public override bool Equals(object o)
	{
		if (o == null)
		{
			return false;
		}
		if (this == o)
		{
			return true;
		}
		if (o.GetType() == GetType())
		{
			return Equals((ComplexCurve)o);
		}
		return false;
	}

	public bool Equals(ComplexCurve other)
	{
		if (other != null)
		{
			return curve.Equals(other.curve);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return curve.GetHashCode();
	}
}
