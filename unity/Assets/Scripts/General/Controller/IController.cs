namespace General.Controller
{
    interface IController
    {
        /// <summary>
        /// Called to initialize a new level
        /// </summary>
        void InitLevel();

        /// <summary>
        /// Check whether current level is solved
        /// </summary>
        void CheckSolution();

        /// <summary>
        /// Advance to the next level
        /// </summary>
        void AdvanceLevel();
    }
}
