import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DeliveryService } from '../../core/services/delivery.service';

@Component({
  selector: 'app-delivery-request',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './delivery-request.html',
  styleUrl: './delivery-request.scss',
})
export class DeliveryRequestComponent {
  origin = '';
  destination = '';
  submitted = false;
  error = '';

  constructor(private deliveryService: DeliveryService) {}

  submitRequest() {
    if (!this.origin || !this.destination) {
      this.error = 'Please fill out both fields.';
      return;
    }

    this.error = '';

    const request = {
      originLocation: this.origin,
      destinationLocation: this.destination
    };

    this.deliveryService.createDeliveryRequest(request).subscribe({
      next: () => {
        this.submitted = true;
      },
      error: () => {
        this.error = 'Failed to submit request.';
      }
    });
  }
}



// import { Component } from '@angular/core';

// @Component({
//   selector: 'app-delivery-request',
//   imports: [],
//   templateUrl: './delivery-request.html',
//   styleUrl: './delivery-request.scss',
// })
// export class DeliveryRequest {}
