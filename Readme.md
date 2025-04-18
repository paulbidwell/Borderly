# Borderly

Borderly watches a directory for JPEG, PNG or TIFF images, applies one or more border/resize profiles, and outputs processed files. You can optionally move or copy the original files after processing, else they will be deleted from the input directory.

---

## Configuration

All options live in **appsettings.json** (or your chosen JSON config file). Example:

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
      "BorderWidth": 50,
      "Quality": 100,
      "ResizeWidth": 0,
      "ResizeHeight": 0
    },
    {
      "Name": "BlackBorder50",
      "BorderWidth": 50,
      "BorderColour": "#000000",
      "Quality": 100,
      "ResizeWidthPercentage": 50,
      "ResizeHeightPercentage": 50
    }
  ]
}
```

### Settings

- **InputDirectory** (`string`)\
  Directory path to watch for new JPEG files.

- **OutputDirectory** (`string`)\
  Directory where processed images are saved. Profiles create subfolders here by their `Name`.

- **ProcessedFileOption** (`string`) — `Move` | `Copy` | `None`\
  What to do with the original file once processed:

  - `Move`: move the original into `ProcessedDirectory`.
  - `Copy`: copy the original into `ProcessedDirectory`.
  - `None`: leave originals untouched.

- **ProcessedDirectory** (`string`)\
  Destination for moved/copied originals (required if `ProcessedFileOption` is `Move` or `Copy`).

### Profiles

An array of profile objects; Borderly applies each profile to every image. Fields:

| Field                      | Type        | Required | Default   | Description                                                          |
| -------------------------- | ----------- | -------- | --------- | -------------------------------------------------------------------- |
| **Name**                   | string      | ✔        | —         | Identifier; used to name subfolder and suffix output files.          |
| **BorderWidth**            | int         | ✔        | —         | Width of the border in pixels.                                       |
| **BorderColour**           | string      | ✘        | `#FFFFFF` | Hex or CSS colour for border.                                        |
| **Quality**                | int (0–100) | ✔        | —         | JPEG quality for output.                                             |
| **ResizeWidth**            | int         | ✘        | skip      | Absolute target width in pixels (0 or omitted = no absolute resize). |
| **ResizeHeight**           | int         | ✘        | skip      | Absolute target height in pixels.                                    |
| **ResizeWidthPercentage**  | int (0–100) | ✘        | skip      | Resize width to this percent of original.                            |
| **ResizeHeightPercentage** | int (0–100) | ✘        | skip      | Resize height to this percent of original.                           |

> **Precedence**: Absolute (`ResizeWidth`/`ResizeHeight`) overrides percentage fields. Omit or set to 0 to skip resizing in that dimension.

---

## Prerequisites

- **.NET 9** SDK & runtime (tested on Windows; may work on Linux and macOS).

---

## Building

1. Clone or download the Borderly source code.

2. Open a terminal and navigate to the project root (where the `.csproj` lives).

3. Run:

   ```bash
   dotnet build -c Release
   ```

4. (Optional) Publish a self-contained build:

   ```bash
   dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
   ```

Compiled binaries will be under `bin/Release/net9.0/` (or your target RID if publishing).

---

## Usage

1. Ensure the .NET runtime is installed.
2. Place `Borderly.exe` (from `bin/Release/net9.0/`) and `appsettings.json` side by side.
3. Edit `appsettings.json` to configure your directories and profiles.
4. Run:
   ```bash
   Borderly.exe
   ```
   Or install it as a Windows Service:
   ```powershell
   sc create Borderly binPath= "C:\path\to\Borderly.exe"
   sc start Borderly
   ```

Processed images appear under:

```
<OutputDirectory>\<ProfileName>\<OriginalFileName>_<ProfileName>.jpg
```

Original files will be moved or copied to `ProcessedDirectory` if that option is enabled.

