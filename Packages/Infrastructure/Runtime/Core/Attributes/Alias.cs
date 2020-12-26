using System;

namespace Origine
{
    /// <summary>
    /// 别名
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class AliasAttribute : Attribute
    {
        public string Name { get; set; }

        public AliasAttribute(string name) => Name = name;
    }
}
