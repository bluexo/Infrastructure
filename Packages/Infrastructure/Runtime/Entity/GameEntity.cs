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
        public bool IgnoreCase { get; set; } = true;

        public AutoRefAttribute() { }
        public AutoRefAttribute(string name) => Name = name;
    }

    public partial class GameEntity : Entity
    {
        public GameObject Self { get; protected set; }
        public Transform Transform => Self ? Self.transform : null;

        protected virtual void CollectAutoReferences(GameObject root = null)
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

            var binders = fields.Select(f => (field: f, bind: f.GetCustomAttribute<AutoRefAttribute>()))
                .Where(b => b.bind != null && b.field.FieldType.IsSubclassOf(typeof(Component)));

            foreach (var (field, bind) in binders)
            {
                var bindName = string.IsNullOrWhiteSpace(bind.Name) ? field.Name : bind.Name;
                var child = root ? Utility.Find(root, bindName) : GameObject.Find(bindName);

                if (bind.IgnoreCase && !child)
                    child = root ? Utility.FindByTitleCase(root, bindName) : GameObject.Find(bindName.TitleCase());

                if (!child)
                {
                    Debug.LogError($"Cannot found child {nameof(GameObject)}={field.Name} from {root.name}");
                    continue;
                }

                var comp = child.GetComponent(field.FieldType);
                if (!comp)
                {
                    Debug.LogError($"Cannot found component {field.FieldType}:{field.Name} from {child.name}, RefName={bind.Name}");
                    continue;
                }
                field.SetValue(this, comp);
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            compositeDisposables.Dispose();
            GameObject.Destroy(Self);
        }
    }
}