import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyDeliveries } from './my-deliveries';

describe('MyDeliveries', () => {
  let component: MyDeliveries;
  let fixture: ComponentFixture<MyDeliveries>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyDeliveries],
    }).compileComponents();

    fixture = TestBed.createComponent(MyDeliveries);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
