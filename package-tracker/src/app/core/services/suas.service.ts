// ─────────────────────────────────────────────
// src/app/core/services/suas.service.ts
// ─────────────────────────────────────────────
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { SUAS } from '../../models';
 
@Injectable({ providedIn: 'root' })
export class SuasService {
  private apiUrl = 'http://localhost:5000/api/suas';
  constructor(private http: HttpClient) {}
 
  private mockSuas: SUAS[] = [
    { suasId: 1, status: 'IN_TRANSIT', currentDepotId: 2, rangeKm: 16, packageOnBoard: 1 },
    { suasId: 2, status: 'IDLE',       currentDepotId: 1, rangeKm: 16 },
    { suasId: 3, status: 'IDLE',       currentDepotId: 4, rangeKm: 16 },
  ];
 
  getAllSuas(): Observable<SUAS[]> {
    return of(this.mockSuas);
    // REAL: return this.http.get<SUAS[]>(this.apiUrl);
  }
 
  getSuasById(id: number): Observable<SUAS> {
    return of(this.mockSuas.find(s => s.suasId === id)!);
    // REAL: return this.http.get<SUAS>(`${this.apiUrl}/${id}`);
  }
 
  manualDispatch(suasId: number, fromDepotId: number, toDepotId: number): Observable<void> {
    return of(void 0);
    // REAL: return this.http.post<void>(`${this.apiUrl}/dispatch`, { suasId, fromDepotId, toDepotId });
  }
}