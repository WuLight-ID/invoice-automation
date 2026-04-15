import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class InvoiceService {
  private RenderUrl = 'https://invoice-automation-uebm.onrender.com';
  private apiUrl = this.RenderUrl + '/api/invoice/upload';

  constructor(private http: HttpClient) {}

  uploadInvoice(file: File) {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post(this.apiUrl, formData);
  }
}
