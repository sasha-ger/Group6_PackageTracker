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
  
  export const DEPOT_MAP: Record<number, string> = {
  1: 'Lincoln – O & 27th',
  2: 'Lincoln – 84th & Hwy 2',
  3: 'Omaha – 72nd & Dodge',
  4: 'Seward Depot',
  5: 'Grand Island Depot'
};
