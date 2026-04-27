export interface PackageEvent {
  eventType: PackageEventType;
  timestamp: string;
  depotId: number | null;
  depotName: string | null;
}

export type PackageEventType =
  | 'Dispatched'
  | 'PickedUp'
  | 'ArrivedAtDepot'
  | 'Delivered';
