#region Imports

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework
{
    [PublicAPI]
    public sealed class TimerAction<T>
    {
        [CanBeNull]
        public delegate T ActionValueGetter();
        public delegate void ActionAfterTimer([CanBeNull]T currentActionValue);
        public delegate bool ActionValueCondition([CanBeNull]T lastValue, [CanBeNull]T newValue);

        [NotNull] private readonly ActionValueGetter _actionValueGetter;
        [NotNull] private readonly ActionAfterTimer _actionAfterTimer;
        [NotNull] private readonly ActionValueCondition _actionValueCondition;
        public int ActionTimeoutInMillis { get; set; }
        [CanBeNull]
        private Timer Timer;
        [CanBeNull]
        private T CurrentActionValue => _actionValueGetter();

        [CanBeNull]
        private T LastActionValue { get; set; } // Will only be set at construction and if the ActionValueCondition is fulfilled

        public TimerAction([NotNull] ActionValueGetter actionValueGetterAfter, [NotNull] ActionAfterTimer actionAfterTimer, [NotNull] ActionValueCondition actionValueCondition, int actionTimeoutInMillis = 1000)
        {
            _actionValueGetter = actionValueGetterAfter;
            LastActionValue = _actionValueGetter();
            _actionAfterTimer = actionAfterTimer;
            ActionTimeoutInMillis = actionTimeoutInMillis;
            _actionValueCondition = actionValueCondition;
        }

        public TimerAction([NotNull] ActionAfterTimer actionAfterTimer, int actionTimeoutInMillis)
        {
            _actionValueGetter = () => default;
            LastActionValue = _actionValueGetter();
            _actionAfterTimer = actionAfterTimer;
            ActionTimeoutInMillis = actionTimeoutInMillis;
            _actionValueCondition = (value, newValue) => true;
        }

        public void StartTimer()
        {
            ResetTimer();
        }

        public void RestartTimer()
        {
            ResetTimer();
        }

        public void StopTimer(bool executeAction)
        {
            if (Timer == null) return;
            Timer.Stop();
            if (!executeAction) return;
            T currentActionValue = CurrentActionValue;
            if (!_actionValueCondition(LastActionValue, currentActionValue)) return;
            LastActionValue = currentActionValue;
            _actionAfterTimer(currentActionValue);
        }

        public void ResetTimer()
        {
            StopTimer(false);

            if (Timer == null || Timer.Interval != ActionTimeoutInMillis)
            {
                Timer = new Timer();
                Timer.Tick += HandleTimerTick;
                Timer.Interval = ActionTimeoutInMillis;
            }
            Timer.Start();
        }

        private void HandleTimerTick(object sender, EventArgs e)
        {
            if (!(sender is Timer)) return;
            StopTimer(true);
        }
    }

    [PublicAPI]
    public static class DelayedAction
    {
        private static readonly object Lock = new object();

        public static int MinimumDelayInMillis { get => DelayedActionTimer.ActionTimeoutInMillis; set => DelayedActionTimer.ActionTimeoutInMillis = value; }

        private static readonly TimerAction<byte> DelayedActionTimer = new TimerAction<byte>(value => ExecuteActions(), 100);

        private static readonly Queue<Action> DelayedActions = new Queue<Action>();

        public static void AddAction([NotNull] Action action)
        {
            lock (Lock)
            {
                DelayedActions.Enqueue(action);
                DelayedActionTimer.ResetTimer();
            }
        }

        private static void ExecuteActions()
        {
            lock (Lock)
            {
                while (DelayedActions.Count > 0) DelayedActions.Dequeue().Invoke();
            }
        }
    }
}
