// ─────────────────────────────────────────────
// src/app/core/services/package-event.service.ts
// ─────────────────────────────────────────────
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { DeliveryEvent } from '../../models';
 
@Injectable({ providedIn: 'root' })
export class PackageEventService {
  private apiUrl = 'http://localhost:5000/api/events';
  constructor(private http: HttpClient) {}
 
  private mockEvents: DeliveryEvent[] = [
    { eventId: 1, packageId: 1, eventType: 'CREATED',         timestamp: new Date('2026-04-01T09:00'), location: 'Lincoln - O & 27th' },
    { eventId: 2, packageId: 1, eventType: 'SUAS_DISPATCHED',  timestamp: new Date('2026-04-01T09:05'), location: 'Lincoln - O & 27th', suasId: 1 },
    { eventId: 3, packageId: 1, eventType: 'PICKED_UP',        timestamp: new Date('2026-04-01T09:20'), location: 'Lincoln - O & 27th', suasId: 1 },
    { eventId: 4, packageId: 1, eventType: 'DEPOT_HANDOFF',    timestamp: new Date('2026-04-01T09:45'), location: 'Lincoln - O & 84th', depotId: 3 },
  ];
 
  getEventsByPackage(packageId: number): Observable<DeliveryEvent[]> {
    return of(this.mockEvents.filter(e => e.packageId === packageId));
    // REAL: return this.http.get<DeliveryEvent[]>(`${this.apiUrl}/package/${packageId}`);
  }
}