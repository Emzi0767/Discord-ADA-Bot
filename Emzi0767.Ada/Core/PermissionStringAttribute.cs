using System;

namespace Emzi0767.Ada.Core
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PermissionStringAttribute : Attribute
    {
        public string String { get; private set; }

        public PermissionStringAttribute(string str)
        {
            this.String = str;
        }
    }
}
