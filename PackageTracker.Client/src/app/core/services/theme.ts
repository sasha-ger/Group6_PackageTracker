import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  isDark = signal<boolean>(true);

  constructor() {
    const saved = localStorage.getItem('theme');
    const prefersDark = saved ? saved === 'dark' : true;
    this.isDark.set(prefersDark);
    this.applyTheme(prefersDark);
  }

  toggle() {
    const newVal = !this.isDark();
    this.isDark.set(newVal);
    localStorage.setItem('theme', newVal ? 'dark' : 'light');
    this.applyTheme(newVal);
  }

  private applyTheme(dark: boolean) {
    document.body.classList.toggle('dark-theme', dark);
    document.body.classList.toggle('light-theme', !dark);
  }
}