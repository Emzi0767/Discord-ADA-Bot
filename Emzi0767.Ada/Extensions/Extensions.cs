using System;
using System.Linq;
using System.Reflection;

namespace Emzi0767.Ada.Extensions
{
    public static class Extensions
    {
        public static string ToSizeString(this long l)
        {
            var d = (double)l;
            var i = 0;
            var u = new string[] { "", "k", "M", "G", "T" };
            while (d >= 900)
            {
                d /= 1024D;
                i++;
            }
            return string.Format("{0:#,##0.00} {1}B", d, u[i]);
        }

        public static string ToSizeString(this int l)
        {
            var d = (double)l;
            var i = 0;
            var u = new string[] { "", "k", "M", "G", "T" };
            while (d >= 900)
            {
                d /= 1024D;
                i++;
            }
            return string.Format("{0:#,##0.00} {1}B", d, u[i]);
        }

        public static string ToPointerString(this IntPtr ptr)
        {
            var i32 = ptr.ToInt32();
            var i64 = ptr.ToInt64();
            var pst = (string)null;
            //if (Environment.Is64BitOperatingSystem)
            if (IntPtr.Size == 8)
                pst = string.Concat("0x", i64.ToString("X16"));
            else
                pst = string.Concat("0x", i32.ToString("X8"));
            return pst;
        }

        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.GetTypeInfo().IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.GetTypeInfo().IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            var baseType = givenType.GetTypeInfo().BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }

        public static bool HasParentType(this Type type, Type parent_type)
        {
            var bt = type.GetTypeInfo().BaseType;

            while (bt != null)
            {
                if (bt == parent_type)
                    return true;

                bt = bt.GetTypeInfo().BaseType;
            }

            return false;
        }
    }
}
