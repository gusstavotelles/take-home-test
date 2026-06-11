import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  CreateLoanRequest,
  Loan,
  PaymentRequest,
} from '../models/loan.model';

@Injectable({ providedIn: 'root' })
export class LoanService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/loans`;

  getAll(): Observable<Loan[]> {
    return this.http.get<Loan[]>(this.baseUrl);
  }

  getById(id: string): Observable<Loan> {
    return this.http.get<Loan>(`${this.baseUrl}/${id}`);
  }

  create(payload: CreateLoanRequest): Observable<Loan> {
    return this.http.post<Loan>(this.baseUrl, payload);
  }

  applyPayment(id: string, payload: PaymentRequest): Observable<Loan> {
    return this.http.post<Loan>(`${this.baseUrl}/${id}/payment`, payload);
  }
}
