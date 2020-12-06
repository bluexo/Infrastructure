using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Origine
{
    public static class AnimatorExtensions
    {
        public static void SetTrigger<T>(this Animator animator, T t) where T : Enum
        {
            animator.SetTrigger(t.ToString());
        }

        public static void SetInteger<T>(this Animator animator, string paramName, T t) where T : Enum
        {
            animator.SetInteger(paramName, Convert.ToInt32(t));
        }

        public static void SetInteger<T>(this Animator animator, int hash, T t) where T : Enum
        {
            animator.SetInteger(hash, Convert.ToInt32(t));
        }
    }
}
