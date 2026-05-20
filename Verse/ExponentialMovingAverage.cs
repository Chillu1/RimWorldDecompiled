namespace Verse;

public class ExponentialMovingAverage
{
	private readonly float alpha;

	private float average;

	private bool initialized;

	public ExponentialMovingAverage(float alpha)
	{
		this.alpha = alpha;
	}

	public void AddValue(float value)
	{
		if (!initialized)
		{
			average = value;
			initialized = true;
		}
		else
		{
			average = alpha * value + (1f - alpha) * average;
		}
	}

	public float GetAverage()
	{
		return average;
	}

	public void Reset()
	{
		initialized = false;
		average = 0f;
	}
}
