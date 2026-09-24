import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { CompanySettings, UpdateCompanySettingsRequest } from '../models/company-settings.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CompanySettingsService {
  private readonly api = inject(ApiService);

  get(): Observable<CompanySettings> {
    return this.api.get<CompanySettings>('companysettings');
  }

  update(dto: UpdateCompanySettingsRequest): Observable<CompanySettings> {
    return this.api.put<CompanySettings>('companysettings', dto);
  }
}
