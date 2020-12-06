using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Origine
{
    public abstract partial class BaseUI 
    {
        private void AfterShowComponent()
        {
            foreach (var component in _componentList)
            {
                if (component is UIComponent comp) comp.AfterShow();
            }
        }

        private void BeforeCloseComponent()
        {
            foreach (var component in _componentList)
            {
                if (component is UIComponent comp) comp.BeforeClose();
            }
        }

        private void BeforeDestoryComponent()
        {
            foreach (var component in _componentList)
            {
                if (component is UIComponent comp) comp.BeforeDestroy();
            }
        }
    }
}
