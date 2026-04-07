import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PackageEventService } from '../../core/services/package-event.service';

@Component({
  selector: 'app-tracking',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './tracking.html',
  styleUrl: './tracking.scss',
})
export class TrackingComponent {
  packageId!: number;
  status: any = null;
  error = '';

  constructor(private packageEvents: PackageEventService) {}

  track() {
    if (!this.packageId) {
      this.error = 'Please enter a package ID.';
      return;
    }

    this.error = '';
    this.packageEvents.getPackageStatus(this.packageId).subscribe({
      next: (result) => {
        this.status = result;
      },
      error: () => {
        this.error = 'Package not found.';
        this.status = null;
      }
    });
  }
}



// import { Component } from '@angular/core';

// @Component({
//   selector: 'app-tracking',
//   imports: [],
//   templateUrl: './tracking.html',
//   styleUrl: './tracking.scss',
// })
// export class Tracking {}
