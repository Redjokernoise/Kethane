/// FissionGeneratorAnimator
/// ---------------------------------------------------
/// Module defining an animation module for a fission generator

/// TODO: Add heat animation for deployed, retracted states

using System;
using System.Linq;
using UnityEngine;

namespace Kethane
{
    public class KethaneDrillAnimator : PartModule, IExtractorAnimator
    {
        //
        [KSPField(isPersistant = false)]
        public string DeployAnimation;

        [KSPField(isPersistant = true)]
        public string State;

        private AnimationState[] deployStates;

        public override void OnStart(PartModule.StartState state)
        {
            deployStates = Misc.SetUpAnimation(DeployAnimation, this.part);


            if (CurrentState == ExtractorState.Deploying) { CurrentState = ExtractorState.Retracted; }
            else if (CurrentState == ExtractorState.Retracting) { CurrentState = ExtractorState.Deployed; }

            if (CurrentState == ExtractorState.Deployed)
            {
                foreach (AnimationState deployState in deployStates)
                {
                    deployState.normalizedTime = 1;
                }
            }


        }

        public ExtractorState CurrentState
        {
            get
            {
                try
                {
                    return (ExtractorState)Enum.Parse(typeof(ExtractorState), State);
                }
                catch
                {
                    CurrentState = ExtractorState.Retracted;
                    return CurrentState;
                }
            }
            private set
            {

                State = Enum.GetName(typeof(ExtractorState), value);
            }
        }

        public void Deploy()
        {
            if (CurrentState != ExtractorState.Retracted) { return; }
            CurrentState = ExtractorState.Deploying;

            foreach (var state in deployStates)
            {
                state.speed = 1;
            }
        }

        public void Retract()
        {
            if (CurrentState != ExtractorState.Deployed) { return; }
            CurrentState = ExtractorState.Retracting;

            foreach (var state in deployStates)
            {
                state.speed = -1;
            }
        }

        public override void OnUpdate()
        {
            foreach (var deployState in deployStates)
            {
                deployState.normalizedTime = Mathf.Clamp01(deployState.normalizedTime);
            }

            if (CurrentState == ExtractorState.Deploying && deployStates[0].normalizedTime >= 1)
            {
                CurrentState = ExtractorState.Deployed;

            }
            else if (CurrentState == ExtractorState.Retracting && deployStates[0].normalizedTime >= 0)
            {
                CurrentState = ExtractorState.Retracted;
            }
        }
    }
}
