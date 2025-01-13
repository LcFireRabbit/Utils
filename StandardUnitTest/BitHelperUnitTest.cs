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

        [Test]
        public void GetBit()
        {
            byte[] data1 = [0x00, 0x00];
            bool result = Standard.Helpers.BitHelper.GetBit(data1, 8);
            Assert.That(result, Is.EqualTo(false));

            byte data2 = 0x00;
            result = Standard.Helpers.BitHelper.GetBit(data2, 0);
            Assert.That(result, Is.EqualTo(false));
        }
    }
}