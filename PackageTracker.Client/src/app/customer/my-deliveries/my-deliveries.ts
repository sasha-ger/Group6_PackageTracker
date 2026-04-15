import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-my-deliveries',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-deliveries.html',
  styleUrl: './my-deliveries.scss',
})
export class MyDeliveriesComponent {

  // Backend-correct dummy data
  packages = [
    {
      id: 1,
      trackingNumber: 'PKG-20240415-001',
      status: 'Pending',
      originLocation: {
        address: '123 Main St, Lincoln NE',
        latitude: 40.81,
        longitude: -96.70
      },
      destinationLocation: {
        address: '500 Vine St, Lincoln NE',
        latitude: 40.82,
        longitude: -96.72
      },
      currentDepotName: 'Lincoln – O & 27th',
      createdAt: new Date('2024-04-15T09:30:00')
    },
    {
      id: 2,
      trackingNumber: 'PKG-20240415-002',
      status: 'InTransit',
      originLocation: {
        address: '84th & Hwy 2, Lincoln NE',
        latitude: 40.73,
        longitude: -96.60
      },
      destinationLocation: {
        address: '72nd & Dodge, Omaha NE',
        latitude: 41.26,
        longitude: -96.04
      },
      currentDepotName: 'Omaha – 72nd & Dodge',
      createdAt: new Date('2024-04-14T14:10:00')
    },
    {
      id: 3,
      trackingNumber: 'PKG-20240415-003',
      status: 'Delivered',
      originLocation: {
        address: 'Seward Depot',
        latitude: 40.90,
        longitude: -97.09
      },
      destinationLocation: {
        address: 'Grand Island Depot',
        latitude: 40.92,
        longitude: -98.34
      },
      currentDepotName: 'Delivered to Destination',
      createdAt: new Date('2024-04-10T11:45:00')
    }
  ];

  goToRequest() {
    window.location.href = '/customer/delivery-request';
  }

  track(id: number) {
    window.location.href = `/customer/tracking/${id}`;
  }
}
