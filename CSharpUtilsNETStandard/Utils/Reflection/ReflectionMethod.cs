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

        public ReflectionMethod([NotNull]string methodName, [NotNull]Type instanceType, [NotNull, ItemNotNull]Type[] parameters, [CanBeNull]TReturn defaultReturnValue = default)
        {
            _methodName = methodName;
            _instanceType = instanceType;
            _defaultReturnValue = defaultReturnValue;
            _parameters = parameters;
            _method = _instanceType.GetMethod(methodName, parameters);
        }

        public ReflectionMethod([NotNull]string methodName, [NotNull]Type instanceType, [NotNull, ItemNotNull]params Type[] parameters)
        {
            _methodName = methodName;
            _instanceType = instanceType;
            _defaultReturnValue = default;
            _parameters = parameters;
            _method = _instanceType.GetMethod(methodName, parameters);
        }

        public ReflectionMethod([NotNull]string methodName, [NotNull]Type instanceType, [CanBeNull]TReturn defaultReturnValue = default)
        {
            _methodName = methodName;
            _instanceType = instanceType;
            _defaultReturnValue = defaultReturnValue;
            _parameters = new Type[0];
            _method = _instanceType.GetMethod(methodName);
        }

        [CanBeNull]
        public TReturn InvokeWithReturn([NotNull]object instance, [NotNull, ItemCanBeNull]params object[] parameters)
        {
            if (_method == null) return ReturnFailure("Method does not exist!");
            if (!_instanceType.IsInstanceOfType(instance)) return ReturnFailure("Instance is not of the right type!");
            if (_parameters.Length != parameters.Length) return ReturnFailure(string.Format("The number of parameters was {0} and must be {1}", parameters.Length, _parameters.Length));

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == null)
                {
                    if (!_parameters[i].IsValueType) continue;
                    return ReturnFailure("A parameter was null but the required Type was a Value Type!");
                }
                if (_parameters[i].IsInstanceOfType(parameters[i])) continue;
                return ReturnFailure(string.Format("A parameter had the Type {0} but the required Type was {1}", parameters[i].GetType().Name, _parameters[i].Name));
            }

            return _method.Invoke(instance, parameters).CastOrDefaultAllowNull(_defaultReturnValue);
        }

        public void Invoke([NotNull]object instance, [NotNull, ItemCanBeNull]params object[] parameters)
        {
            InvokeWithReturn(instance, parameters);
        }

        [CanBeNull]
        private TReturn ReturnFailure([NotNull]string msg)
        {
            Logger.PrintWarning(string.Format("Reflection failed! (Type: {0}, Method: {1})\nReason: {2}", _instanceType.Name, _methodName, msg));
            return _defaultReturnValue;
        }
    }
}
