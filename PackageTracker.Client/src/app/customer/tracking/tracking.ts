import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-tracking',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tracking.html',
  styleUrl: './tracking.scss',
})
export class TrackingComponent {

  pkg: any = null;
  lastEvent: any = null;
  error = '';

  // Dummy backend-correct data
  dummyPackages = [
    {
      id: 1,
      trackingNumber: 'PKG-20240415-001',
      status: 'Pending',
      originLocation: { address: '123 Main St, Lincoln NE' },
      destinationLocation: { address: '500 Vine St, Lincoln NE' },
      updatedAt: new Date('2024-04-15T10:00:00'),
      events: [
        { eventType: 'Dispatched', depotName: 'Lincoln – O & 27th' }
      ]
    },
    {
      id: 2,
      trackingNumber: 'PKG-20240415-002',
      status: 'InTransit',
      originLocation: { address: '84th & Hwy 2, Lincoln NE' },
      destinationLocation: { address: '72nd & Dodge, Omaha NE' },
      updatedAt: new Date('2024-04-15T11:20:00'),
      events: [
        { eventType: 'PickedUp', depotName: 'Lincoln – 84th & Hwy 2' },
        { eventType: 'ArrivedAtDepot', depotName: 'Omaha – 72nd & Dodge' }
      ]
    },
    {
      id: 3,
      trackingNumber: 'PKG-20240415-003',
      status: 'Delivered',
      originLocation: { address: 'Seward Depot' },
      destinationLocation: { address: 'Grand Island Depot' },
      updatedAt: new Date('2024-04-14T16:45:00'),
      events: [
        { eventType: 'Delivered', depotName: 'Grand Island Depot' }
      ]
    }
  ];

  constructor(private route: ActivatedRoute) {}

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!id) {
      this.error = 'No package ID provided.';
      return;
    }

    const found = this.dummyPackages.find(p => p.id === id);

    if (!found) {
      this.error = 'Package not found.';
      return;
    }

    this.pkg = found;
    this.lastEvent = found.events[found.events.length - 1];
  }

  goBack() {
    window.location.href = '/customer/my-deliveries';
  }
}
