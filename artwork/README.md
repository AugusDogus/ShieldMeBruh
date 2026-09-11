# Banner artwork

The README banner follows Buildheim's flat forest-green background and cream
lettering. It uses the existing shield and sword artwork from
`ShieldMeBruhReforged/Resources/shield.png`, originally created for Vapok's
ShieldMeBruh. The original image is embedded unchanged in the SVG, with its
transparent margins excluded from the visible frame.

The title uses DejaVu Serif Bold, with DejaVu Sans for the supporting text.
Lettering is converted to paths for consistent rendering. Font notices are
included in `FONT-LICENSE.txt`; the original artwork remains covered by the
repository's MIT license.

Render the PNG from the repository root with librsvg:

```sh
rsvg-convert artwork/banner.svg -o banner.png
```
