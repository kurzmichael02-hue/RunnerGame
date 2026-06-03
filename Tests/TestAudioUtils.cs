using NUnit.Framework;

[TestFixture]
public class AudioUtilsTests
{
	[Test]
	public void PercentToDb_100Percent_IsZeroDb()
	{
		// Arrange
		float input = 100f;

		// Act
		float result = AudioUtils.PercentToDb(input);

		// Assert
		Assert.That(result,
			Is.EqualTo(0f).Within(0.01f));
	}

	[Test]
	public void PercentToDb_0Percent_IsMuted()
	{
		// Arrange
		float input = 0f;

		// Act
		float result = AudioUtils.PercentToDb(input);

		// Assert
		Assert.That(result,
			Is.EqualTo(-80f));
	}

	[Test]
	public void PercentToDb_50Percent_IsNegative()
	{
		// Arrange
		float input = 50f;

		// Act
		float result = AudioUtils.PercentToDb(input);

		// Assert
		Assert.That(result,
			Is.LessThan(0f));
	}
}
