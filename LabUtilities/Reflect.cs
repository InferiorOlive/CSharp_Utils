using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LivePersonNet.Utilities
{
    public static class Reflect
    {

        /// <summary>
        /// Represents a field or property that we can read/write.
        /// </summary>
        public interface IAccessor
        {
            void SetValue(object target, object value);
            object GetValue(object target);
            Type GetValueType();
        }

        public static IAccessor GetAccessor(Type type, string name)
        {
            return type.GetMember(name)
                .Select<MemberInfo, IAccessor>(m =>
                {
                    switch (m)
                    {
                        case PropertyInfo p:
                            return new PropertyAccessor(p);
                        case FieldInfo f:
                            return new FieldAccessor(f);
                        default:
                            return null;
                    }
                })
                .FirstOrDefault(a => a != null);
        }

        public static IAccessor GetAccessor<T>(string name)
        {
            return GetAccessor(typeof(T), name);
        }


        private class PropertyAccessor : IAccessor
        {
            private readonly PropertyInfo _property;

            internal PropertyAccessor(PropertyInfo property)
            {
                _property = property;
            }

            public void SetValue(object target, object value)
            {
                _property.SetValue(target, value);
            }

            public object GetValue(object target)
            {
                return _property.GetValue(target);
            }

            public Type GetValueType()
            {
                return _property.PropertyType;
            }
        }

        private class FieldAccessor : IAccessor
        {
            private readonly FieldInfo _field;

            internal FieldAccessor(FieldInfo field)
            {
                _field = field;
            }

            public void SetValue(object target, object value)
            {
                _field.SetValue(target, value);
            }

            public object GetValue(object target)
            {
                return _field.GetValue(target);
            }

            public Type GetValueType()
            {
                return _field.FieldType;
            }
        }
    }
}
