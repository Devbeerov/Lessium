using System.Windows;
using System.Windows.Controls;

namespace Lessium.CustomControls
{
    public partial class TitledProgressBar : UserControl
    {
        public TitledProgressBar()
        {
            InitializeComponent();
        }

        #region Methods

        public void UpdateProgressText()
        {
            ProgressText = $"{ProgressValue}/{ProgressMaximum}";
        }

        #endregion

        #region Dependency Properties

        #region Callbacks

        private static void OnProgressValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var titledProgressBar = d as TitledProgressBar;
            titledProgressBar.UpdateProgressText();
        }

        private static object CoerceProgressValue(DependencyObject d, object value)
        {
            var titledProgressBar = d as TitledProgressBar;

            int current = (int)value;
            if (current < titledProgressBar.ProgressMinimum) current = titledProgressBar.ProgressMinimum;
            if (current > titledProgressBar.ProgressMaximum) current = titledProgressBar.ProgressMaximum;
            return current;
        }

        private static void OnProgressMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var titledProgressBar = d as TitledProgressBar;

            if (titledProgressBar.ProgressMinimum > titledProgressBar.ProgressMaximum)
                titledProgressBar.ProgressMaximum = titledProgressBar.ProgressMinimum;

            d.CoerceValue(ProgressValueProperty);
        }

        private static void OnProgressMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var titledProgressBar = d as TitledProgressBar;

            d.CoerceValue(ProgressValueProperty);

            // In case CoerceValue won't call UpdateProgressText, we do it manually here.

            titledProgressBar.UpdateProgressText();
        }

        #endregion

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string ProgressText
        {
            get { return (string)GetValue(ProgressTextProperty); }
            private set { SetValue(ProgressTextProperty, value); }
        }

        public int ProgressValue
        {
            get { return (int)GetValue(ProgressValueProperty); }
            set { SetValue(ProgressValueProperty, value); }
        }

        public int ProgressMinimum
        {
            get { return (int)GetValue(ProgressMinimumProperty); }
            set { SetValue(ProgressMinimumProperty, value); }
        }

        public int ProgressMaximum
        {
            get { return (int)GetValue(ProgressMaximumProperty); }
            set { SetValue(ProgressMaximumProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TitledProgressBar), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ProgressTextProperty =
            DependencyProperty.Register("ProgressText", typeof(string), typeof(TitledProgressBar), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register("ProgressValue", typeof(int), typeof(TitledProgressBar), new PropertyMetadata(0,
                new PropertyChangedCallback(OnProgressValueChanged), new CoerceValueCallback(CoerceProgressValue)));

        public static readonly DependencyProperty ProgressMinimumProperty =
            DependencyProperty.Register("ProgressMinimum", typeof(int), typeof(TitledProgressBar), new PropertyMetadata(0,
                new PropertyChangedCallback(OnProgressMinimumChanged)));

        public static readonly DependencyProperty ProgressMaximumProperty =
            DependencyProperty.Register("ProgressMaximum", typeof(int), typeof(TitledProgressBar), new PropertyMetadata(0,
                new PropertyChangedCallback(OnProgressMaximumChanged)));

        #endregion
    }
}
