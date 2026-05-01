using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using GalloShowdown.Engine;
using GalloShowdown.Input;
using GalloShowdown.Models;
using GalloShowdown.Models.Breeds;

namespace GalloShowdown
{
    public partial class MainWindow : Window
    {
        // ── Battle state ──────────────────────────────────────────────────────

        private BattleEngine? _battle;
        private readonly HashSet<Key> _pressedKeys = new();
        private FrameworkElement? _p1Sprite;
        private FrameworkElement? _p2Sprite;
        private readonly ScaleTransform _p1Transform = new(1, 1);
        private readonly ScaleTransform _p2Transform = new(-1, 1);
        private EventHandler? _renderHandler;

        private readonly Stopwatch _stopwatch = new();
        private TimeSpan _lastFrameTime;

        private int _p1Wins, _p2Wins;
        private bool _roundActive;
        private bool _battleInitialized;
        private bool _roundTransitioning;
        private DateTime? _koDetectedAt;
        private double _roundTimeRemaining;
        private DateTime _p1FlashEnd, _p2FlashEnd;

        // ── Housing state ─────────────────────────────────────────────────────

        private readonly Stable _stable = App.PlayerStable;
        private BitmapImage? _p1IdleImage;
        private BitmapImage? _p1LightImage;
        private BitmapImage? _p1HeavyImage;
        private BitmapImage? _p2IdleImage;
        private BitmapImage? _p2LightImage;
        private BitmapImage? _p2HeavyImage;

        private const double GroundOffset = 20.0;

        private static readonly SolidColorBrush P1Fill        = new(Color.FromRgb(0xe9, 0x45, 0x60));
        private static readonly SolidColorBrush P1StartupFill = new(Color.FromRgb(0xff, 0x80, 0x9a));
        private static readonly SolidColorBrush P1ActiveFill  = new(Color.FromRgb(0xff, 0xff, 0xff));
        private static readonly SolidColorBrush P2Fill        = new(Color.FromRgb(0x0f, 0x34, 0x60));
        private static readonly SolidColorBrush P2StartupFill = new(Color.FromRgb(0x20, 0x60, 0xb0));
        private static readonly SolidColorBrush P2ActiveFill  = new(Color.FromRgb(0xff, 0xff, 0xff));

        // ── Constructor ───────────────────────────────────────────────────────

        public MainWindow()
        {
            InitializeComponent();
            StartLogoTimer();
        }

        private void StartLogoTimer()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(450));
                fadeOut.Completed += (_, _) =>
                {
                    LogoScreen.Visibility = Visibility.Collapsed;
                    ShowPerronLogo();
                };
                LogoScreen.BeginAnimation(OpacityProperty, fadeOut);
            };
            timer.Start();
        }

        private void ShowPerronLogo()
        {
            PerronDrop.Y    = -600;
            PerronScale.ScaleX = 1; PerronScale.ScaleY = 1;
            PerronGlow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, null);
            PerronGlow.BeginAnimation(DropShadowEffect.OpacityProperty,    null);
            PerronGlow.BlurRadius = 0; PerronGlow.Opacity = 0;

            PerronLogoScreen.Opacity    = 1;
            PerronLogoScreen.Visibility = Visibility.Visible;

            // Phase 1 — fall from sky, accelerating like gravity
            var drop = new DoubleAnimation(-600, 0, TimeSpan.FromMilliseconds(500))
            {
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseIn }
            };
            drop.Completed += (_, _) =>
            {
                // Phase 2 — squash on impact
                var squashX = new DoubleAnimation(1.0, 1.28, TimeSpan.FromMilliseconds(65));
                var squashY = new DoubleAnimation(1.0, 0.70, TimeSpan.FromMilliseconds(65));

                // Impact glow flare
                PerronGlow.BeginAnimation(DropShadowEffect.BlurRadiusProperty,
                    new DoubleAnimation(0, 90, TimeSpan.FromMilliseconds(65)));
                PerronGlow.BeginAnimation(DropShadowEffect.OpacityProperty,
                    new DoubleAnimation(0, 1.0, TimeSpan.FromMilliseconds(65)));

                squashX.Completed += (_, _) =>
                {
                    // Phase 3 — elastic snap back to shape
                    var restoreX = new DoubleAnimation(1.28, 1.0, TimeSpan.FromMilliseconds(420))
                    {
                        EasingFunction = new ElasticEase { Oscillations = 2, Springiness = 5, EasingMode = EasingMode.EaseOut }
                    };
                    var restoreY = new DoubleAnimation(0.70, 1.0, TimeSpan.FromMilliseconds(420))
                    {
                        EasingFunction = new ElasticEase { Oscillations = 2, Springiness = 5, EasingMode = EasingMode.EaseOut }
                    };
                    PerronScale.BeginAnimation(ScaleTransform.ScaleXProperty, restoreX);
                    PerronScale.BeginAnimation(ScaleTransform.ScaleYProperty, restoreY);

                    // Glow settles to a warm ambient level
                    PerronGlow.BeginAnimation(DropShadowEffect.BlurRadiusProperty,
                        new DoubleAnimation(90, 22, TimeSpan.FromMilliseconds(700)));
                    PerronGlow.BeginAnimation(DropShadowEffect.OpacityProperty,
                        new DoubleAnimation(1.0, 0.55, TimeSpan.FromMilliseconds(700)));

                    // Phase 4 — hold, then fade to slideshow
                    var hold = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.0) };
                    hold.Tick += (_, _) =>
                    {
                        hold.Stop();
                        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
                        fadeOut.Completed += (_, _) =>
                        {
                            PerronLogoScreen.Visibility = Visibility.Collapsed;
                            ShowScene111();
                        };
                        PerronLogoScreen.BeginAnimation(OpacityProperty, fadeOut);
                    };
                    hold.Start();
                };

                PerronScale.BeginAnimation(ScaleTransform.ScaleXProperty, squashX);
                PerronScale.BeginAnimation(ScaleTransform.ScaleYProperty, squashY);
            };

            PerronDrop.BeginAnimation(TranslateTransform.YProperty, drop);
        }

        // ── Scene 111: typewriter intro ───────────────────────────────────────

        private const string Scene111FullText =
            "Ever since I was a young boy I enjoyed watching the gallos fight...";

        private const string BirthdayFullText =
            "On my 10th birthday my grandfather payed me a visit...";

        private const string GiftingFullText =
            "My abuelo gifted me a rooster of the finest bloodlines...";

        private const string DreamFullText =
            " Ever since that moment I have dreamed of training and winning BIG!";

        private void ShowScene111()
        {
            Scene111Text.Text     = "";
            Scene111Screen.Opacity    = 1;
            Scene111Screen.Visibility = Visibility.Visible;

            int charIndex = 0;
            var typeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(55) };
            typeTimer.Tick += (_, _) =>
            {
                if (charIndex < Scene111FullText.Length)
                {
                    Scene111Text.Text += Scene111FullText[charIndex++];
                    return;
                }
                typeTimer.Stop();

                var hold = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
                hold.Tick += (_, _) =>
                {
                    hold.Stop();
                    var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
                    fadeOut.Completed += (_, _) =>
                    {
                        Scene111Screen.Visibility = Visibility.Collapsed;
                        ShowBirthdayScene();
                    };
                    Scene111Screen.BeginAnimation(OpacityProperty, fadeOut);
                };
                hold.Start();
            };
            typeTimer.Start();
        }

        private void ShowBirthdayScene()
        {
            BirthdayText.Text         = "";
            BirthdayScreen.Opacity    = 1;
            BirthdayScreen.Visibility = Visibility.Visible;

            int charIndex = 0;
            var typeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(55) };
            typeTimer.Tick += (_, _) =>
            {
                if (charIndex < BirthdayFullText.Length)
                {
                    BirthdayText.Text += BirthdayFullText[charIndex++];
                    return;
                }
                typeTimer.Stop();

                var hold = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
                hold.Tick += (_, _) =>
                {
                    hold.Stop();
                    var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
                    fadeOut.Completed += (_, _) =>
                    {
                        BirthdayScreen.Visibility = Visibility.Collapsed;
                        ShowGiftingScene();
                    };
                    BirthdayScreen.BeginAnimation(OpacityProperty, fadeOut);
                };
                hold.Start();
            };
            typeTimer.Start();
        }

        private void ShowGiftingScene()
        {
            GiftingText.Text         = "";
            GiftingScreen.Opacity    = 1;
            GiftingScreen.Visibility = Visibility.Visible;

            int charIndex = 0;
            var typeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(55) };
            typeTimer.Tick += (_, _) =>
            {
                if (charIndex < GiftingFullText.Length)
                {
                    GiftingText.Text += GiftingFullText[charIndex++];
                    return;
                }
                typeTimer.Stop();

                var hold = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
                hold.Tick += (_, _) =>
                {
                    hold.Stop();
                    var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
                    fadeOut.Completed += (_, _) =>
                    {
                        GiftingScreen.Visibility = Visibility.Collapsed;
                        ShowDreamScene();
                    };
                    GiftingScreen.BeginAnimation(OpacityProperty, fadeOut);
                };
                hold.Start();
            };
            typeTimer.Start();
        }

        private void ShowDreamScene()
        {
            DreamText.Text         = "";
            DreamScreen.Opacity    = 1;
            DreamScreen.Visibility = Visibility.Visible;

            int charIndex = 0;
            var typeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(55) };
            typeTimer.Tick += (_, _) =>
            {
                if (charIndex < DreamFullText.Length)
                {
                    DreamText.Text += DreamFullText[charIndex++];
                    return;
                }
                typeTimer.Stop();

                var hold = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
                hold.Tick += (_, _) =>
                {
                    hold.Stop();
                    var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
                    fadeOut.Completed += (_, _) =>
                    {
                        DreamScreen.Visibility   = Visibility.Collapsed;
                        NavScreen.Visibility     = Visibility.Visible;
                    };
                    DreamScreen.BeginAnimation(OpacityProperty, fadeOut);
                };
                hold.Start();
            };
            typeTimer.Start();
        }

        // ── Nav screen ────────────────────────────────────────────────────────

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            string label = btn.Content?.ToString() ?? "";

            if (label == "HOUSING")
            {
                NavScreen.Visibility = Visibility.Collapsed;
                HousingScreen.Visibility = Visibility.Visible;
                RefreshHousing();
                return;
            }

            if (label == "BATTLE")
            {
                StartBattle();
                return;
            }

            MessageBox.Show($"{label} coming soon!", "GalloShowdown",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── Housing scene ─────────────────────────────────────────────────────

        private void RefreshHousing()
        {
            try
            {
                var r = _stable.Current;
                HousingNameBox.Text        = r.Name;
                HousingBreedText.Text      = r.BreedName;
                HousingHealthText.Text     = r.Health.ToString();
                HousingStaminaText.Text    = r.Stamina.ToString();
                HousingSpeedText.Text      = r.Speed.ToString();
                HousingRoosterImage.Source = new BitmapImage(
                    new Uri($"pack://application:,,,/{r.ImagePath}"));
                HousingSelectButton.Content   = _stable.CurrentIsSelected ? "Selected ✓" : "Select";
                HousingSelectButton.IsEnabled = !_stable.CurrentIsSelected;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"RefreshHousing failed:\n\n{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}",
                    "Housing error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HousingPrev_Click(object sender, RoutedEventArgs e)   { _stable.Prev();          RefreshHousing(); }
        private void HousingNext_Click(object sender, RoutedEventArgs e)   { _stable.Next();          RefreshHousing(); }
        private void HousingSelect_Click(object sender, RoutedEventArgs e) { _stable.SelectCurrent(); RefreshHousing(); }

        private void HousingNameBox_LostFocus(object sender, RoutedEventArgs e)
            => _stable.Current.Name = HousingNameBox.Text;

        private void HousingBack_Click(object sender, RoutedEventArgs e)
        {
            HousingScreen.Visibility = Visibility.Collapsed;
            NavScreen.Visibility = Visibility.Visible;
        }

        // ── Battle: entry / exit ──────────────────────────────────────────────

        private void StartBattle()
        {
            _p1Wins = 0;
            _p2Wins = 0;
            _battleInitialized = false;
            _roundTransitioning = false;
            _roundActive = false;

            KeyDown += Window_KeyDown;
            KeyUp   += Window_KeyUp;

            NavScreen.Visibility    = Visibility.Collapsed;
            BattleScreen.Visibility = Visibility.Visible;

            if (ArenaCanvas.ActualWidth > 0)
            {
                _battleInitialized = true;
                StartRound();
            }
            else
            {
                ArenaCanvas.SizeChanged += OnArenaSized;
            }
        }

        private void OnArenaSized(object sender, SizeChangedEventArgs e)
        {
            if (_battleInitialized || e.NewSize.Width <= 0) return;
            _battleInitialized = true;
            ArenaCanvas.SizeChanged -= OnArenaSized;
            StartRound();
        }

        private void StartRound()
        {
            DetachRenderLoop();
            ArenaCanvas.Children.Clear();

            MatchOverlay.Visibility = Visibility.Collapsed;
            KOBanner.Visibility     = Visibility.Collapsed;
            FightBanner.Visibility  = Visibility.Collapsed;
            _roundTransitioning     = false;
            _koDetectedAt           = null;
            _roundActive            = false;
            _roundTimeRemaining     = 99.0;

            double arenaW = ArenaCanvas.ActualWidth;
            double arenaH = ArenaCanvas.ActualHeight;

            var p1 = new RoosterFighter(App.PlayerStable.Selected);
            var p2 = new RoosterFighter(new BlackRooster());
            p1.PlaceAt(100, 0);
            p2.PlaceAt(arenaW - Fighter.BodyWidth - 100, 0);
            p1.FaceTowards(p2.X);
            p2.FaceTowards(p1.X);

            _p1IdleImage  = new BitmapImage(new Uri($"pack://application:,,,/{p1.ImagePath}"));
            _p1LightImage = new BitmapImage(new Uri($"pack://application:,,,/{p1.LightAttackImagePath}"));
            _p1HeavyImage = new BitmapImage(new Uri($"pack://application:,,,/{p1.HeavyAttackImagePath}"));
            _p2IdleImage  = new BitmapImage(new Uri($"pack://application:,,,/{p2.ImagePath}"));
            _p2LightImage = new BitmapImage(new Uri($"pack://application:,,,/{p2.LightAttackImagePath}"));
            _p2HeavyImage = new BitmapImage(new Uri($"pack://application:,,,/{p2.HeavyAttackImagePath}"));

            var bindings = new Dictionary<InputCommand, Key>
            {
                [InputCommand.MoveLeft]    = Key.A,
                [InputCommand.MoveRight]   = Key.D,
                [InputCommand.Jump]        = Key.W,
                [InputCommand.Crouch]      = Key.S,
                [InputCommand.LightAttack] = Key.Left,
                [InputCommand.HeavyAttack] = Key.Right,
            };

            var p1Input = new KeyboardInputProvider(_pressedKeys, bindings);
            var p2Input = new AIInputProvider(p2, p1);

            _battle = new BattleEngine(p1, p1Input, p2, p2Input, arenaW);
            _battle.HitLanded += OnHitLanded;

            // Ground line
            var groundLine = new Rectangle
            {
                Width  = arenaW,
                Height = 3,
                Fill   = new SolidColorBrush(Color.FromRgb(0x3a, 0x3a, 0x5e))
            };
            Canvas.SetLeft(groundLine, 0);
            Canvas.SetTop(groundLine, arenaH - GroundOffset - 3);
            ArenaCanvas.Children.Add(groundLine);

            _p1Sprite = new Image
            {
                Width  = Fighter.BodyWidth,
                Height = Fighter.BodyHeight,
                Source = _p1IdleImage,
                Stretch = Stretch.Uniform,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = _p1Transform
            };
            _p1Transform.ScaleX = 1;

            _p2Sprite = new Image
            {
                Width  = Fighter.BodyWidth,
                Height = Fighter.BodyHeight,
                Source = _p2IdleImage,
                Stretch = Stretch.Uniform,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = _p2Transform
            };
            ArenaCanvas.Children.Add(_p1Sprite);
            ArenaCanvas.Children.Add(_p2Sprite);

            P1Name.Text  = p1.Name;
            P2Name.Text  = p2.Name;
            P1Stats.Text = $"ATK {p1.Attack}  DEF {p1.Defense}  SPD {(int)p1.Speed}";
            P2Stats.Text = $"ATK {p2.Attack}  DEF {p2.Defense}  SPD {(int)p2.Speed}";
            UpdateWinsDisplay();

            _stopwatch.Restart();
            _lastFrameTime = TimeSpan.Zero;
            _renderHandler = OnRendering;
            CompositionTarget.Rendering += _renderHandler;

            ShowFightBanner();
        }

        private void BattleBack_Click(object sender, RoutedEventArgs e)
        {
            DetachRenderLoop();
            KeyDown -= Window_KeyDown;
            KeyUp   -= Window_KeyUp;
            _pressedKeys.Clear();
            ArenaCanvas.SizeChanged -= OnArenaSized;
            ArenaCanvas.Children.Clear();
            _battle      = null;
            _p1Sprite    = null;
            _p2Sprite    = null;
            MatchOverlay.Visibility = Visibility.Collapsed;
            KOBanner.Visibility     = Visibility.Collapsed;
            FightBanner.Visibility  = Visibility.Collapsed;
            BattleScreen.Visibility = Visibility.Collapsed;
            NavScreen.Visibility    = Visibility.Visible;
        }

        private void Rematch_Click(object sender, RoutedEventArgs e)
        {
            _p1Wins = 0;
            _p2Wins = 0;
            StartRound();
        }

        // ── Battle: game loop ─────────────────────────────────────────────────

        private void OnRendering(object? sender, EventArgs e)
        {
            if (_battle == null) return;

            var now = _stopwatch.Elapsed;
            double dt = Math.Min((now - _lastFrameTime).TotalSeconds, 0.05);
            _lastFrameTime = now;

            if (_roundActive && !_battle.RoundOver)
            {
                _roundTimeRemaining -= dt;
                if (_roundTimeRemaining <= 0)
                {
                    _roundTimeRemaining = 0;
                    var timerWinner = _battle.P1.Health >= _battle.P2.Health ? _battle.P1 : _battle.P2;
                    _battle.ForceRoundOver(timerWinner);
                }
                else
                {
                    _battle.Tick(dt);
                }
            }

            if (_battle.RoundOver && !_roundTransitioning)
                HandleKO();

            if (_roundTransitioning && _koDetectedAt.HasValue &&
                (DateTime.Now - _koDetectedAt.Value).TotalSeconds >= 1.5)
                ProcessRoundEnd();

            RefreshBattleUI();
        }

        private void HandleKO()
        {
            _roundTransitioning = true;
            _koDetectedAt = DateTime.Now;
            _roundActive = false;

            if (_battle!.Winner == _battle.P1) _p1Wins++;
            else if (_battle.Winner == _battle.P2) _p2Wins++;

            string koText = _battle.Winner != null
                ? $"K.O.! {_battle.Winner.Name} wins!"
                : "TIME! Draw!";
            KOBanner.Text       = koText;
            KOBanner.Visibility = Visibility.Visible;

            UpdateWinsDisplay();
        }

        private void ProcessRoundEnd()
        {
            _koDetectedAt       = null;
            _roundTransitioning = false;
            KOBanner.Visibility = Visibility.Collapsed;

            if (_p1Wins >= 2 || _p2Wins >= 2)
            {
                DetachRenderLoop();
                string winnerName = _p1Wins >= 2 ? _battle!.P1.Name : _battle!.P2.Name;
                MatchOverlayText.Text   = $"{winnerName} wins the match!";
                MatchOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                StartRound();
            }
        }

        private void DetachRenderLoop()
        {
            if (_renderHandler == null) return;
            CompositionTarget.Rendering -= _renderHandler;
            _renderHandler = null;
        }

        // ── Battle: UI refresh ────────────────────────────────────────────────

        private void RefreshBattleUI()
        {
            if (_battle == null || _p1Sprite == null || _p2Sprite == null) return;

            double arenaH = ArenaCanvas.ActualHeight;

            // HP bars
            double w1 = P1HealthBorder.ActualWidth * (_battle.P1.Health / (double)_battle.P1.MaxHealth);
            double w2 = P2HealthBorder.ActualWidth * (_battle.P2.Health / (double)_battle.P2.MaxHealth);
            P1HealthBar.Width = Math.Max(0, w1);
            P2HealthBar.Width = Math.Max(0, w2);

            // Timer
            RoundTimer.Text = ((int)Math.Ceiling(_roundTimeRemaining)).ToString();

            // Sprites
            UpdateSprite(_p1Sprite, _battle.P1, arenaH, p1Side: true);
            UpdateSprite(_p2Sprite, _battle.P2, arenaH, p1Side: false);
        }

        private void UpdateSprite(FrameworkElement sprite, Fighter f, double arenaH, bool p1Side)
        {
            bool crouching = f.State == FighterState.Crouching;
            double spriteH = crouching ? Fighter.CrouchHeight : Fighter.BodyHeight;
            sprite.Height  = spriteH;
            sprite.Width   = Fighter.BodyWidth;

            double canvasY = arenaH - GroundOffset - f.Y - spriteH;
            Canvas.SetLeft(sprite, f.X);
            Canvas.SetTop(sprite,  canvasY);

            DateTime now      = DateTime.Now;
            DateTime flashEnd = p1Side ? _p1FlashEnd : _p2FlashEnd;

            if (sprite is Image img)
            {
                if (p1Side)
                {
                    bool heavyP1 = f.CurrentAttack == CurrentAttackType.Heavy;
                    _p1Transform.ScaleX = (f.Facing > 0 ? 1 : -1) * (heavyP1 ? -1 : 1);
                    img.Source = f.CurrentAttack == CurrentAttackType.Light ? _p1LightImage
                               : heavyP1                                    ? _p1HeavyImage
                               : _p1IdleImage;
                }
                else
                {
                    bool heavyP2 = f.CurrentAttack == CurrentAttackType.Heavy;
                    _p2Transform.ScaleX = (f.Facing > 0 ? 1 : -1) * (heavyP2 ? -1 : 1);
                    img.Source = f.CurrentAttack == CurrentAttackType.Light ? _p2LightImage
                               : heavyP2                                    ? _p2HeavyImage
                               : _p2IdleImage;
                }

                if (now < flashEnd)           img.Opacity = 0.3;
                else if (f.IsInStartupFrames) img.Opacity = 0.65;
                else                          img.Opacity = 1.0;
            }
            else if (sprite is Rectangle rect)
            {
                if (now < flashEnd || f.IsInActiveFrames) rect.Fill = p1Side ? P1ActiveFill : P2ActiveFill;
                else if (f.IsInStartupFrames)             rect.Fill = p1Side ? P1StartupFill : P2StartupFill;
                else                                      rect.Fill = p1Side ? P1Fill : P2Fill;
            }
        }

        private void UpdateWinsDisplay()
        {
            P1WinsText.Text = new string('●', _p1Wins) + new string('○', Math.Max(0, 2 - _p1Wins));
            P2WinsText.Text = new string('●', _p2Wins) + new string('○', Math.Max(0, 2 - _p2Wins));
        }

        // ── Battle: hit flash ─────────────────────────────────────────────────

        private void OnHitLanded(Fighter defender)
        {
            if (_battle == null) return;
            DateTime flashEnd = DateTime.Now.AddMilliseconds(60);
            if (defender == _battle.P1) _p1FlashEnd = flashEnd;
            else                        _p2FlashEnd = flashEnd;
        }

        // ── Battle: banners ───────────────────────────────────────────────────

        private void ShowFightBanner()
        {
            FightBanner.Text       = "FIGHT!";
            FightBanner.Opacity    = 0;
            FightBanner.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            fadeIn.Completed += (_, _) =>
            {
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300))
                {
                    BeginTime = TimeSpan.FromMilliseconds(500)
                };
                fadeOut.Completed += (_, _) =>
                {
                    FightBanner.Visibility = Visibility.Collapsed;
                    _roundActive = true;
                };
                FightBanner.BeginAnimation(OpacityProperty, fadeOut);
            };
            FightBanner.BeginAnimation(OpacityProperty, fadeIn);
        }

        // ── Battle: keyboard ──────────────────────────────────────────────────

        private void Window_KeyDown(object sender, KeyEventArgs e) => _pressedKeys.Add(e.Key);
        private void Window_KeyUp(object sender, KeyEventArgs e)   => _pressedKeys.Remove(e.Key);

    }
}
