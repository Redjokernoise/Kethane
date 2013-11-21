/// IFissionGeneratorAnimator
/// ---------------------------------------------------
/// Interface for the animator of a fission generator
/// Implement to make these things look right
/// 
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kethane
{
    // State defining whether radiators are retracted
    public enum ExtractorState
    {
        Deployed,
        Deploying,
        Retracted,
        Retracting,
    }

    public interface IExtractorAnimator
    {
        ExtractorState CurrentState { get; }
        void Deploy();
        void Retract();
    }
}
