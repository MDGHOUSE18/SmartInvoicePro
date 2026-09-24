import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  adminOnly?: boolean;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {
  private readonly auth = inject(AuthService);

  @Input() collapsed = false;
  @Output() toggle = new EventEmitter<void>();

  private readonly navItems: NavItem[] = [
    { label: 'Dashboard', icon: '📊', route: '/dashboard' },
    { label: 'Customers', icon: '👥', route: '/customers' },
    { label: 'Products', icon: '📦', route: '/products' },
    { label: 'Invoices', icon: '📄', route: '/invoices' },
    { label: 'Payments', icon: '💳', route: '/payments' },
    { label: 'Reports', icon: '📈', route: '/reports', adminOnly: true },
    { label: 'Settings', icon: '⚙️', route: '/settings' },
    { label: 'Audit Logs', icon: '🔍', route: '/audit-logs', adminOnly: true },
    { label: 'Demo Reset', icon: '🔄', route: '/demo', adminOnly: true },
  ];

  visibleNavItems(): NavItem[] {
    return this.navItems.filter((item) => !item.adminOnly || this.auth.isAdmin());
  }
}
