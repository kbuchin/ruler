using NUnit.Framework;

namespace DotsAndPolygons.Tests.SoCG
{
    public class Headless
    {
        [Test]
        public void Test1()
        {
            HeadlessDotsController1 gamemode1 = new HeadlessDotsController1(
                new MinMaxAi(PlayerNumber.Player1, HelperFunctions.GameMode.GameMode1),
                new MinMaxAi(PlayerNumber.Player2, HelperFunctions.GameMode.GameMode1),
                10
            );

            gamemode1.Start();

            HelperFunctions.print($"Player1 area: {gamemode1.TotalAreaP1}, Player2 area: {gamemode1.TotalAreaP2}",
                true);
        }
    }
}