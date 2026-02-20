# Palette Swap for Unity 2D

This project implements a palette swapping system for Unity 2D games, allowing dynamic switching between day and night color schemes for sprites.

## Screenshots

<img src="ScreenShots/Palettes_Sprite.png" alt="Palettes and Sprite" width="400">

<img src="ScreenShots/Shader.png" alt="Shader Setup" width="400">

<img src="ScreenShots/ShowOff.gif" alt="Show Off" width="400">

## Requirements

- Unity 6.3 or compatible version
- Unity 2D project

## How It Works

This palette swapping system is designed for pixel-style 2D games (not yet compatible with non-pixel art). It works by replacing colors in a sprite's texture with corresponding colors from a palette texture. Each palette is a 1-pixel high texture where the width represents the number of colors (e.g., 8 colors = 8 pixels wide).

### Key Components

1. **PaletteSwap.shader**: A custom shader that performs the color replacement in real-time.
2. **PaletteSwapManager.cs**: Manages the day/night state and handles transitions with a configurable duration.
3. **PaletteSwapper.cs**: Applies the palette swap to individual sprites and ensures palette consistency.

### Palette Format

- **Height**: Must be exactly 1 pixel
- **Width**: Can be any size (number of colors), but day and night palettes must have identical widths and color mappings
- Each pixel represents one color in the palette
- **Important**: The day palette must contain the exact colors used in the sprite texture. The night palette should have corresponding alternative colors in the same positions. Day and night palettes must match exactly in size and color positions. Mismatched palettes may produce undefined visual results.

## Setup Instructions

### 1. Import the Assets

Add the following files to your Unity project:
- `PaletteSwap.shader`
- `PaletteSwapManager.cs`
- `PaletteSwapper.cs`

### 2. Create Palette Textures

1. Create two palette textures (PNG format recommended):
   - Day palette: Contains the exact same colors as the sprite you want to palette swap (original/daytime colors)
   - Night palette: Contains the nighttime/alternative colors corresponding to each day color
2. Ensure both palettes have:
   - Height of 1 pixel
   - Identical width (same number of colors)
   - Import settings in Unity:
     - **Texture Type**: Sprite (2D and UI)
     - **Wrap Mode**: Clamp
     - **Filter Mode**: Point (no filter)
     - **Format**: Any
     - **Compression**: None

### 3. Create the Material

1. Create a new Material in Unity
2. Set the Shader to `Custom/PaletteSwap`
3. In the Material properties:
   - **Sprite**: Assign the sprite texture you want to palette swap (ensure it has pixel-art friendly import settings: Texture Type = Sprite, Wrap Mode = Clamp, Filter Mode = Point, Compression = None)
   - **Day Palette**: Assign your day palette texture
   - **Night Palette**: Assign your night palette texture
   - **Palette Size**: Set to the width of your palettes (number of colors)
   - **Match Threshold**: Adjust sensitivity for color matching (default 0.01)
   - **Is Night**: This is controlled by the script (0 = day, 1 = night)

### 4. Set Up the Palette Swap Manager

1. Attach the `PaletteSwapManager` script to any GameObject in your scene (e.g., the player object)
2. Configure settings:
   - **Starting State**: Choose Day or Night
   - **Toggle Key**: Key to switch between day/night (default Tab)
   - **Transition Duration** (for now it acts as delay, but it will have mix transition)

### 5. Apply to Sprites

1. Select a SpriteRenderer in your scene
2. Attach the `PaletteSwapper` script to the same GameObject as the SpriteRenderer
3. Assign the Material you created to the SpriteRenderer's Material field
4. In the `PaletteSwapper` script:
   - **Day Palette**: Assign the day palette texture (must match the one in the material)
   - **Night Palette**: Assign the night palette texture (must match the one in the material)
   
   **Note**: The palettes assigned to the `PaletteSwapper` script must exactly match those in the material (No clue what happens when they are not matching).

## Usage

- Press the configured toggle key (default Tab) to initiate palette swapping
- The transition duration acts as a delay before the palette change takes effect
- Future updates will implement smooth mixing transitions instead of delays
- Multiple sprites can share the same palette setup by using matching materials and PaletteSwapper configurations

## Technical Details

The shader performs color matching by comparing each pixel of the sprite against the day palette colors. When a match is found (within the threshold), it replaces the color with the corresponding color from the night palette, interpolated based on the `_IsNight` value.

The `PaletteSwapManager` coordinates state changes and can be attached to any GameObject (such as the player). The `PaletteSwapper` ensures each sprite uses consistent palettes between its script and material.
