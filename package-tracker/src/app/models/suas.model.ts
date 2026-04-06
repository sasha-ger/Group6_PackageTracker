// ─────────────────────────────────────────────
// src/app/models/suas.model.ts
// ─────────────────────────────────────────────
export interface SUAS {
    suasId: number;
    status: SuasStatus;
    currentDepotId: number;
    rangeKm: number;
    packageOnBoard?: number; // packageId or null
  }
   
  export type SuasStatus =
  | 'IDLE'
  | 'IN_TRANSIT'
  | 'EN_ROUTE_PICKUP'
  | 'EN_ROUTE_DEPOT'
  | 'EN_ROUTE_DELIVERY'
  | 'MAINTENANCE';