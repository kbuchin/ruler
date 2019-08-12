namespace General.Controller
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    interface IController
    {
        void InitLevel();
        void CheckSolution();
        void AdvanceLevel();
    }
}
