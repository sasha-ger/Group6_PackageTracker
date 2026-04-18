import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DeliveryService } from '../../core/services/delivery.service';
import { Package } from '../../models';

@Component({
  selector: 'app-all-packages',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './all-packages.html',
  styleUrl: './all-packages.scss'
})
export class AllPackages implements OnInit {
  packages: Package[] = [];
  filtered: Package[] = [];
  searchTerm = '';
  selectedStatus = 'ALL';

  statuses = ['ALL', 'PENDING', 'IN_TRANSIT', 'DELIVERED', 'CANCELLED'];

  constructor(private deliveryService: DeliveryService) {}

  ngOnInit() {
    this.deliveryService.getAllDeliveries().subscribe(data => {
      this.packages = data;
      this.filtered = data;
    });
  }

  applyFilter() {
    this.filtered = this.packages.filter(p => {
      const matchesSearch =
        p.origin.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        p.destination.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        p.packageId.toString().includes(this.searchTerm);
      const matchesStatus =
        this.selectedStatus === 'ALL' || p.status === this.selectedStatus;
      return matchesSearch && matchesStatus;
    });
  }

  get totalCount() { return this.packages.length; }
  get inTransitCount() { return this.packages.filter(p => p.status === 'IN_TRANSIT').length; }
  get deliveredCount() { return this.packages.filter(p => p.status === 'DELIVERED').length; }
  get pendingCount() { return this.packages.filter(p => p.status === 'PENDING').length; }
}