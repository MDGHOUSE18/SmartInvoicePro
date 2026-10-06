import { Component, Input } from '@angular/core';

export type SkeletonVariant = 'cards' | 'detail' | 'form' | 'list' | 'table';

// Varied line widths so placeholders read like real content rather than a grid of bars.
const WIDTHS = ['w-75', 'w-50', 'w-90', 'w-60', 'w-40'];

const range = (n: number): number[] => Array.from({ length: Math.max(0, n) }, (_, i) => i);
const widthFor = (i: number): string => WIDTHS[i % WIDTHS.length];

/** Shimmer placeholder shaped like the content it stands in for. Built on the global `.skeleton*` classes. */
@Component({
  selector: 'app-skeleton',
  standalone: true,
  templateUrl: './skeleton.component.html',
  styleUrl: './skeleton.component.scss',
  host: { 'aria-busy': 'true', 'aria-label': 'Loading', role: 'status' },
})
export class SkeletonComponent {
  @Input() variant: SkeletonVariant = 'cards';
  /** Cards, list items, form fields or detail panels. */
  @Input() count = 4;
  /** Rows per panel (detail) or per table. */
  @Input() rows = 5;
  @Input() cols = 4;

  protected readonly range = range;
  protected readonly widthFor = widthFor;
}

/** One shimmer table row; repeat it inside an existing `<tbody>` with `@for`. */
@Component({
  selector: 'tr[appSkeletonRow]',
  standalone: true,
  template: `@for (c of range(cols); track c) {
    <td><span class="skeleton skeleton-text" [class]="widthFor(row + c)"></span></td>
  }`,
  styles: [':host td { padding-top: 0.95rem; padding-bottom: 0.95rem; }'],
  host: { 'aria-hidden': 'true' },
})
export class SkeletonRowComponent {
  @Input() cols = 5;
  /** Row index, used only to vary line widths. */
  @Input() row = 0;

  protected readonly range = range;
  protected readonly widthFor = widthFor;
}
