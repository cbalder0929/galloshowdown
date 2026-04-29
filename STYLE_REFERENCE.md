# GalloShowdown — Style Reference

## Color Palette

| Token          | Hex       | Usage                                        |
|----------------|-----------|----------------------------------------------|
| Background     | `#1a1a2e` | Window background, nav screen top            |
| Panel          | `#16213e` | Cards, arena bg, nav screen mid, HP borders  |
| Deep Blue      | `#0f3460` | Nav tiles, P2 HP bar, nav screen bottom      |
| Accent Red     | `#e94560` | P1 HP bar, slide indicator, danger, titles   |
| Accent Orange  | `#f5a623` | Stat values, CTA buttons, banners, timers    |
| Wood Dark      | `#562a00` | Sign backgrounds, back-to-menu buttons       |
| Wood Mid       | `#7a4a1e` | Sign/button borders, housing accents         |
| Wood Light     | `#f5e6c8` | Sign text, light foreground text             |
| Muted Text     | `#b0b8d1` | Secondary labels, slide descriptions         |
| Pure Black     | `#000000` | Logo screen bg, nav button outer frame       |

## Hover / Active State Variants

| Base Color | Hover / Active | Element                  |
|------------|----------------|--------------------------|
| `#e94560`  | `#c73652`      | Red (SlideButtonStyle)   |
| `#f5a623`  | `#d4891a`      | Orange (StartButtonStyle)|
| `#0f3460`  | `#1a4a80`      | Nav tile hover           |
| `#562a00`  | `#7a4a1e`      | Wood back-button hover   |

## Nav Button Bevel (Pixel-Art Style)

| Layer          | Hex       | Notes                          |
|----------------|-----------|--------------------------------|
| Outer frame    | `#000000` | 3 px solid border              |
| Face (normal)  | `#0f3460` | Main button fill               |
| Top highlight  | `#3567a8` | Gives raised 3-D look          |
| Left highlight | `#3567a8` | Paired with top                |
| Bottom shadow  | `#06182f` | Gives depth                    |
| Right shadow   | `#06182f` | Paired with bottom             |
| Face (hover)   | `#1a4a80` | Lighter on mouse-over          |
| Hi (hover)     | `#5b9cd6` | Brighter highlight on hover    |
| Hi (pressed)   | `#06182f` | Inverted — sunken appearance   |
| Shadow (press) | `#3567a8` | Inverted — sunken appearance   |

## Battle Sprite Tints

| State          | P1 Fill     | P2 Fill     |
|----------------|-------------|-------------|
| Normal         | `#e94560`   | `#0f3460`   |
| Startup frames | `#ff809a`   | `#2060b0`   |
| Active / Flash | `#ffffff`   | `#ffffff`   |

## Fonts

| Context              | Family           | Size | Weight | Notes                      |
|----------------------|------------------|------|--------|----------------------------|
| Nav buttons          | Consolas         | 14   | Bold   | Monospaced pixel-art feel  |
| Nav button icons     | Segoe UI Emoji   | 30   | Normal | Tag property               |
| Battle HP / names    | (Segoe UI)       | 14   | Bold   | WPF default                |
| Battle stats line    | (Segoe UI)       | 11   | Normal |                            |
| Battle timer         | (Segoe UI)       | 32   | Bold   |                            |
| FIGHT! banner        | (Segoe UI)       | 52   | Bold   |                            |
| K.O. banner          | (Segoe UI)       | 48   | Bold   |                            |
| Match overlay text   | (Segoe UI)       | 36   | Bold   |                            |
| Slide title          | (Segoe UI)       | 26   | Bold   |                            |
| Slide body           | (Segoe UI)       | 15   | Normal | LineHeight 24              |
| Slide indicator      | (Segoe UI)       | 12   | SemiBold |                          |

> WPF default is **Segoe UI** on Windows. No explicit FontFamily needed unless overriding.

## Animation Timings

| Effect                  | Duration | Easing Function         |
|-------------------------|----------|-------------------------|
| Slide crossfade         | 120 ms   | Linear                  |
| Carlos logo fade-in     | 700 ms   | QuadraticEase EaseIn    |
| Carlos logo slam-in     | 650 ms   | BackEase EaseOut (0.35) |
| Carlos logo glow pulse  | 1600 ms  | SineEase EaseInOut      |
| Carlos logo scale pulse | 1600 ms  | SineEase EaseInOut      |
| LogoPerron drop         | 500 ms   | QuarticEase EaseIn      |
| Impact squash           | 65 ms    | Linear                  |
| Elastic restore         | 420 ms   | ElasticEase EaseOut     |
| Screen crossfade        | 450–500 ms | Linear                |
| FIGHT! banner fade-in   | 300 ms   | Linear                  |
| FIGHT! banner fade-out  | 300 ms   | Linear (delayed 500 ms) |

## Nav Screen Gradient

```
Top    → #1a1a2e  (Background)
Middle → #16213e  (Panel)
Bottom → #0f3460  (Deep Blue)
Direction: vertical (StartPoint="0,0" EndPoint="0,1")
```
