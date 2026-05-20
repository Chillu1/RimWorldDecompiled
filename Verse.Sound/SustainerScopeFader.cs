namespace Verse.Sound;

public class SustainerScopeFader
{
	public bool inScope = true;

	public float inScopePercent = 1f;

	private const float ScopeMatchFallRate = 0.03f;

	private const float ScopeMatchRiseRate = 0.05f;

	public void SustainerScopeUpdate()
	{
		if (inScope)
		{
			float num = inScopePercent + 0.05f;
			inScopePercent = num;
			if (inScopePercent > 1f)
			{
				inScopePercent = 1f;
			}
		}
		else
		{
			inScopePercent -= 0.03f;
			if (inScopePercent <= 0.001f)
			{
				inScopePercent = 0f;
			}
		}
	}
}
