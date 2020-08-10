#region Imports

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using CSharpUtilsNETFramework.Controls;
using CSharpUtilsNETStandard.Utils;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework.GUI.Dialogs
{
    /// <summary>
    /// This class provides a windows form that shows the calculation progress during analysis to the user.
    /// </summary>
    [PublicAPI]
    public sealed partial class ProgressDialog : ExtendedForm
    {
        public string Title
        {
            get => Text;
            set => Text = value;
        }

        public string StopButtonText
        {
            get => buttonStop.Text;
            set => buttonStop.Text = value;
        }


        public Action ActionStop { get; set; }

        public Action ActionClose { get; set; }

        private bool _showStopButton;

        public bool ShowStopButton
        {
            get => _showStopButton;
            set
            {
                _showStopButton = value;
                UpdateDialog();
            }
        }

        private bool _showOutput;

        public bool ShowOutput
        {
            get => _showOutput;
            set
            {
                _showOutput = value;
                UpdateDialog();
            }
        }

        public bool StopClicked { get; private set; }

        public bool PreventClosing { get; set; }

        public bool HideOnClose { get; set; }

        public bool ShowPercentage
        {
            get => TLPMain.Controls.Contains(percentageLabel);
            set
            {
                if (value)
                {
                    TLPMain.SetColumnSpan(progressBar, 2);
                    TLPMain.Controls.Add(percentageLabel, 2, 0);
                }
                else
                {
                    TLPMain.Controls.Remove(percentageLabel);
                    TLPMain.SetColumnSpan(progressBar, 3);
                }
            }
        }

        public ProgressBar ProgressBar => progressBar;

        public TextBox TextBoxInfo => textBoxInfo;

        private bool _ready;

        public ProgressDialog()
        {
            InitializeComponent();
            ShowPercentage = false;
        }

        public ProgressDialog(string title, bool showStopButton = true, bool showOutput = false, bool preventClosing = true, bool hideOnClose = true) : this()
        {
            Title = title;
            ShowStopButton = showStopButton;
            ShowOutput = showOutput;
            PreventClosing = preventClosing;
            HideOnClose = hideOnClose;
        }

        public ProgressDialog(string title, string stopButtonText, bool showStopButton = true, bool showOutput = true, bool preventClosing = false, bool hideOnClose = false, [CanBeNull] Action actionStop = null, [CanBeNull] Action actionClose = null) : this()
        {
            Title = title;
            StopButtonText = stopButtonText;
            ShowStopButton = showStopButton;
            ShowOutput = showOutput;
            PreventClosing = preventClosing;
            HideOnClose = hideOnClose;
            ActionStop = actionStop;
            ActionClose = actionClose;
            ActiveControl = buttonStop;
        }

        [NotNull]
        public static ProgressDialog GetUnclosable(string title, bool usePercentageAndManualUpdates)
        {
            var progressDialog = new ProgressDialog(title, false, false, true, false);
            if (usePercentageAndManualUpdates)
            {
                progressDialog.ShowPercentage = true;
                progressDialog.ProgressBar.Style = ProgressBarStyle.Blocks;
                progressDialog.UpdateProgressBarValueSafe(0);
            }
            return progressDialog;
        }

        [NotNull]
        public static ProgressDialog GetCancelable(string title, Action actionOnStopAndClose, bool usePercentageAndManualUpdates)
        {
            var progressDialog = new ProgressDialog(title, true, false, false, false);
            progressDialog.ActionClose = actionOnStopAndClose;
            progressDialog.ActionStop = actionOnStopAndClose;
            if (usePercentageAndManualUpdates)
            {
                progressDialog.ShowPercentage = true;
                progressDialog.ProgressBar.Style = ProgressBarStyle.Blocks;
                progressDialog.UpdateProgressBarValueSafe(0);
            }
            return progressDialog;
        }

        public void UpdateProgressBarValueSafe(int value)
        {
            this.InvokeIfRequiredAndNotDisposed(() =>
            {
                if (value < ProgressBar.Minimum) value = ProgressBar.Minimum;
                if (value > ProgressBar.Maximum) value = ProgressBar.Maximum;
                ProgressBar.Value = value;
                percentageLabel.Text = string.Format(" {0}% ", value);
                percentageLabel.Refresh();
            });
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ready = true;
            UpdateDialog();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (CloseOnNextTimerAction) return;
            if (StopClicked) return;
            ActionClose?.Invoke();
            if (HideOnClose) Hide();
            if (PreventClosing) e.Cancel = true;
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            ActionStop?.Invoke();
            StopClicked = true;
        }

        private void UpdateDialog()
        {
            if (_ready)
            {
                SetRowVisibility(TLPMain, ShowOutput, 1);
                SetRowVisibility(TLPMain, ShowStopButton, 2);
            }
        }

        private bool ActivateCloseTimerActionOnShown { get; set; }

        private bool CloseOnNextTimerAction { get; set; }

        public void CloseAndDisposeSeparateUIThread()
        {
            CloseOnNextTimerAction = true;
        }

        private int CloseConditionCheckIntervalInMillis { get; set; }
        [CanBeNull]
        private TimerAction<byte> CloseTimerAction { get; set; }

        /// <summary>
        /// This is dangerous code and should be used very carefully!
        /// It will start the progress dialog in another UI thread.
        /// Because of some reason that I don't know the Main UI thread can still access the progress dialog without invokes.
        /// This behaviour is not safe and the Main UI thread should only access the UpdateProgressBarValueSafe(int value) method.
        /// If you use this method keep it very simple and do access any controls that are running on the Main UI thread.
        /// Make sure you test usages of this method.
        /// Be aware the progress dialog is disposed directly when the second UI thread ends.
        /// </summary>
        /// <param name="closeConditionCheckIntervalInMillis"></param>
        public void RunInSeparateUIThreadAndDisposeAfterwards(int closeConditionCheckIntervalInMillis = 100)
        {
            ActivateCloseTimerActionOnShown = true;
            CloseConditionCheckIntervalInMillis = closeConditionCheckIntervalInMillis;
            Thread thread = new Thread(() =>
            {
                //InvokeRequired will create the Handle
                if (InvokeRequired)
                {
                    //This would mean the Handle was already created in another thread and this does not work
                    Logger.PrintWarning("The progress dialog can only be used in another UI Thread if the Handle was not created yet!\nThe progress dialog will not be shown!");
                    Dispose();
                    return;
                }
                Application.Run(this);
                CloseTimerAction?.StopTimer(false);
                Dispose();
            });
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public void RunInSeparateUIThreadAndDisposeAfterwards(bool condition, int closeConditionCheckIntervalInMillis = 100)
        {
            if (!condition) return;
            RunInSeparateUIThreadAndDisposeAfterwards(closeConditionCheckIntervalInMillis);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (!ActivateCloseTimerActionOnShown) return;
            CloseTimerAction = new TimerAction<byte>(value =>
            {
                if (CloseOnNextTimerAction) Close();
                else CloseTimerAction?.ResetTimer();
            }, CloseConditionCheckIntervalInMillis);
            CloseTimerAction.ResetTimer();
        }
    }
}
