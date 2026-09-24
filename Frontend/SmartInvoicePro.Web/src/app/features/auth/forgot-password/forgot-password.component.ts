import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { getApiErrorMessage } from '../../../core/utils/api-error';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss',
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  readonly loading = signal(false);
  readonly error = signal('');
  readonly message = signal('');
  readonly demoResetToken = signal('');

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');
    this.message.set('');
    this.demoResetToken.set('');
    this.auth.forgotPassword(this.form.getRawValue()).subscribe({
      next: (result) => {
        this.loading.set(false);
        this.message.set(
          result.message ??
            'If the email exists, a reset link has been sent (demo: logged to EmailLogs).'
        );
        if (result.resetToken) {
          this.demoResetToken.set(result.resetToken);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(getApiErrorMessage(err, 'Request failed'));
      },
    });
  }
}
