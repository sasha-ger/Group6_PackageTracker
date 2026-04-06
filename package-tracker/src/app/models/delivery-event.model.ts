// ─────────────────────────────────────────────
// src/app/models/delivery-event.model.ts
// ─────────────────────────────────────────────
export interface DeliveryEvent {
    eventId: number;
    packageId: number;
    eventType: DeliveryEventType;
    timestamp: Date;
    location: string;
    suasId?: number;
    depotId?: number;
    notes?: string;
  }
   
  export type DeliveryEventType =
    | 'CREATED'
    | 'SUAS_DISPATCHED'
    | 'PICKED_UP'
    | 'DEPOT_HANDOFF'
    | 'OUT_FOR_DELIVERY'
    | 'DELIVERED'
    | 'CANCELLED';