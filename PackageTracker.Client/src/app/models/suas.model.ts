export interface SUAS {
  id: number;
  status: SuasStatus;
  homeDepotId: number;
  currentDepotId: number | null;
  currentPackageId: number | null;
  destinationDepotId: number | null;
  estimatedArrivalTime: string | null;
}

export type SuasStatus =
  | 'Idle'
  | 'EnRouteToPickup'
  | 'InTransit'
  | 'Charging'
  | 'Maintenance';
