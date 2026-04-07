import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeliveryService } from '../../core/services/delivery.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-my-deliveries',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-deliveries.html',
  styleUrl: './my-deliveries.scss',
})
export class MyDeliveriesComponent implements OnInit {
  deliveries: any[] = [];
  error = '';

  constructor(
    private deliveryService: DeliveryService,
    private auth: AuthService
  ) {}

  ngOnInit() {
    const userId = this.auth.getUserIdFromToken();

    if (!userId) {
      this.error = 'You must be logged in to view your deliveries.';
      return;
    }

    this.deliveryService.getPackagesByCustomer(userId).subscribe({
      next: (data) => {
        this.deliveries = data;
      },
      error: () => {
        this.error = 'Failed to load deliveries.';
      }
    });
  }
}



// import { Component } from '@angular/core';

// @Component({
//   selector: 'app-my-deliveries',
//   imports: [],
//   templateUrl: './my-deliveries.html',
//   styleUrl: './my-deliveries.scss',
// })
// export class MyDeliveries {}
