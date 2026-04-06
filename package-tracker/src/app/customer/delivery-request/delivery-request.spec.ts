import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeliveryRequest } from './delivery-request';

describe('DeliveryRequest', () => {
  let component: DeliveryRequest;
  let fixture: ComponentFixture<DeliveryRequest>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeliveryRequest],
    }).compileComponents();

    fixture = TestBed.createComponent(DeliveryRequest);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
