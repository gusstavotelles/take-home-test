import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';

import { LoanStore } from './loan.store';
import { LoanService } from '../services/loan.service';
import { Loan } from '../models/loan.model';

describe('LoanStore', () => {
  let store: LoanStore;
  let service: jasmine.SpyObj<LoanService>;

  const buildLoan = (overrides: Partial<Loan> = {}): Loan => ({
    id: overrides.id ?? 'loan-1',
    amount: 1000,
    currentBalance: 1000,
    applicantName: 'A',
    status: 'active',
    createdAt: '2025-01-01T00:00:00Z',
    updatedAt: '2025-01-01T00:00:00Z',
    ...overrides,
  });

  beforeEach(() => {
    service = jasmine.createSpyObj<LoanService>('LoanService', [
      'getAll',
      'getById',
      'create',
      'applyPayment',
    ]);

    TestBed.configureTestingModule({
      providers: [LoanStore, { provide: LoanService, useValue: service }],
    });

    store = TestBed.inject(LoanStore);
  });

  it('load() populates loans on success', async () => {
    const loans = [buildLoan()];
    service.getAll.and.returnValue(of(loans));

    await store.load();

    expect(store.loans()).toEqual(loans);
    expect(store.loading()).toBeFalse();
    expect(store.error()).toBeNull();
  });

  it('load() captures backend error message', async () => {
    service.getAll.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 500,
            error: { statusCode: 500, message: 'boom', details: ['x'] },
          }),
      ),
    );

    await store.load();

    expect(store.loans()).toEqual([]);
    expect(store.error()).toBe('boom: x');
    expect(store.loading()).toBeFalse();
  });

  it('create() prepends new loan to the list', async () => {
    const initial = buildLoan({ id: 'old' });
    service.getAll.and.returnValue(of([initial]));
    await store.load();

    const created = buildLoan({ id: 'new' });
    service.create.and.returnValue(of(created));

    const result = await store.create({ amount: 10, applicantName: 'New' });

    expect(result).toEqual(created);
    expect(store.loans()[0].id).toBe('new');
    expect(store.loans()).toHaveSize(2);
  });

  it('applyPayment() replaces the matching loan', async () => {
    const initial = buildLoan({ id: 'p', currentBalance: 500 });
    service.getAll.and.returnValue(of([initial]));
    await store.load();

    const updated = buildLoan({ id: 'p', currentBalance: 300 });
    service.applyPayment.and.returnValue(of(updated));

    await store.applyPayment('p', { amount: 200 });

    expect(store.loans()[0].currentBalance).toBe(300);
  });

  it('applyPayment() exposes server error', async () => {
    service.applyPayment.and.returnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: { statusCode: 409, message: 'exceeds balance' },
          }),
      ),
    );

    const result = await store.applyPayment('p', { amount: 10 });

    expect(result).toBeNull();
    expect(store.error()).toBe('exceeds balance');
  });
});
