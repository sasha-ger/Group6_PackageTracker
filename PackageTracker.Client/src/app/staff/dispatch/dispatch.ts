import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SuasService } from '../../core/services/suas.service';
import { DepotService } from '../../core/services/depot.service';
import { SUAS, Depot } from '../../models';

@Component({
  selector: 'app-dispatch',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dispatch.html',
  styleUrl: './dispatch.scss'
})
export class Dispatch implements OnInit {
  suas: SUAS[] = [];
  depots: Depot[] = [];

  selectedSuasId: number | null = null;
  selectedFromDepotId: number | null = null;
  selectedToDepotId: number | null = null;

  successMessage = '';
  errorMessage = '';
  isSubmitting = false;

  constructor(
    private suasService: SuasService,
    private depotService: DepotService
  ) {}

  ngOnInit() {
    this.suasService.getAllSuas().subscribe(data => {
      // Only show idle drones for dispatch
      this.suas = data.filter(s => s.status === 'Idle');
    });

    this.depotService.getAllDepots().subscribe(data => {
      this.depots = data;
    });
  }

  getDepotName(depotId: number): string {
    return this.depots.find(d => d.depotId === depotId)?.name ?? `Depot ${depotId}`;
  }

  get availableToDepots(): Depot[] {
    return this.depots.filter(d => d.depotId !== this.selectedFromDepotId);
  }

  onSuasSelect() {
    const selected = this.suas.find(s => s.id === Number(this.selectedSuasId));
    if (selected) {
      this.selectedFromDepotId = selected.currentDepotId;
    }
    this.selectedToDepotId = null;
    this.successMessage = '';
    this.errorMessage = '';
  }

  canDispatch(): boolean {
    return !!(this.selectedSuasId && this.selectedFromDepotId && this.selectedToDepotId);
  }

  dispatch() {
    if (!this.canDispatch()) return;
    this.isSubmitting = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.suasService.manualDispatch(
      Number(this.selectedSuasId),
      Number(this.selectedFromDepotId),
      Number(this.selectedToDepotId)
    ).subscribe({
      next: () => {
        const suasName = `SUAS-${this.selectedSuasId}`;
        const fromName = this.getDepotName(Number(this.selectedFromDepotId));
        const toName = this.getDepotName(Number(this.selectedToDepotId));
        this.successMessage = `${suasName} successfully dispatched from ${fromName} to ${toName}.`;
        this.isSubmitting = false;
        this.selectedSuasId = null;
        this.selectedFromDepotId = null;
        this.selectedToDepotId = null;
      },
      error: () => {
        this.errorMessage = 'Dispatch failed. Please try again.';
        this.isSubmitting = false;
      }
    });
  }
}