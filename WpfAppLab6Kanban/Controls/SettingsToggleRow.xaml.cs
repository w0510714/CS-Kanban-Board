using System.Windows;
using System.Windows.Controls;

namespace WpfAppLab6Kanban.Controls
{
    // ==========================================================================
    //  SettingsToggleRow — code-behind for the custom UserControl
    // ==========================================================================
    //
    //  Demonstrates a TWO-WAY Dependency Property (IsOn).
    //
    //  Two-way binding challenge with UserControls:
    //    When a CheckBox lives inside a generic UserControl, the host can't
    //    directly bind to the inner CheckBox.IsChecked because it's private
    //    to the control's XAML.  The solution is to expose a public DP (IsOn)
    //    on the UserControl itself and keep it in sync with the inner CheckBox
    //    via a PropertyChangedCallback.
    //
    //  ROUTED EVENT — Toggled:
    //    We also expose a Toggled RoutedEvent so the host can react immediately
    //    when the user flips the toggle (e.g., to preview Dark Mode live),
    //    without needing to poll IsOn in a Save handler.
    // ==========================================================================
    public partial class SettingsToggleRow : UserControl
    {
        // ── IsOn Dependency Property ───────────────────────────────────────────
        // The PropertyChangedCallback keeps the inner CheckBox in sync when
        // the host sets IsOn programmatically (e.g., LoadExistingSettings()).
        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register(
                nameof(IsOn),
                typeof(bool),
                typeof(SettingsToggleRow),
                new FrameworkPropertyMetadata(
                    defaultValue: false,
                    flags: FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    propertyChangedCallback: OnIsOnChanged));

        public bool IsOn
        {
            get => (bool)GetValue(IsOnProperty);
            set => SetValue(IsOnProperty, value);
        }

        // ── Label Dependency Property ──────────────────────────────────────────
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(SettingsToggleRow),
                new PropertyMetadata("Setting"));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        // ── Toggled RoutedEvent ────────────────────────────────────────────────
        // Bubbles up the visual tree so the host Window can react immediately.
        public static readonly RoutedEvent ToggledEvent =
            EventManager.RegisterRoutedEvent(
                nameof(Toggled),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(SettingsToggleRow));

        // CLR event wrapper — enables += / -= subscription in the host
        public event RoutedEventHandler Toggled
        {
            add    => AddHandler(ToggledEvent, value);
            remove => RemoveHandler(ToggledEvent, value);
        }

        // ── Constructor ────────────────────────────────────────────────────────
        public SettingsToggleRow()
        {
            InitializeComponent();
        }

        // ── PropertyChangedCallback ────────────────────────────────────────────
        // Fires when the host sets IsOn (e.g., from LoadExistingSettings).
        // Syncs the visual CheckBox state to match the new DP value.
        private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingsToggleRow row)
                row.InnerCheckBox.IsChecked = (bool)e.NewValue;
        }

        // ── CheckBox event handler ─────────────────────────────────────────────
        // Fires when the USER clicks the toggle.
        // Syncs the DP back to the CheckBox and raises the Toggled RoutedEvent.
        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            IsOn = InnerCheckBox.IsChecked ?? false;
            RaiseEvent(new RoutedEventArgs(ToggledEvent, this));
        }
    }
}
