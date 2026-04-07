import { Routes } from '@angular/router';

const routes: Routes = [
  { path: '', redirectTo: 'customer/login', pathMatch: 'full' },

  {
    path: 'customer',
    children: [
      {
        path: 'login',
        loadComponent: () =>
          import('./customer/login/login').then(m => m.LoginComponent)
      },
      {
        path: 'tracking',
        loadComponent: () =>
          import('./customer/tracking/tracking').then(m => m.TrackingComponent)
      },
      {
        path: 'delivery-request',
        loadComponent: () =>
          import('./customer/delivery-request/delivery-request').then(m => m.DeliveryRequestComponent)
      },
      {
        path: 'my-deliveries',
        loadComponent: () =>
          import('./customer/my-deliveries/my-deliveries').then(m => m.MyDeliveriesComponent)
      }
    ]
  }
];

export default routes;
