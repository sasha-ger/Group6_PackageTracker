import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DeliveryService } from '../../core/services/delivery.service';

@Component({
  selector: 'app-delivery-request',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './delivery-request.html',
  styleUrl: './delivery-request.scss',
})
export class DeliveryRequestComponent {

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

  recipient = '';

  error = '';
  success = '';
  isSubmitting = false;

  constructor(private deliveryService: DeliveryService) {}

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

    if (!this.recipient.trim()) {
      this.error = 'Please enter a recipient name.';
      return;
    }

    this.isSubmitting = true;

    this.deliveryService.createDeliveryRequest({
      originAddress: this.pickup.address,
      originLat: this.pickup.latitude,
      originLng: this.pickup.longitude,
      destinationAddress: this.destination.address,
      destinationLat: this.destination.latitude,
      destinationLng: this.destination.longitude,
      recipient: this.recipient.trim()
    }).subscribe({
      next: msg => {
        this.success = msg || 'Delivery request submitted successfully.';
        this.isSubmitting = false;
      },
      error: err => {
        this.error = typeof err?.error === 'string' ? err.error : 'Failed to submit delivery request.';
        this.isSubmitting = false;
      }
    });
  }
}
