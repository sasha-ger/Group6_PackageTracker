// ─────────────────────────────────────────────
// src/app/core/services/delivery.service.ts
// ─────────────────────────────────────────────
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Package, PackageStatus } from '../../models';
 
@Injectable({ providedIn: 'root' })
export class DeliveryService {
  private apiUrl = 'http://localhost:5000/api/deliveries';
 
  constructor(private http: HttpClient) {}
 
  // ── MOCK DATA ─────────────────────────────────────────
  private mockPackages: Package[] = [
    {
      packageId: 1, customerId: 1, status: 'IN_TRANSIT',
      weight: 2.5, origin: 'Lincoln - O & 27th',
      destination: 'Omaha - 84th & Nebraska Hwy 2',
      currentDepotId: 3, createdAt: new Date('2026-04-01')
    },
    {
      packageId: 2, customerId: 1, status: 'DELIVERED',
      weight: 1.0, origin: 'Seward Depot',
      destination: 'Lincoln - O & 84th',
      currentDepotId: undefined, createdAt: new Date('2026-03-28')
    }
  ];
 
  requestDelivery(pkg: Partial<Package>): Observable<Package> {
    // MOCK — remove when API ready
    const newPkg: Package = { ...pkg, packageId: 99, status: 'PENDING', createdAt: new Date() } as Package;
    return of(newPkg);
    // REAL: return this.http.post<Package>(this.apiUrl, pkg);
  }
 
  trackPackage(packageId: number): Observable<Package> {
    // MOCK
    return of(this.mockPackages.find(p => p.packageId === packageId)!);
    // REAL: return this.http.get<Package>(`${this.apiUrl}/${packageId}`);
  }
 
  getMyDeliveries(customerId: number): Observable<Package[]> {
    // MOCK
    return of(this.mockPackages.filter(p => p.customerId === customerId));
    // REAL: return this.http.get<Package[]>(`${this.apiUrl}/customer/${customerId}`);
  }
 
  getAllDeliveries(): Observable<Package[]> {
    // MOCK
    return of(this.mockPackages);
    // REAL: return this.http.get<Package[]>(this.apiUrl);
  }
 
  cancelDelivery(packageId: number): Observable<void> {
    return of(void 0);
    // REAL: return this.http.delete<void>(`${this.apiUrl}/${packageId}`);
  }
}
