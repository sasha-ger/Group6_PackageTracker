import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-delivery-request',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './delivery-request.html',
  styleUrl: './delivery-request.scss',
})
export class DeliveryRequestComponent {

  // Dummy depot data (matches backend Depot + Location)
  depots = [
    { id: 1, name: 'Lincoln – O & 27th', location: { latitude: 40.8001, longitude: -96.6678 }},
    { id: 2, name: 'Lincoln – 84th & Hwy 2', location: { latitude: 40.7372, longitude: -96.6044 }},
    { id: 3, name: 'Omaha – 72nd & Dodge', location: { latitude: 41.2625, longitude: -96.0461 }},
    { id: 4, name: 'Seward Depot', location: { latitude: 40.9070, longitude: -97.0989 }},
    { id: 5, name: 'Grand Island Depot', location: { latitude: 40.9263, longitude: -98.3420 }}
  ];

  pickup = {
    address: '',
    latitude: null as number | null,
    longitude: null as number | null
  };

  destination = {
    address: '',
    latitude: null as number | null,
    longitude: null as number | null
  };

  closestDepot: any = null;

  error = '';
  success = '';

  calculateClosestDepot() {
    if (this.pickup.latitude == null || this.pickup.longitude == null) return;

    let best = null;
    let bestDist = Infinity;

    for (const depot of this.depots) {
      const d = Math.sqrt(
        Math.pow(depot.location.latitude - this.pickup.latitude, 2) +
        Math.pow(depot.location.longitude - this.pickup.longitude, 2)
      );

      if (d < bestDist) {
        bestDist = d;
        best = depot;
      }
    }

    this.closestDepot = best;
  }

  submitRequest() {
    this.error = '';
    this.success = '';

    if (!this.pickup.address || !this.destination.address) {
      this.error = 'Please enter both addresses.';
      return;
    }

    if (
      this.pickup.latitude == null ||
      this.pickup.longitude == null ||
      this.destination.latitude == null ||
      this.destination.longitude == null
    ) {
      this.error = 'Please enter all latitude and longitude fields.';
      return;
    }

    this.calculateClosestDepot();

    if (!this.closestDepot) {
      this.error = 'Could not determine closest depot.';
      return;
    }

    // Dummy success message
    this.success = `Delivery request created! Closest depot: ${this.closestDepot.name}`;
  }
}
