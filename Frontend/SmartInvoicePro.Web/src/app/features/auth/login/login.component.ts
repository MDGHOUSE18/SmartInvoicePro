import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { getApiErrorMessage } from '../../../core/utils/api-error';
import { AuthService } from '../../../core/services/auth.service';
import { WELCOME_FLAG_KEY } from '../../../shared/components/welcome-dialog/welcome-dialog.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal('');

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');
    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.loading.set(false);
        sessionStorage.setItem(WELCOME_FLAG_KEY, '1');
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(getApiErrorMessage(err, 'Login failed'));
      },
    });
  }
}
