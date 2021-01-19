using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Video;

namespace Origine
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AutoRefAttribute : Attribute
    {
        public string Name { get; set; }
        public string Parent { get; set; }
        public bool IgnoreCase { get; set; } = true;

        public AutoRefAttribute() { }
        public AutoRefAttribute(string name) => Name = name;
    }

    public partial class GameEntity : Entity
    {
        public GameObject Self { get; protected set; }
        public Transform Transform => Self ? Self.transform : null;
        public bool Initialized { get; protected set; }

        protected virtual void CollectAutoReferences(GameObject root = null)
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

            var binders = fields.Select(f =>
            {
                var bind = f.TryGetAttribute(out AutoRefAttribute autoRefAttribute);
                return (field: f, bind: autoRefAttribute);
            })
            .Where(b =>
            {
                var isSubClass = b.field.FieldType.IsSubclassOf(typeof(Component));
                return b.bind != null && isSubClass;
            });

            foreach (var (field, bind) in binders)
            {
                GameObject parent = null;
                if (!string.IsNullOrWhiteSpace(bind.Parent))
                    parent = Find(root, bind.Parent, bind.IgnoreCase);

                if (!parent) parent = root;

                var bindName = string.IsNullOrWhiteSpace(bind.Name) ? field.Name : bind.Name;
                var child = Find(parent, bindName, bind.IgnoreCase);
                if (!child) continue;

                var comp = child.GetComponent(field.FieldType);
                if (!comp)
                {
                    Debug.LogError($"Cannot found component {field.FieldType}:{field.Name} from {child.name}, RefName={bind.Name}");
                    continue;
                }
                field.SetValue(this, comp);
            }

            GameObject Find(GameObject parent, string name, bool ignoreCase)
            {
                var child = parent ? Utility.Find(parent, name) : GameObject.Find(name);

                if (ignoreCase && !child)
                    child = parent ? Utility.FindByTitleCase(parent, name) : GameObject.Find(name.TitleCase());

                if (!child)
                {
                    Debug.LogError($"Cannot found child {nameof(GameObject)}={name} from {parent.name}");
                    return null;
                }

                return child;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            compositeDisposables.Dispose();
            GameObject.Destroy(Self);
        }
    }
}