// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty

namespace DotsAndPolygons
{
    using static HelperFunctions;

    public class HeadlessDotsController1 : HeadlessDotsController
    {
        public HeadlessDotsController1(
            DotsPlayer player1,
            DotsPlayer player2,
            int numberOfDots = 20
        ) : base(player1, player2, numberOfDots)
        {
        }
        
        public override GameMode CurrentGamemode => GameMode.GameMode1;

        public override bool CheckSolutionOfGameState()
        {
            if (CheckHull())
            {
                FinishLevel();
                return true;
            }
            return false;
        }

        public override void InitLevel()
        {
            base.InitLevel();

            AddDotsInGeneralPosition();
        }
    }
}