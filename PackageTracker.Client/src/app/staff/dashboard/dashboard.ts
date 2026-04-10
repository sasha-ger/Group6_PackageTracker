import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SuasService } from '../../core/services/suas.service';
import { DepotService } from '../../core/services/depot.service';
import { DeliveryService } from '../../core/services/delivery.service';
import { ThemeService } from '../../core/services/theme';
import { SUAS, Depot, Package } from '../../models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class DashboardComponent implements OnInit {
  suas: SUAS[] = [];
  depots: Depot[] = [];
  packages: Package[] = [];

  totalDrones = 0;
  idleDrones = 0;
  activeDrones = 0;
  totalPackages = 0;
  inTransitPackages = 0;
  deliveredPackages = 0;

  constructor(
    private suasService: SuasService,
    private depotService: DepotService,
    private deliveryService: DeliveryService,
    private themeService: ThemeService
  ) {}

  isDark() { return this.themeService.isDark(); }
  toggleTheme() { this.themeService.toggle(); }

  ngOnInit() {
    this.suasService.getAllSuas().subscribe(data => {
      this.suas = data;
      this.totalDrones = data.length;
      this.idleDrones = data.filter(s => s.status === 'IDLE').length;
      this.activeDrones = data.filter(s => s.status !== 'IDLE').length;
    });

    this.depotService.getAllDepots().subscribe(data => {
      this.depots = data;
    });

    this.deliveryService.getAllDeliveries().subscribe(data => {
      this.packages = data;
      this.totalPackages = data.length;
      this.inTransitPackages = data.filter(p => p.status === 'IN_TRANSIT').length;
      this.deliveredPackages = data.filter(p => p.status === 'DELIVERED').length;
    });
  }
}