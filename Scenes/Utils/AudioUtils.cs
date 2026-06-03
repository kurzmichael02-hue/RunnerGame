using Godot;

public static class AudioUtils
{
	public static float PercentToDb(float percent)
	{
		float linear = percent / 100f;

		return linear <= 0.001f
			? -80f
			: Mathf.LinearToDb(linear);
	}
}
