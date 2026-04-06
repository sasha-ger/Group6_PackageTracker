// ─────────────────────────────────────────────
// src/app/core/services/depot.service.ts
// ─────────────────────────────────────────────
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Depot } from '../../models';
 
@Injectable({ providedIn: 'root' })
export class DepotService {
  private apiUrl = 'http://localhost:5000/api/depots';
  constructor(private http: HttpClient) {}
 
  private mockDepots: Depot[] = [
    { depotId: 1, name: 'Seward Depot',          locationKm: 0,   address: 'Seward, NE',               availableSuas: [2] },
    { depotId: 2, name: 'Lincoln O & 27th',       locationKm: 40,  address: 'O St & 27th St, Lincoln',  availableSuas: [] },
    { depotId: 3, name: 'Lincoln O & 84th',       locationKm: 46,  address: 'O St & 84th St, Lincoln',  availableSuas: [1] },
    { depotId: 4, name: 'Lincoln 84th & Hwy 2',   locationKm: 48,  address: '84th St & NE-2, Lincoln',  availableSuas: [3] },
  ];
 
  getAllDepots(): Observable<Depot[]> {
    return of(this.mockDepots);
    // REAL: return this.http.get<Depot[]>(this.apiUrl);
  }
 
  getDepotById(id: number): Observable<Depot> {
    return of(this.mockDepots.find(d => d.depotId === id)!);
    // REAL: return this.http.get<Depot>(`${this.apiUrl}/${id}`);
  }
}