import { Routes } from '@angular/router';
import { Dashboard } from './dashboard';
import { Login } from './login/login';

export const routes: Routes = [
  { path: '', component: Login, pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'dashboard', component: Dashboard },
  { path: '**', redirectTo: '' }
];