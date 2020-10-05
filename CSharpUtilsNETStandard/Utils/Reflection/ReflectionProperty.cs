#region Imports

using System;
using System.Reflection;
using CSharpUtilsNETStandard.Utils.Extensions.General;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Reflection
{
    /// <summary>
    /// Used for caching reflection property
    /// </summary>
    /// <typeparam name="TReturn">The type of the property</typeparam>
    [PublicAPI]
    public sealed class ReflectionProperty<TReturn>
    {
        [NotNull]
        private readonly string _propertyName;

        [NotNull]
        private readonly Type _instanceType;

        [CanBeNull]
        private readonly TReturn _defaultReturnValue;

        [CanBeNull]
        private readonly PropertyInfo _property;

        private readonly bool _canRead;

        private readonly bool _canWrite;

        public ReflectionProperty([NotNull] string propertyName, [NotNull] Type instanceType, [CanBeNull] TReturn defaultReturnValue = default)
        {
            _propertyName = propertyName;
            _instanceType = instanceType;
            _defaultReturnValue = defaultReturnValue;
            _property = _instanceType.GetProperty(propertyName, BindingFlags.Public);
            if (_property == null) return;
            _canRead = _property.CanRead;
            _canWrite = _property.CanWrite;
        }

        [CanBeNull]
        public TReturn GetValue([NotNull] object instance)
        {
            if (_property == null) return ReturnFailure("Property does not exist!");
            if (!_instanceType.IsInstanceOfType(instance)) return ReturnFailure("Instance is not of the right type!");
            if (!_canRead) return ReturnFailure("Property cannot be read!");
            return _property.GetValue(instance).CastOrDefaultAllowNull(_defaultReturnValue);
        }

        public void SetValue([NotNull] object instance, [CanBeNull] TReturn value)
        {
            if (_property == null)
            {
                ReturnFailure("Property does not exist!");
                return;
            }
            if (!_instanceType.IsInstanceOfType(instance))
            {
                ReturnFailure("Instance is not of the right type!");
                return;
            }
            if (!_canWrite)
            {
                ReturnFailure("Property cannot be read!");
                return;
            }
            _property.SetValue(instance, value);
        }

        [CanBeNull]
        private TReturn ReturnFailure([NotNull] string msg)
        {
            Logger.PrintWarning($"Reflection failed! (Type: {_instanceType.Name}, Property: {_propertyName})\nReason: {msg}");
            return _defaultReturnValue;
        }
    }
}
