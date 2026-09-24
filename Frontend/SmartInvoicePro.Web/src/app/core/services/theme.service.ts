import { Injectable, computed, signal, effect } from '@angular/core';

export type ThemeMode = 'light' | 'dark';
const THEME_KEY = 'sip_theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly themeSignal = signal<ThemeMode>(this.loadTheme());
  readonly theme = this.themeSignal.asReadonly();
  readonly isDark = computed(() => this.themeSignal() === 'dark');

  constructor() {
    this.applyTheme(this.themeSignal());

    effect(() => {
      this.applyTheme(this.themeSignal());
    });
  }

  toggle(): void {
    this.themeSignal.update((t) => (t === 'light' ? 'dark' : 'light'));
  }

  setTheme(theme: ThemeMode): void {
    this.themeSignal.set(theme);
  }

  private applyTheme(theme: ThemeMode): void {
    if (typeof document === 'undefined') return;
    document.documentElement.setAttribute('data-theme', theme);
    document.documentElement.style.colorScheme = theme;
    localStorage.setItem(THEME_KEY, theme);
  }

  private loadTheme(): ThemeMode {
    if (typeof window === 'undefined') return 'light';
    const stored = localStorage.getItem(THEME_KEY) as ThemeMode | null;
    if (stored === 'light' || stored === 'dark') return stored;
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
}
