import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { API_BASE } from '../api.config';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = `${API_BASE}/auth`;

  constructor(private http: HttpClient) {}

  login(email: string, password: string, isStaff: boolean = false): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, { email, password }).pipe(
      tap(res => {
        localStorage.setItem('token', res.token);
        // Get role from token after login
        this.getRoleFromToken(res.token).subscribe(roleRes => {
          localStorage.setItem('role', roleRes.role);
        });
      })
    );
  }

  getRoleFromToken(token: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/role?token=${token}`);
  }

  validateToken(token: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/validate?token=${token}`);
  }

  logout(): void {
    const token = this.getToken();
    if (token) {
      this.http.post(`${this.apiUrl}/logout`, {}).subscribe();
    }
    localStorage.removeItem('token');
    localStorage.removeItem('role');
  }

  isStaff(): boolean { return localStorage.getItem('role') === 'Staff'; }
  isLoggedIn(): boolean { return !!localStorage.getItem('token'); }
  getToken(): string | null { return localStorage.getItem('token'); }

  getUserIdFromToken(): number {
    const token = this.getToken();
    if (!token) return 0;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const sub = payload.sub ?? payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
      return Number(sub) || 0;
    } catch {
      return 0;
    }
  }
}