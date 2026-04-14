import { Routes } from '@angular/router';
import { UploadComponent } from './components/upload/upload.component';

export const routes: Routes = [
  { path: '', redirectTo: 'upload', pathMatch: 'full' },
  { path: 'upload', component: UploadComponent }
];
