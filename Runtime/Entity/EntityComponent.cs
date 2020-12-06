using System;
using System.Collections;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Origine
{
    public abstract class EntityComponent : IReference
    {
        public bool IsDestroy { get; protected set; }

        private bool enabled;

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (value && !enabled) OnEnable();
                else if (!value && enabled) OnDisable();
                enabled = value;
            }
        }

        protected virtual void OnEnable()
        {

        }

        protected virtual void OnDisable()
        {

        }

        public virtual void OnInit(Entity gameEntity)
        {
            IsDestroy = false;
        }

        public virtual void OnUpdate(float deltaTime)
        {

        }

        public virtual void OnDestroy()
        {
            enabled = false;
            IsDestroy = true;
        }
    }
}