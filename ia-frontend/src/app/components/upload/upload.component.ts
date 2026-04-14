import { Component } from '@angular/core';
import { InvoiceService } from '../../services/invoice.service';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './upload.component.html',
  styleUrl: './upload.component.css'
})
export class UploadComponent {
  file!: File;
  result: any;
  loading = false;

  constructor(private invoiceService: InvoiceService) {}

  onFileSelected(event: any) {
    this.file = event.target.files[0];
  }

  upload() {
    if (!this.file) return;

    this.loading = true;

    this.invoiceService.uploadInvoice(this.file).subscribe(res => {
      this.result = res;
      this.loading = false;
    });
  }
}
