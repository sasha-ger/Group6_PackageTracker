import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';
import { DeliveryService } from '../../core/services/delivery.service';
import { Package, PackageEvent } from '../../models';

@Component({
  selector: 'app-tracking',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tracking.html',
  styleUrl: './tracking.scss',
})
export class TrackingComponent implements OnInit {

  pkg: Package | null = null;
  events: PackageEvent[] = [];
  error = '';
  isLoading = true;

  constructor(
    private route: ActivatedRoute,
    private deliveryService: DeliveryService
  ) {}

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!id) {
      this.error = 'No package ID provided.';
      this.isLoading = false;
      return;
    }

    forkJoin({
      pkg: this.deliveryService.getDeliveryById(id),
      events: this.deliveryService.getPackageEvents(id)
    }).subscribe({
      next: ({ pkg, events }) => {
        this.pkg = pkg;
        this.events = events;
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Could not load tracking information.';
        this.isLoading = false;
      }
    });
  }

  goBack() {
    window.location.href = '/customer/my-deliveries';
  }
}
