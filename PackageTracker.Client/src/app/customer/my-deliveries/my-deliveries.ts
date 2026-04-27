import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeliveryService } from '../../core/services/delivery.service';
import { AuthService } from '../../core/services/auth.service';
import { Package } from '../../models';

@Component({
  selector: 'app-my-deliveries',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-deliveries.html',
  styleUrl: './my-deliveries.scss',
})
export class MyDeliveriesComponent implements OnInit {

  packages: Package[] = [];
  error = '';

  constructor(
    private deliveryService: DeliveryService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    const userId = this.authService.getUserIdFromToken();
    this.deliveryService.getDeliveriesForCustomer(userId).subscribe({
      next: data => this.packages = data,
      error: () => this.error = 'Failed to load deliveries.'
    });
  }

  goToRequest() {
    window.location.href = '/customer/delivery-request';
  }

  track(id: number) {
    window.location.href = `/customer/tracking/${id}`;
  }
}
