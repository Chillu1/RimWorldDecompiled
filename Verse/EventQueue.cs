using UnityEngine;

namespace Verse;

public class EventQueue
{
	public float Queued { get; private set; }

	public float Threshold { get; private set; }

	public EventQueue(float threshold)
	{
		Threshold = threshold;
	}

	public void Push(float amount)
	{
		Queued += amount;
	}

	public bool Peek()
	{
		return Queued >= Threshold;
	}

	public bool Pop()
	{
		if (Queued >= Threshold)
		{
			Queued -= Threshold;
			return true;
		}
		return false;
	}

	public int PopAll()
	{
		int result = Mathf.FloorToInt(Queued / Threshold);
		Queued %= Threshold;
		return result;
	}

	public int Clear()
	{
		int result = Mathf.FloorToInt(Queued / Threshold);
		Queued = 0f;
		return result;
	}
}
