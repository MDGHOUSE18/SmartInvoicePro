import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CompanySettingsService } from '../../../core/services/company-settings.service';
import { AuthService } from '../../../core/services/auth.service';
import { getApiErrorMessage } from '../../../core/utils/api-error';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-settings-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PageHeaderComponent],
  templateUrl: './settings-form.component.html',
  styleUrl: './settings-form.component.scss',
})
export class SettingsFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly settingsService = inject(CompanySettingsService);
  private readonly auth = inject(AuthService);

  readonly loading = signal(false);
  readonly message = signal('');
  readonly error = signal('');
  readonly canEdit = this.auth.isAdmin;

  readonly form = this.fb.nonNullable.group({
    companyName: ['', Validators.required],
    address: [''],
    city: [''],
    state: [''],
    country: ['India'],
    postalCode: [''],
    phone: [''],
    email: [''],
    website: [''],
    taxNumber: [''],
    logoUrl: [''],
    defaultCurrency: ['INR'],
    termsAndConditions: [''],
    bankName: [''],
    bankAccountNumber: [''],
    bankIfsc: [''],
  });

  ngOnInit(): void {
    this.settingsService.get().subscribe({
      next: (s) => this.form.patchValue(s),
      error: (err) => this.error.set(getApiErrorMessage(err)),
    });
  }

  submit(): void {
    if (!this.canEdit() || this.form.invalid) return;
    this.loading.set(true);
    this.settingsService.update(this.form.getRawValue()).subscribe({
      next: () => { this.loading.set(false); this.message.set('Settings updated successfully'); },
      error: (err) => { this.loading.set(false); this.error.set(getApiErrorMessage(err, 'Save failed')); },
    });
  }
}
