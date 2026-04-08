import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';
  isStaff = false;

  constructor(private auth: AuthService) {}

  login() {
    this.auth.login(this.email, this.password, this.isStaff).subscribe({
      next: () => {
        if (this.isStaff) {
          window.location.href = '/staff/dashboard';
        } else {
          window.location.href = '/customer/my-deliveries';
        }
      },
      error: () => {
        this.error = 'Invalid email or password';
      }
    });
  }
}