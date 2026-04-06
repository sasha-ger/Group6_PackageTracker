// ─────────────────────────────────────────────
// src/app/models/depot.model.ts
// ─────────────────────────────────────────────
export interface Depot {
    depotId: number;
    name: string;
    locationKm: number;   // distance along I-80 from reference point
    address: string;
    availableSuas: number[]; // list of suasIds currently idle here
  }