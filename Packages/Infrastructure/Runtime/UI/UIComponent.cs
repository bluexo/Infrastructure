using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Origine
{
    public abstract class UIComponent : EntityComponent
    {
        public virtual void AfterShow()
        {
        }

        public virtual void BeforeClose()
        {
        }

        public virtual void BeforeDestroy()
        {
        }
    }
}
