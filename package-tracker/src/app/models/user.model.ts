// ─────────────────────────────────────────────
// src/app/models/user.model.ts
// ─────────────────────────────────────────────
export interface User {
    userId: number;
    name: string;
    email: string;
    role: UserRole;
  }
   
  export type UserRole = 'CUSTOMER' | 'STAFF';
   
  export interface AuthResponse {
    token: string;
    user: User;
  }
   
  export interface LoginRequest {
    email: string;
    password: string;
  }