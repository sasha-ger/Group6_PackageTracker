import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Package, PackageEvent } from '../../models';
import { API_BASE } from '../api.config';

export interface DeliveryRequest {
  originAddress: string;
  originLat: number;
  originLng: number;
  destinationAddress: string;
  destinationLat: number;
  destinationLng: number;
  recipient: string;
}

@Injectable({
  providedIn: 'root'
})
export class DeliveryService {

  constructor(private http: HttpClient) {}

  getAllDeliveries(): Observable<Package[]> {
    return this.http.get<Package[]>(`${API_BASE}/staff/packages/active`);
  }

  getDeliveriesForCustomer(customerId: number): Observable<Package[]> {
    return this.http.get<Package[]>(`${API_BASE}/customer/${customerId}/packages`);
  }

  getDeliveryById(packageId: number): Observable<Package> {
    return this.http.get<Package>(`${API_BASE}/customer/package/${packageId}`);
  }

  getPackageEvents(packageId: number): Observable<PackageEvent[]> {
    return this.http.get<PackageEvent[]>(`${API_BASE}/customer/package/${packageId}/events`);
  }

  createDeliveryRequest(request: DeliveryRequest): Observable<string> {
    return this.http.post(`${API_BASE}/customer/request`, request, { responseType: 'text' as const });
  }
}
