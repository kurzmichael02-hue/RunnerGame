using NUnit.Framework;

[TestFixture]
public class AudioUtilsTests
{
	// =========================
	// PercentToDb basic cases
	// =========================

	[Test]
	public void PercentToDb_100Percent_IsZeroDb()
	{
		float result = AudioUtils.PercentToDb(100f);

		Assert.That(result, Is.EqualTo(0f).Within(0.01f));
	}

	[Test]
	public void PercentToDb_0Percent_IsMuted()
	{
		float result = AudioUtils.PercentToDb(0f);

		Assert.That(result, Is.EqualTo(-80f));
	}

	[Test]
	public void PercentToDb_50Percent_IsNegative()
	{
		float result = AudioUtils.PercentToDb(50f);

		Assert.That(result, Is.LessThan(0f));
	}

	// =========================
	// Clamping tests
	// =========================

	[Test]
	public void PercentToDb_NegativeValue_IsClampedToMuted()
	{
		float result = AudioUtils.PercentToDb(-50f);

		Assert.That(result, Is.EqualTo(-80f));
	}

	[Test]
	public void PercentToDb_Over100_IsClampedToZeroDb()
	{
		float result = AudioUtils.PercentToDb(200f);

		Assert.That(result, Is.EqualTo(0f).Within(0.01f));
	}

	// =========================
	// DbToPercent tests
	// =========================

	[Test]
	public void DbToPercent_Muted_IsZero()
	{
		float result = AudioUtils.DbToPercent(-80f);

		Assert.That(result, Is.EqualTo(0f));
	}

	[Test]
	public void DbToPercent_ZeroDb_Is100Percent()
	{
		float result = AudioUtils.DbToPercent(0f);

		Assert.That(result, Is.EqualTo(100f).Within(0.01f));
	}

	[Test]
	public void DbToPercent_NegativeDb_IsBelow100()
	{
		float result = AudioUtils.DbToPercent(-10f);

		Assert.That(result, Is.LessThan(100f));
	}

	// =========================
	// Roundtrip consistency
	// =========================

	[Test]
	public void PercentToDb_And_Back_IsConsistent()
	{
		float input = 75f;

		float db = AudioUtils.PercentToDb(input);
		float back = AudioUtils.DbToPercent(db);

		Assert.That(back, Is.EqualTo(input).Within(2f));
	}

	[Test]
	public void PercentToDb_And_Back_At50Percent_IsStable()
	{
		float input = 50f;

		float db = AudioUtils.PercentToDb(input);
		float back = AudioUtils.DbToPercent(db);

		Assert.That(back, Is.EqualTo(input).Within(2f));
	}
}
