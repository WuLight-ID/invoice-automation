import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvoiceService } from './services/invoice.service';
import { RouterOutlet } from '@angular/router';
import { NgIf } from '@angular/common';
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, NgIf],
  templateUrl: './app.component.html'
})
export class AppComponent {

  selectedFile: File | null = null;
  result: any;

  constructor(private invoiceService: InvoiceService) {}

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  upload() {
    if (!this.selectedFile) return;

    this.invoiceService.uploadInvoice(this.selectedFile)
      .subscribe({
        next: (res) => {
          this.result = res;
        },
        error: (err) => {
          console.error(err);
        }
      });
  }
}
