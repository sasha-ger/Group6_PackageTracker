// ─────────────────────────────────────────────
// src/app/models/package.model.ts
// ─────────────────────────────────────────────
export interface Package {
    packageId: number;
    customerId: number;
    status: PackageStatus;
    weight: number;
    origin: string;
    destination: string;
    currentDepotId?: number;
    currentSuasId?: number;
    createdAt: Date;
  }
   
  export type PackageStatus =
    | 'PENDING'
    | 'DISPATCHED'
    | 'IN_TRANSIT'
    | 'AT_DEPOT'
    | 'DELIVERED'
    | 'CANCELLED';