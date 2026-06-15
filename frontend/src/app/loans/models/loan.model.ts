export interface Loan {
  id: string;
  amount: number;
  currentBalance: number;
  applicantName: string;
  status: LoanStatus;
  createdAt: string;
  updatedAt: string;
}

export type LoanStatus = 'active' | 'paid';

export interface CreateLoanRequest {
  amount: number;
  currentBalance?: number;
  applicantName: string;
  status?: LoanStatus;
}

export interface PaymentRequest {
  amount: number;
}

export interface ApiError {
  statusCode: number;
  message: string;
  details?: string[];
}
