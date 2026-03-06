using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfAppLab6Kanban.Models;

namespace WpfAppLab6Kanban.Controls
{
    // ==========================================================================
    //  KanbanColumnControl — code-behind for the custom UserControl
    // ==========================================================================
    //
    //  WHAT IS A DEPENDENCY PROPERTY?
    //  ───────────────────────────────
    //  A Dependency Property (DP) is WPF's enhanced property system.  Unlike a
    //  plain C# auto-property, a DP:
    //    • Can be the TARGET of a data binding  (e.g. TasksSource="{Binding TodoTasks}")
    //    • Participates in style inheritance and animation
    //    • Notifies the layout system automatically when its value changes
    //    • Is registered on the WPF property system via DependencyProperty.Register
    //
    //  Each DP needs:
    //    1. A static DependencyProperty field (the "key")
    //    2. A CLR property wrapper (for designer and code-behind access)
    //    3. Registered with the correct owner type and default value
    // ==========================================================================
    public partial class KanbanColumnControl : UserControl
    {
        // ── ColumnTitle ────────────────────────────────────────────────────────
        // Used by: the header TextBlock Text binding
        public static readonly DependencyProperty ColumnTitleProperty =
            DependencyProperty.Register(
                nameof(ColumnTitle),
                typeof(string),
                typeof(KanbanColumnControl),
                new PropertyMetadata("Column"));

        public string ColumnTitle
        {
            get => (string)GetValue(ColumnTitleProperty);
            set => SetValue(ColumnTitleProperty, value);
        }

        // ── ColumnColor ────────────────────────────────────────────────────────
        // Used by: the Border.BorderBrush and the header TextBlock.Foreground
        public static readonly DependencyProperty ColumnColorProperty =
            DependencyProperty.Register(
                nameof(ColumnColor),
                typeof(Brush),
                typeof(KanbanColumnControl),
                new PropertyMetadata(Brushes.SteelBlue));

        public Brush ColumnColor
        {
            get => (Brush)GetValue(ColumnColorProperty);
            set => SetValue(ColumnColorProperty, value);
        }

        // ── ColumnBackground ───────────────────────────────────────────────────
        // Used by: the Border.Background (transparent tinted fill)
        public static readonly DependencyProperty ColumnBackgroundProperty =
            DependencyProperty.Register(
                nameof(ColumnBackground),
                typeof(Brush),
                typeof(KanbanColumnControl),
                new PropertyMetadata(Brushes.Transparent));

        public Brush ColumnBackground
        {
            get => (Brush)GetValue(ColumnBackgroundProperty);
            set => SetValue(ColumnBackgroundProperty, value);
        }

        // ── TasksSource ────────────────────────────────────────────────────────
        // Used by: the ListBox.ItemsSource binding
        // IEnumerable lets this work with any collection type, including
        // ObservableCollection<KanbanTask>, without a tight coupling.
        public static readonly DependencyProperty TasksSourceProperty =
            DependencyProperty.Register(
                nameof(TasksSource),
                typeof(IEnumerable),
                typeof(KanbanColumnControl),
                new PropertyMetadata(null));

        public IEnumerable TasksSource
        {
            get => (IEnumerable)GetValue(TasksSourceProperty);
            set => SetValue(TasksSourceProperty, value);
        }

        // ── ItemDoubleClickCommand ─────────────────────────────────────────────
        // Executed when the user double-clicks a task card.
        // The host Window passes its ViewModel's EditTaskCommand here.
        // Using ICommand as the type keeps this control decoupled from the ViewModel.
        public static readonly DependencyProperty ItemDoubleClickCommandProperty =
            DependencyProperty.Register(
                nameof(ItemDoubleClickCommand),
                typeof(ICommand),
                typeof(KanbanColumnControl),
                new PropertyMetadata(null));

        public ICommand ItemDoubleClickCommand
        {
            get => (ICommand)GetValue(ItemDoubleClickCommandProperty);
            set => SetValue(ItemDoubleClickCommandProperty, value);
        }

        // ── Constructor ────────────────────────────────────────────────────────
        public KanbanColumnControl()
        {
            InitializeComponent();
        }

        // ── Event handler ──────────────────────────────────────────────────────
        // Double-click on a card → execute ItemDoubleClickCommand with the task.
        // The host Window doesn't need a code-behind handler for this anymore.
        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox lb &&
                lb.SelectedItem is KanbanTask task &&
                ItemDoubleClickCommand?.CanExecute(task) == true)
            {
                ItemDoubleClickCommand.Execute(task);
            }
        }
    }
}
