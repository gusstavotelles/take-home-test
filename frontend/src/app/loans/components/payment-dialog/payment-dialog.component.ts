import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { Loan } from '../../models/loan.model';
import { LoanStore } from '../../store/loan.store';

interface PaymentDialogData {
  loan: Loan;
}

@Component({
  selector: 'app-payment-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './payment-dialog.component.html',
  styleUrls: ['./payment-dialog.component.scss'],
})
export class PaymentDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(LoanStore);
  private readonly dialogRef = inject(MatDialogRef<PaymentDialogComponent>);
  readonly data = inject<PaymentDialogData>(MAT_DIALOG_DATA);

  readonly submitting = signal(false);
  readonly serverError = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    amount: [
      0,
      [Validators.required, Validators.min(0.01)],
    ],
  });

  get maxAmount(): number {
    return this.data.loan.currentBalance;
  }

  async submit(): Promise<void> {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const amount = this.form.controls.amount.value;
    if (amount > this.maxAmount) {
      this.serverError.set('Payment amount exceeds the current balance.');
      return;
    }

    this.submitting.set(true);
    this.serverError.set(null);

    const updated = await this.store.applyPayment(this.data.loan.id, { amount });

    this.submitting.set(false);

    if (updated) {
      this.dialogRef.close(updated);
    } else {
      this.serverError.set(this.store.error() ?? 'Could not apply payment.');
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
