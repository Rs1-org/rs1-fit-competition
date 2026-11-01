import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { CheckoutSession, PremiumPlanMonths, PremiumPricing, PremiumStatus } from '../models/payment';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/api/payment`;
  getPricing() { return this.http.get<PremiumPricing>(`${this.api}/pricing`); }
  getStatus() { return this.http.get<PremiumStatus>(`${this.api}/status`); }
  checkout(months: PremiumPlanMonths) { return this.http.post<CheckoutSession>(`${this.api}/checkout`, { months }); }
}
