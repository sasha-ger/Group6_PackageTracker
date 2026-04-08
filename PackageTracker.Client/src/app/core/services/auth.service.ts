import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private http: HttpClient) {}

  login(email: string, password: string, isStaff: boolean = false): Observable<any> {
    const fakeResponse = {
      token: 'fake-jwt-token',
      userId: 1,
      role: isStaff ? 'STAFF' : 'CUSTOMER'
    };
    return of(fakeResponse).pipe(
      tap(res => {
        localStorage.setItem('token', res.token);
        localStorage.setItem('role', res.role);
      })
    );
  }

  getUserIdFromToken(): number {
    return 1;
  }

  isStaff(): boolean {
    return localStorage.getItem('role') === 'STAFF';
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('role');
  }
}