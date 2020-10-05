#region Imports

using System;
using System.Reflection;
using CSharpUtilsNETStandard.Utils.Extensions.General;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETStandard.Utils.Reflection
{
    /// <summary>
    /// Used for caching reflection methods
    /// </summary>
    /// <typeparam name="TReturn">The return type of the method (use object in case of a void method)</typeparam>
    [PublicAPI]
    public sealed class ReflectionMethod<TReturn>
    {
        [NotNull]
        private readonly string _methodName;

        [NotNull]
        private readonly Type _instanceType;

        [CanBeNull]
        private readonly TReturn _defaultReturnValue;

        [NotNull]
        private readonly Type[] _parameters;

        [CanBeNull]
        private readonly MethodInfo _method;

        public ReflectionMethod([NotNull] string methodName, [NotNull] Type instanceType, [NotNull, ItemNotNull] Type[] parameters, [CanBeNull] TReturn defaultReturnValue = default)
        {
            _methodName = methodName;
            _instanceType = instanceType;
            _defaultReturnValue = defaultReturnValue;
            _parameters = parameters;
            _method = _instanceType.GetMethod(methodName, parameters);
        }

        public ReflectionMethod([NotNull] string methodName, [NotNull] Type instanceType, [NotNull, ItemNotNull] params Type[] parameters)
        {
            _methodName = methodName;
            _instanceType = instanceType;
            _defaultReturnValue = default;
            _parameters = parameters;
            _method = _instanceType.GetMethod(methodName, parameters);
        }

        public ReflectionMethod([NotNull] string methodName, [NotNull] Type instanceType, [CanBeNull] TReturn defaultReturnValue = default)
        {
            _methodName = methodName;
            _instanceType = instanceType;
            _defaultReturnValue = defaultReturnValue;
            _parameters = new Type[0];
            _method = _instanceType.GetMethod(methodName);
        }

        [CanBeNull]
        public TReturn InvokeWithReturn([NotNull] object instance, [NotNull, ItemCanBeNull] params object[] parameters)
        {
            if (_method == null) return ReturnFailure("Method does not exist!");
            if (!_instanceType.IsInstanceOfType(instance)) return ReturnFailure("Instance is not of the right type!");
            if (_parameters.Length != parameters.Length) return ReturnFailure($"The number of parameters was {parameters.Length} and must be {_parameters.Length}");

            for (int i = 0; i < parameters.Length; i++)
            {
                object parameter = parameters[i];
                if (parameter == null)
                {
                    if (!_parameters[i].IsValueType) continue;
                    return ReturnFailure("A parameter was null but the required Type was a Value Type!");
                }
                if (_parameters[i].IsInstanceOfType(parameter)) continue;
                return ReturnFailure($"A parameter had the Type {parameter.GetType().Name} but the required Type was {_parameters[i].Name}");
            }

            return _method.Invoke(instance, parameters).CastOrDefaultAllowNull(_defaultReturnValue);
        }

        public void Invoke([NotNull] object instance, [NotNull, ItemCanBeNull] params object[] parameters)
        {
            InvokeWithReturn(instance, parameters);
        }

        [CanBeNull]
        private TReturn ReturnFailure([NotNull] string msg)
        {
            Logger.PrintWarning($"Reflection failed! (Type: {_instanceType.Name}, Method: {_methodName})\nReason: {msg}");
            return _defaultReturnValue;
        }
    }
}
