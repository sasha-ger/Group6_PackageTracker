import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { PackageEventService } from '../../core/services/package-event.service';

@Component({
  selector: 'app-tracking',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tracking.html',
  styleUrl: './tracking.scss',
})
export class TrackingComponent implements OnInit {
  packageId: number | null = null;
  status: any = null;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private packageEvents: PackageEventService
  ) {}

  ngOnInit() {
    this.packageId = Number(this.route.snapshot.queryParamMap.get('packageId'));

    if (!this.packageId) {
      this.error = 'No package ID provided.';
      return;
    }

    this.packageEvents.getPackageStatus(this.packageId).subscribe({
      next: (result: any) => {
        this.status = result;
      },
      error: () => {
        this.error = 'Failed to load package status.';
      }
    });
  }

  goBack() {
    window.location.href = '/customer/my-deliveries';
  }
}
