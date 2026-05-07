using System;
using UnityEngine;

namespace Kaleidoscope2.Core
{
    public interface IKaleidoscopeModule
    {
        string ModuleId { get; }
        bool IsInitialized { get; }
        bool IsActive { get; }

        void Initialize(KaleidoscopeState runtimeState);
        void Activate();
        void Deactivate();
        void Tick(float deltaTime);
        bool CanHandle(KaleidoscopeCommand command);
        void HandleCommand(KaleidoscopeCommand command);
        void Validate();
        KaleidoscopeModuleStatus GetStatus();
    }

    [Serializable]
    public struct KaleidoscopeModuleStatus
    {
        public string ModuleId;
        public bool IsInitialized;
        public bool IsActive;
        public string Message;

        public KaleidoscopeModuleStatus(string moduleId, bool isInitialized, bool isActive, string message)
        {
            ModuleId = moduleId;
            IsInitialized = isInitialized;
            IsActive = isActive;
            Message = message;
        }
    }

    public abstract class KaleidoscopeModuleBase : MonoBehaviour, IKaleidoscopeModule
    {
        [SerializeField] private bool startActive = true;

        private KaleidoscopeState state;

        public virtual string ModuleId
        {
            get { return GetType().Name; }
        }

        public bool IsInitialized { get; private set; }
        public bool IsActive { get; private set; }

        protected KaleidoscopeState State
        {
            get { return state; }
        }

        public virtual void Initialize(KaleidoscopeState runtimeState)
        {
            if (runtimeState == null)
            {
                Debug.LogError("[" + ModuleId + "] Cannot initialize without a KaleidoscopeState.");
                return;
            }

            state = runtimeState;
            IsInitialized = true;
            IsActive = startActive && isActiveAndEnabled;
            OnInitialized();
        }

        public void Activate()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[" + ModuleId + "] Cannot activate before initialization.");
                return;
            }

            if (IsActive)
            {
                return;
            }

            IsActive = true;
            OnActivated();
        }

        public void Deactivate()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            OnDeactivated();
        }

        public virtual void Tick(float deltaTime)
        {
        }

        public virtual bool CanHandle(KaleidoscopeCommand command)
        {
            return false;
        }

        public virtual void HandleCommand(KaleidoscopeCommand command)
        {
        }

        public virtual void Validate()
        {
        }

        public virtual KaleidoscopeModuleStatus GetStatus()
        {
            return CreateStatus(IsActive ? "Ready" : "Inactive");
        }

        protected virtual void OnInitialized()
        {
        }

        protected virtual void OnActivated()
        {
        }

        protected virtual void OnDeactivated()
        {
        }

        protected KaleidoscopeModuleStatus CreateStatus(string message)
        {
            return new KaleidoscopeModuleStatus(ModuleId, IsInitialized, IsActive, message);
        }

        protected void ReportWarning(string message)
        {
            if (state != null)
            {
                state.ReportWarning("[" + ModuleId + "] " + message);
                return;
            }

            Debug.LogWarning("[" + ModuleId + "] " + message);
        }

        protected void ReportMissingReference(string referenceName)
        {
            if (state != null)
            {
                state.ReportMissingReference(ModuleId + "." + referenceName);
                return;
            }

            Debug.LogWarning("[" + ModuleId + "] Missing reference: " + referenceName);
        }
    }
}
