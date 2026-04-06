// ─────────────────────────────────────────────
// src/app/core/services/auth.service.ts
// ─────────────────────────────────────────────
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { User, AuthResponse, LoginRequest } from '../../models';
 
@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/auth';
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSubject.asObservable();
 
  constructor(private http: HttpClient) {
    // Restore user from localStorage on app load
    const stored = localStorage.getItem('currentUser');
    if (stored) this.currentUserSubject.next(JSON.parse(stored));
  }
 
  login(credentials: LoginRequest): Observable<AuthResponse> {
    // ── MOCK (remove when API is ready) ──────────────────
    const mockResponse: AuthResponse = {
      token: 'mock-jwt-token',
      user: { userId: 1, name: 'Jane Doe', email: credentials.email, role: 'CUSTOMER' }
    };
    return of(mockResponse).pipe(tap(res => this.storeSession(res)));
    // ── REAL (uncomment when API is ready) ───────────────
    // return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials)
    //   .pipe(tap(res => this.storeSession(res)));
  }
 
  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }
 
  isLoggedIn(): boolean { return !!localStorage.getItem('token'); }
  isStaff(): boolean { return this.currentUserSubject.value?.role === 'STAFF'; }
  getToken(): string | null { return localStorage.getItem('token'); }
 
  private storeSession(res: AuthResponse): void {
    localStorage.setItem('token', res.token);
    localStorage.setItem('currentUser', JSON.stringify(res.user));
    this.currentUserSubject.next(res.user);
  }
}