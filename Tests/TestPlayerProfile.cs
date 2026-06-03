using NUnit.Framework;

[TestFixture]
public class PlayerProfileTests
{
	// =========================
	// Character prices
	// =========================

	[Test]
	public void GetCharacterPrice_Default_IsFree()
	{
		int price = Player.GetCharacterPrice(0);

		Assert.That(price, Is.EqualTo(0));
	}

	[Test]
	public void GetCharacterPrice_Mischa_Costs100()
	{
		int price = Player.GetCharacterPrice(1);

		Assert.That(price, Is.EqualTo(100));
	}

	[Test]
	public void GetCharacterPrice_Tim_Costs250()
	{
		int price = Player.GetCharacterPrice(2);

		Assert.That(price, Is.EqualTo(250));
	}

	[Test]
	public void GetCharacterPrice_UnknownId_ReturnsMinusOne()
	{
		int price = Player.GetCharacterPrice(99);

		Assert.That(price, Is.EqualTo(-1));
	}

	[Test]
	public void GetCharacterPrice_NegativeId_ReturnsMinusOne()
	{
		int price = Player.GetCharacterPrice(-1);

		Assert.That(price, Is.EqualTo(-1));
	}
}
