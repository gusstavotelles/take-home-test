import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import {
  ApiError,
  CreateLoanRequest,
  Loan,
  PaymentRequest,
} from '../models/loan.model';
import { LoanService } from '../services/loan.service';

@Injectable({ providedIn: 'root' })
export class LoanStore {
  private readonly loanService = inject(LoanService);

  private readonly _loans = signal<Loan[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);

  readonly loans = this._loans.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly hasLoans = computed(() => this._loans().length > 0);

  async load(): Promise<void> {
    this._loading.set(true);
    this._error.set(null);
    try {
      const loans = await firstValueFrom(this.loanService.getAll());
      this._loans.set(loans ?? []);
    } catch (error) {
      this._error.set(this.formatError(error));
    } finally {
      this._loading.set(false);
    }
  }

  async create(payload: CreateLoanRequest): Promise<Loan | null> {
    this._loading.set(true);
    this._error.set(null);
    try {
      const created = await firstValueFrom(this.loanService.create(payload));
      this._loans.update(current => [created, ...current]);
      return created;
    } catch (error) {
      this._error.set(this.formatError(error));
      return null;
    } finally {
      this._loading.set(false);
    }
  }

  async applyPayment(id: string, payload: PaymentRequest): Promise<Loan | null> {
    this._loading.set(true);
    this._error.set(null);
    try {
      const updated = await firstValueFrom(
        this.loanService.applyPayment(id, payload),
      );
      this._loans.update(current =>
        current.map(loan => (loan.id === id ? updated : loan)),
      );
      return updated;
    } catch (error) {
      this._error.set(this.formatError(error));
      return null;
    } finally {
      this._loading.set(false);
    }
  }

  clearError(): void {
    this._error.set(null);
  }

  private formatError(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const apiError = error.error as ApiError | undefined;
      if (apiError?.message) {
        const details = apiError.details?.length
          ? `: ${apiError.details.join(', ')}`
          : '';
        return `${apiError.message}${details}`;
      }
      return error.message;
    }
    return 'Unexpected error.';
  }
}
