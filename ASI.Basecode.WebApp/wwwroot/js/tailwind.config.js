/* AttendTa v2 • Tailwind config (Play CDN)
   Load order: Play CDN <script> FIRST, then this file via <script src>.
   Dev-only: the Razor port replaces this; legacy numeric aliases removed at port. */

tailwind.config = {
  theme: {
    extend: {
      fontFamily: {
        sans: ['var(--font-body)'],
        mono: ['var(--font-mono)'],
        serif: ['var(--font-ceremonial)'],
      },
      colors: {
        ink: {
          DEFAULT: 'var(--ink-primary)',
          primary: 'var(--ink-primary)',
          secondary: 'var(--ink-secondary)',
          muted: 'var(--ink-muted)',
          faint: 'var(--ink-faint)',
          inverse: 'var(--ink-inverse)',
          950: 'var(--ink-primary)',
          900: 'var(--canvas-dark)',
          700: 'var(--ink-secondary)',
          500: 'var(--ink-muted)',
        },
        canvas: {
          ground: 'var(--canvas-ground)',
          surface: 'var(--canvas-surface)',
          subtle: 'var(--canvas-subtle)',
          elevated: 'var(--canvas-elevated)',
          dark: 'var(--canvas-dark)',
        },
        scholastic: {
          DEFAULT: 'var(--accent-amber)',
          deep: 'var(--accent-amber-deep)',
          subtle: 'var(--accent-amber-subtle)',
          glow: 'var(--accent-amber-glow)',
          500: 'var(--accent-amber)',
          600: 'var(--accent-amber-deep)',
          50: 'var(--accent-amber-subtle)',
        },
        status: {
          emerald: 'var(--status-emerald)',
          'emerald-text': 'var(--status-emerald-text)',
          'emerald-subtle': 'var(--status-emerald-subtle)',
          amber: 'var(--status-amber)',
          'amber-subtle': 'var(--status-amber-subtle)',
          rose: 'var(--status-rose)',
          'rose-subtle': 'var(--status-rose-subtle)',
          neutral: 'var(--status-neutral)',
          'neutral-subtle': 'var(--status-neutral-subtle)',
        },
        event: {
          moss: {
            DEFAULT: 'var(--event-moss)',
            deep: 'var(--event-moss-deep)',
            subtle: 'var(--event-moss-subtle)',
          },
          indigo: {
            DEFAULT: 'var(--event-indigo)',
            deep: 'var(--event-indigo-deep)',
            subtle: 'var(--event-indigo-subtle)',
          },
          teal: {
            DEFAULT: 'var(--event-teal)',
            deep: 'var(--event-teal-deep)',
            subtle: 'var(--event-teal-subtle)',
          },
          violet: {
            DEFAULT: 'var(--event-violet)',
            deep: 'var(--event-violet-deep)',
            subtle: 'var(--event-violet-subtle)',
          },
          crimson: {
            DEFAULT: 'var(--event-crimson)',
            deep: 'var(--event-crimson-deep)',
            subtle: 'var(--event-crimson-subtle)',
          },
          orange: {
            DEFAULT: 'var(--event-orange)',
            deep: 'var(--event-orange-deep)',
            subtle: 'var(--event-orange-subtle)',
          },
          slate: {
            DEFAULT: 'var(--event-slate)',
            deep: 'var(--event-slate-deep)',
            subtle: 'var(--event-slate-subtle)',
          },
          amber: {
            DEFAULT: 'var(--event-amber)',
            deep: 'var(--event-amber-deep)',
            subtle: 'var(--event-amber-subtle)',
          },
        },
      },
      transitionDuration: {
        fast: '120ms',
        base: '180ms',
        slow: '240ms',
      },
      transitionTimingFunction: {
        standard: 'cubic-bezier(0.2, 0, 0, 1)',
      },
      zIndex: {
        nav: '30',
        sticky: '40',
        drawer: '50',
        modal: '60',
      },
    },
    borderRadius: {
      none: '0px',
      DEFAULT: 'var(--radius-sm)',
      sm: 'var(--radius-sm)',
      md: 'var(--radius-md)',
      lg: 'var(--radius-lg)',
      pill: 'var(--radius-pill)',
    },
    boxShadow: {
      none: 'none',
      tactile: 'var(--shadow-tactile)',
      subtle: 'var(--shadow-subtle)',
      modal: 'var(--shadow-modal)',
      flash: 'var(--shadow-flash)',
    },
    transitionProperty: {
      none: 'none',
      DEFAULT: 'color, background-color, border-color, opacity, box-shadow, transform',
    },
  },
};
