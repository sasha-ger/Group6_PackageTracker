import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SuasService } from '../../core/services/suas.service';
import { DepotService } from '../../core/services/depot.service';
import { SUAS, Depot } from '../../models';

@Component({
  selector: 'app-drone-tracker',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './drone-tracker.html',
  styleUrl: './drone-tracker.scss'
})
export class DroneTracker implements OnInit {
  suas: SUAS[] = [];
  depots: Depot[] = [];
  selectedSuas: SUAS | null = null;

  constructor(
    private suasService: SuasService,
    private depotService: DepotService
  ) {}

  ngOnInit() {
    this.suasService.getAllSuas().subscribe(data => this.suas = data);
    this.depotService.getAllDepots().subscribe(data => this.depots = data);
  }

  getDepotName(depotId: number): string {
    return this.depots.find(d => d.depotId === depotId)?.name ?? `Depot ${depotId}`;
  }

  selectSuas(s: SUAS) {
    this.selectedSuas = this.selectedSuas?.suasId === s.suasId ? null : s;
  }

  get idleCount() { return this.suas.filter(s => s.status === 'IDLE').length; }
  get activeCount() { return this.suas.filter(s => s.status !== 'IDLE').length; }
}