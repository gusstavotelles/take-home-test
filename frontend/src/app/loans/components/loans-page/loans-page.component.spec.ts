import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';

import { LoansPageComponent } from './loans-page.component';
import { LoanStore } from '../../store/loan.store';
import { Loan } from '../../models/loan.model';

class LoanStoreStub {
  load = jasmine.createSpy('load').and.resolveTo();
  create = jasmine.createSpy('create');
  applyPayment = jasmine.createSpy('applyPayment');
  clearError = jasmine.createSpy('clearError');

  private readonly _loans = signal<Loan[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);

  loans = this._loans.asReadonly();
  loading = this._loading.asReadonly();
  error = this._error.asReadonly();
  hasLoans = () => this._loans().length > 0;

  setLoans(loans: Loan[]): void {
    this._loans.set(loans);
  }

  setError(message: string | null): void {
    this._error.set(message);
  }

  setLoading(value: boolean): void {
    this._loading.set(value);
  }
}

describe('LoansPageComponent', () => {
  let component: LoansPageComponent;
  let fixture: ComponentFixture<LoansPageComponent>;
  let store: LoanStoreStub;
  let dialogOpenSpy: jasmine.Spy;

  beforeEach(async () => {
    store = new LoanStoreStub();

    await TestBed.configureTestingModule({
      imports: [LoansPageComponent],
      providers: [
        provideAnimationsAsync(),
        provideRouter([]),
        { provide: LoanStore, useValue: store },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoansPageComponent);
    component = fixture.componentInstance;

    const dialog = fixture.debugElement.injector.get(MatDialog);
    dialogOpenSpy = spyOn(dialog, 'open').and.returnValue({} as never);
  });

  it('triggers store load on init', () => {
    fixture.detectChanges();
    expect(store.load).toHaveBeenCalled();
  });

  it('renders the empty state when no loans', () => {
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('[data-testid="empty"]')).toBeTruthy();
  });

  it('renders the error state when store reports error', () => {
    store.setError('boom');
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('[data-testid="error"]')?.textContent).toContain(
      'boom',
    );
  });

  it('renders rows when loans are present', () => {
    store.setLoans([
      {
        id: '1',
        amount: 100,
        currentBalance: 50,
        applicantName: 'Alice',
        status: 'active',
        createdAt: '2025-01-01T00:00:00Z',
        updatedAt: '2025-01-01T00:00:00Z',
      },
    ]);
    fixture.detectChanges();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('table')).toBeTruthy();
    expect(el.textContent).toContain('Alice');
  });

  it('opens create dialog when New Loan is clicked', () => {
    fixture.detectChanges();
    component.openCreateDialog();
    expect(dialogOpenSpy).toHaveBeenCalled();
  });

  it('does not open payment dialog for paid loans', () => {
    component.openPaymentDialog({
      id: '1',
      amount: 0,
      currentBalance: 0,
      applicantName: 'X',
      status: 'paid',
      createdAt: '2025-01-01T00:00:00Z',
      updatedAt: '2025-01-01T00:00:00Z',
    });
    expect(dialogOpenSpy).not.toHaveBeenCalled();
  });
});
