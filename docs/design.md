# Design — NCalc Documentation

A locked design system for the NCalc DocFX site. Page-specific work extends this file instead of inventing a separate theme.

## Genre

Modern-minimal with a technical tone: precise navigation, strong code ergonomics, restrained motion, and an almost monochrome palette.

## Macrostructure family

- Documentation home: **Index-First** — introductory statement followed by categorized navigation rows.
- Guides and articles: **Long Document** — continuous, left-aligned reading flow with a 65–72ch measure.
- API reference: **Catalogue** — wider tabular and member-reference surfaces selected by DocFX YAML MIME metadata.
- Playground: **Workbench** — a compact product header, direct evaluator surface, and supporting parameter/history panels; the tool carries the page.

## Theme

The NCalc palette is graphite-first. Brand violet `#4E2ACA` anchors primary actions, while links use a slightly lighter derivative; focus rings and selected code tokens use softer variants. Surfaces and text remain achromatic.

- Light paper: `oklch(98% 0 0)`
- Light ink: `oklch(20% 0 0)`
- Light accent: `#4E2ACA` / `oklch(44.8% 0.2255 282.72)`
- Dark paper: `oklch(12% 0 0)`
- Dark ink: `oklch(95% 0 0)`
- Dark accent: `#4E2ACA` / `oklch(44.8% 0.2255 282.72)`

## Typography

- Display: Space Grotesk, weights 600–700, normal style, `-0.025em` tracking.
- Body: Source Sans 3, weights 400–700.
- Code and keyboard hints: JetBrains Mono, weights 400–600.
- Fonts are self-hosted WOFF2 assets with `font-display: swap`.
- Headings are always roman; code and numeric reference data use tabular figures where applicable.

## Spacing and shape

- A named 4-point scale from `--space-3xs` through `--space-3xl`.
- Controls use 6px radii; bounded content surfaces use 10px radii.
- Hairline rules and negative space establish depth. Shadows are limited to a single whisper token.

## Motion and interaction

- Reading surfaces are static.
- Button feedback uses transform only; focus indicators appear instantly.
- Search uses the existing DocFX index inside a native dialog opened by its visible control or Ctrl/Cmd+K.
- Reduced motion collapses spatial transitions to at most 150ms.
- All touch targets are at least 44px and all hover affordances have keyboard equivalents.

## CTA voice

- Primary: `#4E2ACA` fill in both modes, 6px radius, destination-specific label.
- Secondary: paper surface, visible rule, destination-specific label.
- Never use “Click here”, generic “Learn more”, gradients, or pill-shaped marketing controls.

## What pages must share

- NCalc logo retains its original brand mark; the surrounding interface stays graphite.
- Typography, spacing, focus, color, radius, and motion tokens.
- Search-first header, sidebar behavior, article heading rhythm, and inline-rule footer.
- Light/dark parity and the same Bootstrap Icons set.

## What pages may differ

- Article measure versus API-reference width.
- Presence of side navigation and in-page outline.
- Home-page index rows and the functional playground frame.

## Playground application

The standalone Blazor playground reuses this token and type system. Its header uses an edge-aligned product context, its evaluator leads the page, and its parameter and history surfaces collapse into mobile-native lists rather than shrinking desktop tables. The docs and playground intentionally share brand, footer voice, theme behavior, controls, and interaction states.

## Exports

### tokens.css

The canonical, complete export is [`themes/ncalc/public/tokens.css`](themes/ncalc/public/tokens.css). It defines both color modes, typography, spacing, type scale, rules, radii, durations, easing, and z-index roles.

### Tailwind v4 `@theme`

```css
@theme {
  --color-paper: oklch(98% 0 0);
  --color-ink: oklch(20% 0 0);
  --color-accent: oklch(44.8% 0.2255 282.72);
  --font-display: "Space Grotesk", sans-serif;
  --font-body: "Source Sans 3", sans-serif;
  --font-mono: "JetBrains Mono", monospace;
  --spacing-sm: 1rem;
  --spacing-md: 1.5rem;
  --spacing-lg: 2rem;
  --radius-input: 0.375rem;
  --radius-card: 0.625rem;
  --ease-out: cubic-bezier(0.16, 1, 0.3, 1);
}
```

### DTCG `tokens.json`

```json
{
  "$schema": "https://design-tokens.github.io/community-group/format/",
  "color": {
    "paper": { "$value": "oklch(98% 0 0)", "$type": "color" },
    "ink": { "$value": "oklch(20% 0 0)", "$type": "color" },
    "accent": { "$value": "oklch(44.8% 0.2255 282.72)", "$type": "color" }
  },
  "font": {
    "display": { "$value": "Space Grotesk, sans-serif", "$type": "fontFamily" },
    "body": { "$value": "Source Sans 3, sans-serif", "$type": "fontFamily" },
    "mono": { "$value": "JetBrains Mono, monospace", "$type": "fontFamily" }
  },
  "space": {
    "sm": { "$value": "1rem", "$type": "dimension" },
    "md": { "$value": "1.5rem", "$type": "dimension" },
    "lg": { "$value": "2rem", "$type": "dimension" }
  }
}
```

### shadcn/ui CSS variables

```css
:root {
  --background: 98% 0 0;
  --foreground: 20% 0 0;
  --card: 95.5% 0 0;
  --card-foreground: 20% 0 0;
  --primary: 44.8% 0.2255 282.72;
  --primary-foreground: 98% 0 0;
  --muted: 92.5% 0 0;
  --muted-foreground: 40% 0 0;
  --border: 84% 0 0;
  --input: 84% 0 0;
  --ring: 58% 0.15 282.72;
  --radius: 0.375rem;
}

.dark {
  --background: 12% 0 0;
  --foreground: 95% 0 0;
  --card: 16% 0 0;
  --card-foreground: 95% 0 0;
  --primary: 44.8% 0.2255 282.72;
  --primary-foreground: 98% 0 0;
  --muted: 21% 0 0;
  --muted-foreground: 73% 0 0;
  --border: 30% 0 0;
  --input: 30% 0 0;
  --ring: 58% 0.16 282.72;
}
```
