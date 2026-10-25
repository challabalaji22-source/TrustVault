
import { Component, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { finalize } from 'rxjs';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  email = '';
  password = '';
  message = '';
  isLoading = false;

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  login(): void {
    this.message = '';

    if (!this.email.trim() || !this.password) {
      this.message = 'Please enter your email and password.';
      return;
    }

    this.isLoading = true;

    this.http.post<any>(
      'http://localhost:5201/login?useCookies=false',
      {
        email: this.email.trim(),
        password: this.password
      }
    ).pipe(
      finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (response) => {
        if (response.accessToken) {
          sessionStorage.setItem('trustvault_token', response.accessToken);
          this.router.navigate(['/dashboard']);
        } else {
          this.message = 'Login response received, but no access token was returned.';
        }
      },
      error: (error) => {
        if (error.status === 0) {
          this.message = 'Cannot connect to the API. Check that the backend is running.';
        } else if (error.status === 401) {
          this.message = 'Invalid email or password.';
        } else {
          this.message = `Login failed. HTTP status: ${error.status}`;
        }
      }
    });
  }
}
