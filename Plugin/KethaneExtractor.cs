/// FissionGenerator
/// ---------------------------------------------------
/// FissionGeenrator part module

/// TODO: Figure out how to refresh UI widgets for deploy/retract radiators

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Kethane
{
    public class KethaneExtractor : PartModule
    {
        // Implement the fissiongeneratoranimator class
        private class DefaultExtractorAnimator : IExtractorAnimator
        {
            public ExtractorState CurrentState { get; private set; }
            public void Deploy() { CurrentState = ExtractorState.Deployed; }
            public void Retract() { CurrentState = ExtractorState.Retracted; }

            public DefaultExtractorAnimator()
            {
                CurrentState = ExtractorState.Retracted;
            }
        }

        private IExtractorAnimator animator;

        // Is generator online
        [KSPField(isPersistant = true)]
        public bool Enabled;

        // Power generation when closed and open
        [KSPField(isPersistant = false)]
        public float PowerGenerationDeployed;
        [KSPField(isPersistant = false)]
        public float PowerGenerationRetracted;
        [KSPField(isPersistant = false)]
        public float PowerGenerationResponseRate;

        // Radiator Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Status")]
        public string Status;

        // current generation
        public float currentGeneration;

        // Reactor activation actions
        [KSPEvent(guiActive = true, guiName = "Enable Reactor", active = true)]
        public void Enable()
        {

            Enabled = true;
        }
        [KSPEvent(guiActive = true, guiName = "Disable Reactor", active = false)]
        public void Disable()
        {
            GeneratorStatus = "Reactor Offline";
            Enabled = false;
        }
        [KSPAction("Enable Reactor")]
        public void EnableAction(KSPActionParam param) { Enable(); }

        [KSPAction("Disable Reactor")]
        public void DisableAction(KSPActionParam param) { Disable(); }

        [KSPAction("Toggle Reactor")]
        public void ToggleAction(KSPActionParam param)
        {
            Enabled = !Enabled;
        }

        // Reactor Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Output")]
        public string GeneratorStatus;

        public override void OnStart(PartModule.StartState state)
        {
            this.part.force_activate();
            animator = part.Modules.OfType<IExtractorAnimator>().SingleOrDefault() ?? new KethaneDrillAnimator();

            // Figure out what the current production should be
            if (Enabled)
            {
                if (animator.CurrentState != ExtractorState.Deployed)
                {
                    currentGeneration = PowerGenerationRetracted;
                }
                else
                {
                    currentGeneration = PowerGenerationDeployed;
                }
            }
            else
            {
                currentGeneration = 0f;
            }

        }

        public override void OnLoad(ConfigNode node)
        { }

        // Radiator Actions
        [KSPEvent(guiActive = true, guiName = "Deploy Drill", active = true)]
        public void DeployDrill()
        {
            animator.Deploy();
        }

        [KSPEvent(guiActive = true, guiName = "Retract Drill", active = false)]
        public void RetractDrill()
        {
            animator.Retract();
        }

        [KSPAction("Deploy Drill")]
        public void DeployDrillAction(KSPActionParam param)
        {
            DeployDrill();
        }

        [KSPAction("Retract Drill")]
        public void RetractDrillAction(KSPActionParam param)
        {
            RetractDrill();
        }

        [KSPAction("Toggle Drill")]
        public void ToggleDrillAction(KSPActionParam param)
        {
            if (animator.CurrentState == ExtractorState.Deployed || animator.CurrentState == ExtractorState.Deploying)
            {
                RetractDrill();
            }
            else if (animator.CurrentState == ExtractorState.Retracted || animator.CurrentState == ExtractorState.Retracting)
            {
                DeployDrill();
            }
        }

        // Info for ui
        public override string GetInfo()
        {
            return String.Format("Maximum Power: {0:F2}/s", currentGeneration);
        }

        // Update function for animation, UI
        public override void OnUpdate()
        {
            Events["Enable"].active = !Enabled;
            Events["Disable"].active = Enabled;

            var retracted = (animator.CurrentState == ExtractorState.Retracted);
            var deployed = (animator.CurrentState == ExtractorState.Deployed);

            if (Events["DeployDrill"].active != retracted || Events["RetractDrill"].active != deployed)
            {
                Events["DeployDrill"].active = retracted;
                Events["RetractDrill"].active = deployed;
            }
            Status = animator.CurrentState.ToString();

            // Update GUI 
            GeneratorStatus = String.Format("Generation rate: {0:F2}/s", currentGeneration);

        }

        // Fixed update function. Actually does the gameplay stuff
        public override void OnFixedUpdate()
        {
            if (Enabled)
            {
                // if radiators are not open, move towards closed generation at response rate
                if (animator.CurrentState != ExtractorState.Deployed)
                {
                    currentGeneration = Mathf.MoveTowards(currentGeneration, PowerGenerationRetracted, TimeWarp.fixedDeltaTime * PowerGenerationResponseRate);
                    this.part.RequestResource("ElectricCharge", -TimeWarp.fixedDeltaTime * currentGeneration);
                }
                else
                {
                    currentGeneration = Mathf.MoveTowards(currentGeneration, PowerGenerationDeployed, TimeWarp.fixedDeltaTime * PowerGenerationResponseRate);
                    this.part.RequestResource("ElectricCharge", -TimeWarp.fixedDeltaTime * currentGeneration);
                }

            }
            else
            {
                currentGeneration = Mathf.MoveTowards(currentGeneration, 0f, TimeWarp.fixedDeltaTime * PowerGenerationResponseRate);
                this.part.RequestResource("ElectricCharge", -TimeWarp.fixedDeltaTime * currentGeneration);

            }
        }
    }
}
