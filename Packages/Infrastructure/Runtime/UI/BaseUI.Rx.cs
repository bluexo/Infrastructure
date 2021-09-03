using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.EventSystems;

using UniRx;
using UniRx.Triggers;

namespace Origine
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PreloadAttribute : Attribute
    {

    }

    public abstract class PresenterBaseUI<TPresenter> : BaseUI where TPresenter : PresenterBase
    {
       
    }
}
