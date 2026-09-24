import { Injectable, computed, signal } from '@angular/core';

export type DayPeriod = 'morning' | 'afternoon' | 'evening' | 'night';

const PERIOD_GREETING: Record<DayPeriod, { text: string; icon: string }> = {
  morning: { text: 'Good morning', icon: '🌅' },
  afternoon: { text: 'Good afternoon', icon: '☀️' },
  evening: { text: 'Good evening', icon: '🌇' },
  night: { text: 'Good night', icon: '🌙' },
};

/** App-wide ticking clock based on the browser's local time zone. */
@Injectable({ providedIn: 'root' })
export class ClockService {
  private readonly nowSignal = signal(new Date());
  readonly now = this.nowSignal.asReadonly();

  private readonly timeFormat = new Intl.DateTimeFormat(undefined, { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  private readonly shortDateFormat = new Intl.DateTimeFormat(undefined, { weekday: 'short', day: '2-digit', month: 'short', year: 'numeric' });
  private readonly longDateFormat = new Intl.DateTimeFormat(undefined, { dateStyle: 'full' });

  readonly timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
  readonly timeZoneShort = this.resolveShortZoneName();
  /** City derived from the IANA zone, e.g. "Asia/Kolkata" -> "Kolkata". */
  readonly location = (this.timeZone.split('/').pop() ?? this.timeZone).replace(/_/g, ' ');

  readonly period = computed<DayPeriod>(() => {
    const hour = this.nowSignal().getHours();
    if (hour >= 5 && hour < 12) return 'morning';
    if (hour >= 12 && hour < 17) return 'afternoon';
    if (hour >= 17 && hour < 21) return 'evening';
    return 'night';
  });
  readonly greeting = computed(() => PERIOD_GREETING[this.period()].text);
  readonly greetingIcon = computed(() => PERIOD_GREETING[this.period()].icon);
  readonly timeText = computed(() => this.timeFormat.format(this.nowSignal()));
  readonly dateText = computed(() => this.shortDateFormat.format(this.nowSignal()));
  readonly longDateText = computed(() => this.longDateFormat.format(this.nowSignal()));

  constructor() {
    setInterval(() => this.nowSignal.set(new Date()), 1000);
  }

  private resolveShortZoneName(): string {
    const part = new Intl.DateTimeFormat(undefined, { timeZoneName: 'short' })
      .formatToParts(new Date())
      .find((p) => p.type === 'timeZoneName');
    return part?.value ?? '';
  }
}
