import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';

import { LoanService } from './loan.service';
import { environment } from '../../../environments/environment';
import { Loan } from '../models/loan.model';

describe('LoanService', () => {
  let service: LoanService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/loans`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(LoanService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('GET /loans returns a list', () => {
    const loans: Loan[] = [
      {
        id: 'a',
        amount: 100,
        currentBalance: 50,
        applicantName: 'A',
        status: 'active',
        createdAt: '2025-01-01T00:00:00Z',
        updatedAt: '2025-01-01T00:00:00Z',
      },
    ];
    service.getAll().subscribe(result => expect(result).toEqual(loans));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush(loans);
  });

  it('POST /loans sends body and returns created loan', () => {
    const created: Loan = {
      id: 'a',
      amount: 100,
      currentBalance: 100,
      applicantName: 'New',
      status: 'active',
      createdAt: '2025-01-01T00:00:00Z',
      updatedAt: '2025-01-01T00:00:00Z',
    };
    service
      .create({ amount: 100, applicantName: 'New' })
      .subscribe(result => expect(result).toEqual(created));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ amount: 100, applicantName: 'New' });
    req.flush(created);
  });

  it('POST /loans/{id}/payment posts to the right url', () => {
    service.applyPayment('id-1', { amount: 50 }).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/id-1/payment`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ amount: 50 });
    req.flush({});
  });
});
