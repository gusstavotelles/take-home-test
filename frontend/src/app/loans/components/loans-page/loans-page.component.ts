import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';

import { Loan } from '../../models/loan.model';
import { LoanStore } from '../../store/loan.store';
import { CreateLoanDialogComponent } from '../create-loan-dialog/create-loan-dialog.component';
import { PaymentDialogComponent } from '../payment-dialog/payment-dialog.component';

@Component({
  selector: 'app-loans-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
  templateUrl: './loans-page.component.html',
  styleUrls: ['./loans-page.component.scss'],
})
export class LoansPageComponent implements OnInit {
  private readonly dialog = inject(MatDialog);
  protected readonly store = inject(LoanStore);

  readonly displayedColumns = [
    'applicantName',
    'amount',
    'currentBalance',
    'status',
    'actions',
  ];

  ngOnInit(): void {
    void this.store.load();
  }

  openCreateDialog(): void {
    this.dialog.open(CreateLoanDialogComponent, {
      autoFocus: 'first-tabbable',
    });
  }

  openPaymentDialog(loan: Loan): void {
    if (loan.status === 'paid') {
      return;
    }

    this.dialog.open(PaymentDialogComponent, {
      data: { loan },
      autoFocus: 'first-tabbable',
    });
  }

  refresh(): void {
    void this.store.load();
  }

  trackById(_index: number, loan: Loan): string {
    return loan.id;
  }
}
