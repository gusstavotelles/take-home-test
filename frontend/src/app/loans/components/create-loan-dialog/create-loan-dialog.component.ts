import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Output,
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
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { LoanStore } from '../../store/loan.store';

@Component({
  selector: 'app-create-loan-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './create-loan-dialog.component.html',
  styleUrls: ['./create-loan-dialog.component.scss'],
})
export class CreateLoanDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(LoanStore);
  private readonly dialogRef = inject(MatDialogRef<CreateLoanDialogComponent>);

  readonly submitting = signal(false);
  readonly serverError = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    applicantName: ['', [Validators.required, Validators.maxLength(200)]],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    currentBalance: [null as number | null],
  });

  async submit(): Promise<void> {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.serverError.set(null);

    const value = this.form.getRawValue();
    const created = await this.store.create({
      applicantName: value.applicantName.trim(),
      amount: value.amount,
      currentBalance:
        value.currentBalance === null || value.currentBalance === undefined
          ? undefined
          : Number(value.currentBalance),
    });

    this.submitting.set(false);

    if (created) {
      this.dialogRef.close(created);
    } else {
      this.serverError.set(this.store.error() ?? 'Could not create loan.');
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
