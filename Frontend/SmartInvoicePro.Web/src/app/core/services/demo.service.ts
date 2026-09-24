import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DemoService {
  private readonly api = inject(ApiService);

  reset(): Observable<string | null> {
    return this.api.postEmpty('demo/reset');
  }
}
