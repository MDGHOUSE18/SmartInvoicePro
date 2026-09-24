import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { Dashboard } from '../models/dashboard.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly api = inject(ApiService);

  get(): Observable<Dashboard> {
    return this.api.get<Dashboard>('dashboard');
  }
}
