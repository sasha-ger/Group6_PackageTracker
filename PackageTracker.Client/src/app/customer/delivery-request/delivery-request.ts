import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DeliveryService } from '../../core/services/delivery.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-delivery-request',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './delivery-request.html',
  styleUrl: './delivery-request.scss',
})
export class DeliveryRequestComponent {
  pickupLat: number | null = null;
  pickupLng: number | null = null;
  destLat: number | null = null;
  destLng: number | null = null;

  error = '';
  success = '';

  constructor(
    private deliveryService: DeliveryService,
    private auth: AuthService
  ) {}

  submitRequest() {
    const userId = this.auth.getUserIdFromToken();

    if (!userId) {
      this.error = 'You must be logged in to submit a delivery request.';
      return;
    }

    if (
      this.pickupLat === null ||
      this.pickupLng === null ||
      this.destLat === null ||
      this.destLng === null
    ) {
      this.error = 'Please fill out all latitude and longitude fields.';
      return;
    }

    this.error = '';
    this.success = '';

    this.deliveryService.createDeliveryRequest({
      customerId: userId,
      pickupLat: this.pickupLat,
      pickupLng: this.pickupLng,
      destLat: this.destLat,
      destLng: this.destLng
    }).subscribe({
      next: (pkg) => {
        this.success = `Delivery request submitted! New package ID: ${pkg.packageId}`;

        this.pickupLat = null;
        this.pickupLng = null;
        this.destLat = null;
        this.destLng = null;
      },
      error: () => {
        this.error = 'Failed to submit delivery request.';
      }
    });
  }
}




