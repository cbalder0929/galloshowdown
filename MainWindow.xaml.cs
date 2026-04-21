using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace GalloShowdown
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _currentIndex = 0;

        private readonly (string Title, string Description)[] _slides =
        [
            (
                "Welcome to GalloShowdown",
                "In a world where only the strongest roosters survive, legends are born in the arena."
            ),
            (
                "The Rise",
                "You are a young rooster — underestimated, yet burning with determination to prove your strength."
            ),
            (
                "The Arena",
                "Fighters from every land gather to battle for honor, glory, and ultimate dominance."
            ),
            (
                "Your Rival",
                "A fierce champion stands in your path, a warrior known for never once tasting defeat."
            ),
            (
                "The Challenge",
                "Train hard, fight harder, and rise through the ranks to claim the place you were born for."
            ),
            (
                "Showdown Begins",
                "Step into the arena. The crowd roars. Your destiny awaits."
            )
        ];

        public MainWindow()
        {
            InitializeComponent();
            BuildDotIndicators();
            UpdateSlide(animate: false);
        }

        // ── Slideshow navigation ──────────────────────────────────────────────

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                UpdateSlide(animate: true);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex < _slides.Length - 1)
            {
                _currentIndex++;
                UpdateSlide(animate: true);
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            SlideshowScreen.Visibility = Visibility.Collapsed;
            NavScreen.Visibility = Visibility.Visible;
        }

        // ── Nav screen ────────────────────────────────────────────────────────

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            if (btn.Content?.ToString() == "Housing")
            {
                NavScreen.Visibility = Visibility.Collapsed;
                HousingScreen.Visibility = Visibility.Visible;
                return;
            }

            MessageBox.Show($"{btn.Content} coming soon!", "GalloShowdown",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── Housing scene ─────────────────────────────────────────────────────

        private void HousingBack_Click(object sender, RoutedEventArgs e)
        {
            HousingScreen.Visibility = Visibility.Collapsed;
            NavScreen.Visibility = Visibility.Visible;
        }

        // ── Slide rendering ───────────────────────────────────────────────────

        private void UpdateSlide(bool animate)
        {
            bool isLast = _currentIndex == _slides.Length - 1;

            PrevButton.IsEnabled = _currentIndex > 0;
            NextButton.Visibility = isLast ? Visibility.Collapsed : Visibility.Visible;
            StartButton.Visibility = isLast ? Visibility.Visible : Visibility.Collapsed;

            UpdateDotIndicators();

            if (animate)
                FadeTransition(ApplySlideContent);
            else
                ApplySlideContent();
        }

        private void ApplySlideContent()
        {
            var (title, description) = _slides[_currentIndex];
            SlideTitle.Text = title;
            SlideDescription.Text = description;
            SlideIndicator.Text = $"SLIDE {_currentIndex + 1} OF {_slides.Length}";
        }

        // ── Dot indicators ────────────────────────────────────────────────────

        private void BuildDotIndicators()
        {
            DotIndicators.Items.Clear();
            for (int i = 0; i < _slides.Length; i++)
            {
                DotIndicators.Items.Add(new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Margin = new Thickness(4, 0, 4, 0),
                    Fill = i == _currentIndex
                        ? new SolidColorBrush(Color.FromRgb(0xe9, 0x45, 0x60))
                        : new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x77))
                });
            }
        }

        private void UpdateDotIndicators()
        {
            for (int i = 0; i < DotIndicators.Items.Count; i++)
            {
                if (DotIndicators.Items[i] is Ellipse dot)
                {
                    dot.Fill = i == _currentIndex
                        ? new SolidColorBrush(Color.FromRgb(0xe9, 0x45, 0x60))
                        : new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x77));
                }
            }
        }

        // ── Fade transition ───────────────────────────────────────────────────

        private void FadeTransition(Action midAction)
        {
            var fadeIn  = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(120));
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(120));

            fadeIn.Completed += (_, _) =>
            {
                midAction();
                FadeOverlay.BeginAnimation(OpacityProperty, fadeOut);
            };

            FadeOverlay.BeginAnimation(OpacityProperty, fadeIn);
        }
    }
}