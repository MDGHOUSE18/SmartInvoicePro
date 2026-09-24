import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { getApiErrorMessage } from '../../../core/utils/api-error';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-profile-home',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PageHeaderComponent],
  templateUrl: './profile-home.component.html',
  styleUrl: './profile-home.component.scss',
})
export class ProfileHomeComponent {
  readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly pwdLoading = signal(false);
  readonly pwdMessage = signal('');
  readonly pwdError = signal('');

  readonly pwdForm = this.fb.nonNullable.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
  });

  changePassword(): void {
    if (this.pwdForm.invalid) return;
    this.pwdLoading.set(true);
    this.auth.changePassword(this.pwdForm.getRawValue()).subscribe({
      next: (msg) => { this.pwdLoading.set(false); this.pwdMessage.set(msg ?? 'Password changed'); this.pwdForm.reset(); },
      error: (err) => { this.pwdLoading.set(false); this.pwdError.set(getApiErrorMessage(err, 'Failed')); },
    });
  }
}
