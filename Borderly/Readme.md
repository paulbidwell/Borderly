# Borderly

Borderly watches a directory for JPEG, PNG, or TIFF images, applies one or more border/resize profiles, and outputs processed files. Originals can be moved, copied, or deleted after processing.

---

## Configuration

All options live in **appsettings.json** (or your chosen JSON file). Example:

```json
{
  "Settings": {
    "InputDirectory": "D:\\Images\\Input",
    "OutputDirectory": "D:\\Images\\Output",
    "ProcessedFileOption": "Move",
    "ProcessedDirectory": "D:\\Images\\Processed"
  },
  "Profiles": [
    {
      "Name": "WhiteBorder",
      "BorderWidth": "50px",
      "Quality": 100
    },
    {
      "Name": "BlackBorder5",
      "BorderWidth": "5%",
      "BorderColour": "#000000",
      "Quality": 50,
      "ResizeWidth": "50%",
      "ResizeHeight": "50%"
    }
  ]
}
```

### Settings

- **InputDirectory** (`string`)  
  Directory to watch for new image files (JPEG, PNG, TIFF).

- **OutputDirectory** (`string`)  
  Directory where processed images are saved. Each profile creates its own subfolder.

- **ProcessedFileOption** (`string`) — `Move` | `Copy` | `None`  
  What to do with originals after processing:
  - `Move`: move into `ProcessedDirectory`.
  - `Copy`: copy into `ProcessedDirectory`.
  - `None`: delete from input.

- **ProcessedDirectory** (`string`)  
  Destination for moved/copied originals (required when `Move` or `Copy`).

### Profiles

An array of profile objects; Borderly applies each profile to every image. Fields:

| Field            | Type          | Required | Default   | Description                                                                                                      |
| ---------------- | ------------- | -------- | --------- | ---------------------------------------------------------------------------------------------------------------- |
| **Name**         | string        | ✓        | —         | Identifier; names the output subfolder and filename suffix.                                                      |
| **BorderWidth**  | string        | ✓        | —         | Thickness: pixels (`"10px"`) or percentage (`"5%"`, if applicable based on post-resize size).                |
| **BorderColour** | string        | ✗        | `#FFFFFF` | Hex or CSS color for the border.                                                                                 |
| **Quality**      | int (0–100)   | ✓        | —         | JPEG quality for output (ignored for PNG/TIFF).                                                                  |
| **ResizeWidth**  | string        | ✗        | skip      | Width as pixels (`"200px"`) or percentage (`"50%"`, based on original image size).                           |
| **ResizeHeight** | string        | ✗        | skip      | Height as pixels or percentage.                                                                                  |

> **Note**: Percentage values for **BorderWidth** apply to the image’s final (post-resize) dimensions; percentage for **ResizeWidth**/**ResizeHeight** apply to the original image.

---

## Prerequisites

- **.NET 9** SDK & runtime (tested on Windows; likely works on Linux/macOS).

---

## Building

1. Clone or download the source.
2. In a terminal, navigate to the project root (where the `.csproj` resides).
3. Run:
   ```bash
   dotnet build -c Release
   ```
4. (Optional) Publish a self-contained build:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
   ```

Binaries appear under `bin/Release/net9.0/` (or your target RID).

---

## Usage

1. Install the .NET runtime if needed.
2. Place `Borderly.exe` (from `bin/Release/net9.0/`) and `appsettings.json` together.
3. Edit `appsettings.json` as desired.
4. Run:
   ```bash
   Borderly.exe
   ```
   Or install as a Windows Service:
   ```powershell
   sc create Borderly binPath= "C:\path\to\Borderly.exe"
   sc start Borderly
   ```

Processed files live in:
```
<OutputDirectory>\<ProfileName>\<OriginalName>_<ProfileName>.jpg
```

Originals move or copy into `ProcessedDirectory` if enabled.