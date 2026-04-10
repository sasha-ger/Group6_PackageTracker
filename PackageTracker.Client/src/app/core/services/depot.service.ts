import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Depot } from '../../models';

@Injectable({ providedIn: 'root' })
export class DepotService {

  private mockDepots: Depot[] = [
    // I-80 Corridor
    { depotId: 1,  name: 'Seward',           locationKm: 0,   address: 'Seward, NE',            availableSuas: [1] },
    { depotId: 2,  name: 'Pawnee Lake',       locationKm: 16,  address: 'Pawnee Lake, NE',        availableSuas: [] },
    { depotId: 3,  name: 'Lincoln Northwest', locationKm: 29,  address: 'Lincoln NW, NE',         availableSuas: [2] },
    { depotId: 4,  name: 'Waverly',           locationKm: 42,  address: 'Waverly, NE',            availableSuas: [] },
    { depotId: 5,  name: 'Greenwood',         locationKm: 55,  address: 'Greenwood, NE',          availableSuas: [3] },
    { depotId: 6,  name: 'Melia',             locationKm: 68,  address: 'Melia, NE',              availableSuas: [] },
    { depotId: 7,  name: 'Millard',           locationKm: 81,  address: 'Millard, NE',            availableSuas: [] },
    { depotId: 8,  name: 'Omaha',             locationKm: 94,  address: 'Omaha, NE',              availableSuas: [] },
    // Lincoln City
    { depotId: 9,  name: '27th and O',        locationKm: 35,  address: 'O St & 27th St, Lincoln', availableSuas: [] },
    { depotId: 10, name: '84th and O',        locationKm: 41,  address: 'O St & 84th St, Lincoln', availableSuas: [] },
    { depotId: 11, name: '84th and Hwy 2',    locationKm: 43,  address: '84th St & Hwy 2, Lincoln', availableSuas: [] },
  ];

  getAllDepots(): Observable<Depot[]> {
    return of(this.mockDepots);
  }

  getDepotById(id: number): Observable<Depot> {
    return of(this.mockDepots.find(d => d.depotId === id)!);
  }
}