namespace StandardUnitTest
{
    public class BitHelperUnitTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void SetBit()
        {
            byte[] data1 = [0x00,0x00];
            Standard.Helpers.BitHelper.SetBit(data1, 8, true);
            Assert.That(data1[1], Is.EqualTo(0x01));

            byte data2 = 0x00;
            data2 = Standard.Helpers.BitHelper.SetBit(data2, 0, true);
            Assert.That(data2, Is.EqualTo(0x01));
        }
    }
}